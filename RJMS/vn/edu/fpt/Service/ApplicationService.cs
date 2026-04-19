using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ApplicationService : IApplicationService
    {
        private readonly FindingJobsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ApplicationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ApplicationService(
            FindingJobsDbContext context,
            IEmailService emailService,
            ICloudinaryService cloudinaryService,
            ILogger<ApplicationService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET APPLY MODAL DATA
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApplyModalData?> GetApplyModalDataAsync(int jobId, int userId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return null;

            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null) return null;

            var cvs = await _context.Cvs
                .Where(c => c.CandidateId == candidate.Id)
                .OrderByDescending(c => c.IsDefault)
                .ThenByDescending(c => c.CreatedAt)
                .Select(c => new CvSelectItemDto
                {
                    Id = c.Id,
                    Title = c.Title ?? c.FileName ?? "CV chưa đặt tên",
                    CvType = c.CvType,
                    FileUrl = c.FileUrl,
                    FileName = c.FileName,
                    CreatedAt = c.CreatedAt,
                    IsDefault = c.IsDefault
                })
                .ToListAsync();

            var uploadCount = await _context.Cvs
                .CountAsync(c => c.CandidateId == candidate.Id && c.CvType == "UPLOAD");

            var alreadyApplied = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.CandidateId == candidate.Id);

            return new ApplyModalData
            {
                JobId = jobId,
                JobTitle = job.Title,
                Cvs = cvs,
                UploadCount = uploadCount,
                AlreadyApplied = alreadyApplied
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // APPLY JOB  (Fire & Forget email + notifications)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApplyJobResponse> ApplyJobAsync(
            int jobId, int userId, int cvId, string? coverLetter, IFormFile? uploadFile)
        {
            // --- Auth check ---
            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == userId);
            if (candidate == null)
                return Fail("Không tìm thấy hồ sơ ứng viên.");

            // --- Duplicate check ---
            var alreadyApplied = await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.CandidateId == candidate.Id);
            if (alreadyApplied)
                return Fail("Bạn đã ứng tuyển vị trí này rồi.");

            // --- Job check ---
            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobRecruiters)
                    .ThenInclude(jr => jr.Recruiter)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) return Fail("Không tìm thấy công việc.");
            if (job.Status != "Active") return Fail("Công việc này không còn tuyển dụng.");

            // --- CV resolution ---
            Cv? cv = null;

            if (uploadFile != null && uploadFile.Length > 0)
            {
                // User wants to upload a new CV on-the-fly
                var uploadCount = await _context.Cvs
                    .CountAsync(c => c.CandidateId == candidate.Id && c.CvType == "UPLOAD");

                if (uploadCount >= 3)
                    return Fail("Bạn đã đạt giới hạn 3 CV upload. Vui lòng chọn CV từ thư viện.");

                var allowedExts = new[] { ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                    return Fail("Chỉ hỗ trợ file .pdf, .doc, .docx.");
                if (uploadFile.Length > 10 * 1024 * 1024)
                    return Fail("File không được vượt quá 10MB.");

                var fileUrl = ext == ".pdf"
                    ? await _cloudinaryService.UploadPdfAsImageAsync(uploadFile, "cv-uploads")
                    : await _cloudinaryService.UploadRawAsync(uploadFile, "cv-uploads");
                if (string.IsNullOrEmpty(fileUrl))
                    return Fail("Upload file thất bại. Vui lòng thử lại.");

                cv = new Cv
                {
                    CandidateId = candidate.Id,
                    Title = Path.GetFileNameWithoutExtension(uploadFile.FileName),
                    CvType = "UPLOAD",
                    FileUrl = fileUrl,
                    FileName = uploadFile.FileName,
                    FileSize = (int)uploadFile.Length,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Cvs.Add(cv);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Use existing CV from library
                cv = await _context.Cvs.FirstOrDefaultAsync(c => c.Id == cvId && c.CandidateId == candidate.Id);
                if (cv == null) return Fail("Không tìm thấy CV.");
            }

            // --- Create Application ---
            var application = new Application
            {
                JobId = jobId,
                CandidateId = candidate.Id,
                Cvid = cv.Id,
                CoverLetter = coverLetter,
                Status = "Đang xem xét",
                CreatedAt = DateTime.Now
            };
            _context.Applications.Add(application);

            // --- Increment job application count ---
            job.ApplicationCount = (job.ApplicationCount ?? 0) + 1;

            await _context.SaveChangesAsync();

            // --- Fire and Forget: send emails + create notifications ---
            var user = await _context.Users.FindAsync(userId);
            
            // Extract values to avoid DbContext disposed exception in background task
            int finalJobId = job.Id;
            string finalJobTitle = job.Title ?? "vị trí";
            string finalCompanyName = job.Company?.Name ?? "công ty";
            string candidateName = candidate.FullName ?? "bạn";
            string candidateEmailArg = user?.Email ?? "";
            int candidateUserId = user?.Id ?? candidate.UserId;
            int appIfd = application.Id;
            int? recruiterUserId = job.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.Recruiter?.UserId
                ?? job.JobRecruiters.FirstOrDefault()?.Recruiter?.UserId;
            string targetRecruiterEmail = job.Company?.Email 
                ?? job.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.Recruiter?.User?.Email ?? "";
            
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<FindingJobsDbContext>();
                var hubCtx = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<RJMS.vn.edu.fpt.Hubs.NotificationHub>>();
                var log = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationService>>();

                try
                {
                    await SendEmailsInternal(emailSvc, finalJobTitle, finalCompanyName, candidateName, candidateEmailArg, targetRecruiterEmail);
                    await CreateNotificationsInternal(dbCtx, hubCtx, finalJobTitle, finalCompanyName, candidateName, candidateUserId, recruiterUserId, appIfd);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Lỗi background task apply job");
                }
            });

            return new ApplyJobResponse { Success = true, Message = "Ứng tuyển thành công! Nhà tuyển dụng sẽ liên hệ với bạn sớm." };
        }

        // ─────────────────────────────────────────────────────────────────────
        // NOTIFICATIONS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<List<NotificationDto>> GetNotificationsAsync(int userId, int take = 20)
        {
            return await _context.Set<AppNotification>()
                .Include(n => n.References)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    ReferenceType = n.References.OrderBy(r => r.Id).FirstOrDefault() != null ? n.References.OrderBy(r => r.Id).FirstOrDefault()!.ReferenceType : null,
                    ReferenceId = n.References.OrderBy(r => r.Id).FirstOrDefault() != null ? n.References.OrderBy(r => r.Id).FirstOrDefault()!.ReferenceId : null,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Set<AppNotification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkReadAsync(int notificationId, int userId)
        {
            var notif = await _context.Set<AppNotification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notif == null) return false;
            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task MarkAllReadAsync(int userId)
        {
            var unread = await _context.Set<AppNotification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SendEmailsInternal(IEmailService emailSvc, string jobTitle, string companyName, string candidateName, string candidateEmail, string recruiterEmail)
        {
            if (!string.IsNullOrEmpty(candidateEmail))
            {
                var candidateBody = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
    <div style='background:#00b14f;padding:24px;text-align:center;border-radius:8px 8px 0 0;'>
        <h2 style='color:#fff;margin:0;'>Ứng tuyển thành công!</h2>
    </div>
    <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
        <p style='color:#374151;'>Xin chào <strong>{candidateName}</strong>,</p>
        <p style='color:#374151;'>Bạn đã ứng tuyển thành công vào vị trí <strong>«{jobTitle}»</strong> tại <strong>{companyName}</strong>.</p>
        <p style='color:#374151;'>Nhà tuyển dụng sẽ xem xét hồ sơ và liên hệ với bạn trong thời gian sớm nhất.</p>
        <p style='color:#9ca3af;font-size:12px;margin-top:24px;'>Finding Jobs – Nền tảng tuyển dụng hàng đầu</p>
    </div>
</div>";
                await emailSvc.SendEmailAsync(candidateEmail, $"Xác nhận ứng tuyển – {jobTitle} | Finding Jobs", candidateBody);
            }

            if (!string.IsNullOrEmpty(recruiterEmail))
            {
                var recruiterBody = $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
    <div style='background:#1e40af;padding:24px;text-align:center;border-radius:8px 8px 0 0;'>
        <h2 style='color:#fff;margin:0;'>Có ứng viên mới!</h2>
    </div>
    <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
        <p style='color:#374151;'>Tin tuyển dụng <strong>«{jobTitle}»</strong> vừa nhận được hồ sơ mới.</p>
        <p style='color:#374151;'>Ứng viên: <strong>{candidateName}</strong></p>
        <p style='color:#374151;'>Đăng nhập vào hệ thống để xem chi tiết hồ sơ ứng tuyển.</p>
        <p style='color:#9ca3af;font-size:12px;margin-top:24px;'>Finding Jobs – Nền tảng tuyển dụng hàng đầu</p>
    </div>
</div>";
                await emailSvc.SendEmailAsync(recruiterEmail, $"[Finding Jobs] Có ứng viên mới ứng tuyển – {jobTitle}", recruiterBody);
            }
        }

        private async Task CreateNotificationsInternal(
            FindingJobsDbContext dbCtx, 
            Microsoft.AspNetCore.SignalR.IHubContext<RJMS.vn.edu.fpt.Hubs.NotificationHub> hubCtx,
            string jobTitle, string companyName, string candidateName,
            int candidateUserId, int? recruiterUserId, int applicationId)
        {
            var notifs = new List<AppNotification>();

            // Notification for candidate
            var notifCandidate = new AppNotification
            {
                UserId = candidateUserId,
                Title = "Ứng tuyển thành công",
                Content = $"Bạn đã ứng tuyển vào vị trí «{jobTitle}» tại {companyName}.",
                Type = "APPLICATION",
                IsRead = false,
                CreatedAt = DateTimeHelper.NowVietnam
            };
            notifCandidate.References.Add(new NotificationReference { ReferenceType = "APPLICATION", ReferenceId = applicationId });
            notifs.Add(notifCandidate);

            // Notification for recruiter
            AppNotification? notifRecruiter = null;
            if (recruiterUserId.HasValue)
            {
                notifRecruiter = new AppNotification
                {
                    UserId = recruiterUserId.Value,
                    Title = "Hồ sơ ứng tuyển mới",
                    Content = $"{candidateName} vừa ứng tuyển vào «{jobTitle}».",
                    Type = "APPLICATION",
                    IsRead = false,
                    CreatedAt = DateTimeHelper.NowVietnam
                };
                notifRecruiter.References.Add(new NotificationReference { ReferenceType = "APPLICATION", ReferenceId = applicationId });
                notifs.Add(notifRecruiter);
            }

            dbCtx.Set<AppNotification>().AddRange(notifs);
            await dbCtx.SaveChangesAsync();

            // Real-time broadcast
            await hubCtx.Clients.Group($"User_{candidateUserId}").SendCoreAsync("ReceiveNotification", new object [] { notifCandidate.Title, notifCandidate.Content });

            if (notifRecruiter != null && recruiterUserId.HasValue)
            {
                await hubCtx.Clients.Group($"User_{recruiterUserId.Value}").SendCoreAsync("ReceiveNotification", new object[] { notifRecruiter.Title, notifRecruiter.Content });
            }
        }

        private static ApplyJobResponse Fail(string msg) => new() { Success = false, Message = msg };
    }
}
