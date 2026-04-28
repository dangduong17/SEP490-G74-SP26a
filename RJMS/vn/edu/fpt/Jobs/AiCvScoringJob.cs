using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RJMS.vn.edu.fpt.Hubs;
using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Service;
using System.Text.Json;

namespace RJMS.vn.edu.fpt.Jobs
{
    /// <summary>
    /// Hangfire job: Chấm điểm CV bằng Gemini AI.
    /// Xử lý theo batch 5 CV/lần, cập nhật DB và push SignalR real-time.
    /// Retry 2 lần tự động nếu batch bị lỗi.
    /// </summary>
    public class AiCvScoringJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AiCvScoringJob> _logger;

        public AiCvScoringJob(IServiceScopeFactory scopeFactory, ILogger<AiCvScoringJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Entry point cho Hangfire. Xử lý tất cả Applications của một Job.
        /// </summary>
        /// <param name="jobId">ID của tin tuyển dụng</param>
        /// <param name="recruiterUserId">UserId của nhà tuyển dụng để push SignalR đúng người</param>
        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ProcessJobAsync(int jobId, int recruiterUserId)
        {
            _logger.LogInformation("AiCvScoringJob bắt đầu cho JobId={JobId}, RecruiterUserId={UserId}", jobId, recruiterUserId);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FindingJobsDbContext>();
            var gemini = scope.ServiceProvider.GetRequiredService<IGeminiService>();
            var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            var subscriptionService = scope.ServiceProvider.GetRequiredService<RJMS.Vn.Edu.Fpt.Service.ISubscriptionService>();

            // 1. Lấy thông tin job (title, requirements, skills)
            var job = await db.Jobs
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
            {
                _logger.LogWarning("Không tìm thấy Job {JobId}", jobId);
                return;
            }

            var jobContext = new JobContextDto
            {
                Title = job.Title,
                Requirements = job.Requirements,
                Description = job.Description,
                RequiredSkills = job.JobSkills
                    .Where(js => js.IsRequired == true)
                    .Select(js => js.Skill.Name)
                    .ToList(),
                AllSkills = job.JobSkills
                    .Select(js => js.Skill.Name)
                    .ToList()
            };

            // 2. Lấy tất cả applications cần chấm (Pending hoặc chưa xử lý)
            var applications = await db.Applications
                .Include(a => a.Cv)
                    .ThenInclude(cv => cv.CvData)
                .Where(a => a.JobId == jobId
                    && (a.AiProcessStatus == "Pending" || a.AiProcessStatus == null))
                .OrderBy(a => a.Id)
                .ToListAsync();

            if (applications.Count == 0)
            {
                _logger.LogInformation("Không có application nào cần chấm điểm cho Job {JobId}", jobId);
                return;
            }

            _logger.LogInformation("Tìm thấy {Count} applications cần chấm cho Job {JobId}", applications.Count, jobId);

            // Push thông báo bắt đầu
            await hub.Clients.Group($"User_{recruiterUserId}")
                .SendCoreAsync("AiScoringStarted", new object[] { jobId, applications.Count });

            // 3. Chia thành batches tối đa 5 CV
            var batches = applications
                .Select((app, idx) => new { app, idx })
                .GroupBy(x => x.idx / 5)
                .Select(g => g.Select(x => x.app).ToList())
                .ToList();

            int processedCount = 0;

            foreach (var batch in batches)
            {
                // Set trạng thái Processing
                foreach (var app in batch)
                {
                    app.AiProcessStatus = "Processing";
                }
                await db.SaveChangesAsync();

                try
                {
                    // Xây dựng batch item list
                    var cvBatchItems = batch.Select(app =>
                    {
                        var jsonData = app.Cv?.CvData?.JsonData ?? "{}";
                        return new CvBatchItemDto
                        {
                            ApplicationId = app.Id,
                            JsonData = jsonData
                        };
                    }).ToList();

                    // Gọi Gemini
                    var results = await gemini.ScoreCvBatchAsync(jobContext, cvBatchItems);

                    // Cập nhật kết quả vào DB
                    foreach (var result in results)
                    {
                        var app = batch.FirstOrDefault(a => a.Id == result.Id);
                        if (app == null)
                        {
                            _logger.LogWarning("Gemini trả về id={Id} không tồn tại trong batch", result.Id);
                            continue;
                        }

                        app.AiScore = result.AiScore;
                        app.MatchedSkills = JsonSerializer.Serialize(result.MatchedSkills, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                        app.MissingSkills = JsonSerializer.Serialize(result.MissingSkills, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                        app.Summary = result.Summary;
                        app.AiProcessStatus = "Completed";
                        processedCount++;

                        // Trừ quota CV_AI_FILTER chỉ khi AI trả về điểm hợp lệ (> 0)
                        if (result.AiScore > 0)
                        {
                            try
                            {
                                await subscriptionService.ConsumeQuotaAsync(recruiterUserId, "CV_AI_FILTER");
                            }
                            catch (Exception qEx)
                            {
                                _logger.LogWarning(qEx, "Không thể trừ quota cho recruiterUserId={UserId}, appId={AppId}", recruiterUserId, app.Id);
                            }
                        }

                        // Push SignalR real-time cho từng application
                        await hub.Clients.Group($"User_{recruiterUserId}")
                            .SendCoreAsync("AiScoreUpdated", new object[]
                            {
                                new
                                {
                                    applicationId = app.Id,
                                    aiScore = app.AiScore,
                                    matchedSkills = result.MatchedSkills,
                                    missingSkills = result.MissingSkills,
                                    summary = result.Summary,
                                    processedCount,
                                    totalCount = applications.Count
                                }
                            });
                    }

                    // Xử lý các app trong batch mà Gemini không trả về kết quả
                    foreach (var app in batch)
                    {
                        if (app.AiProcessStatus == "Processing")
                        {
                            app.AiProcessStatus = "Failed";
                            _logger.LogWarning("Application {AppId} không có kết quả từ Gemini, đánh dấu Failed", app.Id);
                        }
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xử lý batch cho Job {JobId}", jobId);

                    // Đánh dấu tất cả app trong batch này là Failed, không làm treo cả queue
                    foreach (var app in batch)
                    {
                        app.AiProcessStatus = "Failed";
                    }
                    await db.SaveChangesAsync();

                    // Push thông báo lỗi batch qua SignalR
                    await hub.Clients.Group($"User_{recruiterUserId}")
                        .SendCoreAsync("AiBatchError", new object[]
                        {
                            new
                            {
                                jobId,
                                batchSize = batch.Count,
                                errorMessage = ex.Message
                            }
                        });

                    // Tiếp tục batch tiếp theo thay vì throw (để không retry toàn bộ job)
                    continue;
                }

                // Delay nhỏ giữa các batch để tránh rate limit
                await Task.Delay(1000);
            }

            _logger.LogInformation("AiCvScoringJob hoàn thành: {Processed}/{Total} cho Job {JobId}",
                processedCount, applications.Count, jobId);

            // Push thông báo hoàn thành
            await hub.Clients.Group($"User_{recruiterUserId}")
                .SendCoreAsync("AiScoringCompleted", new object[]
                {
                    new { jobId, processedCount, totalCount = applications.Count }
                });
        }
    }
}
