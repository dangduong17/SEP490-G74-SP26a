using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly FindingJobsDbContext _db;

        public ManagerRepository(FindingJobsDbContext db)
        {
            _db = db;
        }

        public async Task<ManagerPeriodData> GetDashboardPeriodDataAsync(int days)
        {
            var endDate = DateTimeHelper.NowVietnam.Date;
            var startDate = endDate.AddDays(-(days - 1));
            var endExclusive = endDate.AddDays(1);

            var paymentEvents = await _db.Payments
                .AsNoTracking()
                .Where(p => p.Status == "SUCCESS" && p.PaymentDate != null && p.PaymentDate >= startDate && p.PaymentDate < endExclusive)
                .Select(p => new PaymentEvent(
                    p.PaymentDate!.Value,
                    p.Amount ?? 0,
                    p.Subscription.SubscribedPlanName
                    ?? p.Subscription.Plan.Name
                    ?? "Gói khác"))
                .ToListAsync();

            var jobEvents = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.CreatedAt != null && j.CreatedAt >= startDate && j.CreatedAt < endExclusive)
                .Select(j => j.CreatedAt!.Value)
                .ToListAsync();

            var revenueByPlan = paymentEvents
                .GroupBy(p => p.PlanName)
                .Select(g => new ManagerCategoryAmount { Label = g.Key, Value = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Value)
                .ToList();

            if (revenueByPlan.Count == 0)
            {
                revenueByPlan.Add(new ManagerCategoryAmount { Label = "Chưa có dữ liệu", Value = 0 });
            }

            var data = new ManagerPeriodData
            {
                TotalRevenue = paymentEvents.Sum(p => p.Amount),
                TotalJobPosts = jobEvents.Count,
                RevenueByPlan = revenueByPlan
            };

            if (days <= 7)
            {
                BuildDailySeries(data, startDate, days, paymentEvents, jobEvents);
            }
            else if (days <= 30)
            {
                BuildWeeklySeries(data, startDate, 4, paymentEvents, jobEvents);
            }
            else
            {
                BuildMonthlySeries(data, startDate, 3, paymentEvents, jobEvents);
            }

            return data;
        }

        public async Task<ManagerPeriodData> GetDashboardRangeDataAsync(DateTime from, DateTime to)
        {
            var startDate = from.Date;
            var endExclusive = to.Date.AddDays(1);

            var paymentEvents = await _db.Payments
                .AsNoTracking()
                .Where(p => p.Status == "SUCCESS" && p.PaymentDate != null && p.PaymentDate >= startDate && p.PaymentDate < endExclusive)
                .Select(p => new PaymentEvent(
                    p.PaymentDate!.Value,
                    p.Amount ?? 0,
                    p.Subscription.SubscribedPlanName ?? p.Subscription.Plan.Name ?? "Gói khác"))
                .ToListAsync();

            var jobEvents = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.CreatedAt != null && j.CreatedAt >= startDate && j.CreatedAt < endExclusive)
                .Select(j => j.CreatedAt!.Value)
                .ToListAsync();

            var revenueByPlan = paymentEvents
                .GroupBy(p => p.PlanName)
                .Select(g => new ManagerCategoryAmount { Label = g.Key, Value = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Value)
                .ToList();

            if (revenueByPlan.Count == 0)
                revenueByPlan.Add(new ManagerCategoryAmount { Label = "Chưa có dữ liệu", Value = 0 });

            var data = new ManagerPeriodData
            {
                TotalRevenue = paymentEvents.Sum(p => p.Amount),
                TotalJobPosts = jobEvents.Count,
                RevenueByPlan = revenueByPlan
            };

            var totalDays = (to.Date - from.Date).Days + 1;
            var totalMonths = (to.Year - from.Year) * 12 + to.Month - from.Month + 1;

            if (totalDays <= 31)
            {
                BuildDailySeries(data, startDate, totalDays, paymentEvents, jobEvents);
            }
            else if (totalMonths <= 3)
            {
                var weeks = (int)Math.Ceiling(totalDays / 7.0);
                BuildWeeklySeries(data, startDate, weeks, paymentEvents, jobEvents);
            }
            else
            {
                BuildMonthlySeries(data, startDate, totalMonths, paymentEvents, jobEvents);
            }

            return data;
        }

        public async Task<List<ManagerActivityItem>> GetRecentActivitiesAsync(int take)
        {
            var jobActivities = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.CreatedAt != null)
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new ManagerActivityItem
                {
                    Time = j.CreatedAt!.Value,
                    TimeLabel = j.CreatedAt!.Value.ToString("dd/MM HH:mm"),
                    Type = "Đăng tin",
                    Actor = j.Company.Name ?? "Công ty",
                    Detail = j.Title ?? "Tin tuyển dụng",
                    StatusLabel = "Hoạt động",
                    StatusClass = "status-active"
                })
                .Take(take)
                .ToListAsync();

            var paymentActivities = await _db.Payments
                .AsNoTracking()
                .Where(p => p.Status == "SUCCESS" && p.PaymentDate != null)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new ManagerActivityItem
                {
                    Time = p.PaymentDate!.Value,
                    TimeLabel = p.PaymentDate!.Value.ToString("dd/MM HH:mm"),
                    Type = "Thanh toán",
                    Actor = p.Subscription.Company != null
                        ? p.Subscription.Company.Name
                        : (p.Subscription.User.Email ?? "Khách hàng"),
                    Detail = $"Gói {p.Subscription.SubscribedPlanName ?? p.Subscription.Plan.Name ?? "Dịch vụ"}",
                    StatusLabel = "Thành công",
                    StatusClass = "status-success"
                })
                .Take(take)
                .ToListAsync();

            var userActivities = await _db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.User.CreatedAt != null)
                .Where(ur => ur.Role.Name == "Candidate" || ur.Role.Name == "Recruiter")
                .OrderByDescending(ur => ur.User.CreatedAt)
                .Select(ur => new ManagerActivityItem
                {
                    Time = ur.User.CreatedAt!.Value,
                    TimeLabel = ur.User.CreatedAt!.Value.ToString("dd/MM HH:mm"),
                    Type = "Đăng ký",
                    Actor = (ur.User.FirstName + " " + ur.User.LastName).Trim(),
                    Detail = ur.Role.Name == "Candidate" ? "Ứng viên mới đăng ký" : "Nhà tuyển dụng mới đăng ký",
                    StatusLabel = "Đang xử lý",
                    StatusClass = "status-pending"
                })
                .Take(take)
                .ToListAsync();

            var combined = jobActivities
                .Concat(paymentActivities)
                .Concat(userActivities)
                .OrderByDescending(x => x.Time)
                .Take(take)
                .ToList();

            return combined;
        }

        public async Task<List<ManagerAlertItem>> GetAlertItemsAsync()
        {
            var now = DateTimeHelper.NowVietnam;
            var lastDay = now.AddDays(-1);
            var nextWeek = now.AddDays(7);
            var lastWeek = now.AddDays(-7);

            var bannedJobs = await _db.Jobs
                .AsNoTracking()
                .Where(j => j.IsBanned && j.BannedAt != null && j.BannedAt >= lastDay)
                .CountAsync();

            var expiringSubs = await _db.Subscriptions
                .AsNoTracking()
                .Where(s => s.Status == "ACTIVE" && s.EndDate != null && s.EndDate >= now && s.EndDate <= nextWeek)
                .CountAsync();

            var newUsers = await _db.Users
                .AsNoTracking()
                .Where(u => u.CreatedAt != null && u.CreatedAt >= lastWeek)
                .CountAsync();

            return new List<ManagerAlertItem>
            {
                new ManagerAlertItem
                {
                    Style = "danger",
                    Message = $"{bannedJobs} tin tuyển dụng bị khóa trong 24h"
                },
                new ManagerAlertItem
                {
                    Style = "warning",
                    Message = $"{expiringSubs} nhà tuyển dụng sắp hết gói trong 7 ngày"
                },
                new ManagerAlertItem
                {
                    Style = "success",
                    Message = $"{newUsers} tài khoản mới trong 7 ngày"
                }
            };
        }

        private static void BuildDailySeries(
            ManagerPeriodData data,
            DateTime startDate,
            int days,
            List<PaymentEvent> paymentEvents,
            List<DateTime> jobEvents)
        {
            var labels = new List<string>();
            var jobCounts = new int[days];

            var planNames = paymentEvents.Select(p => p.PlanName).Distinct().ToList();
            if (planNames.Count == 0) planNames.Add("Chưa có dữ liệu");

            var revenueMap = planNames.ToDictionary(p => p, _ => new decimal[days]);

            for (var i = 0; i < days; i++)
            {
                labels.Add(startDate.AddDays(i).ToString("dd/MM"));
            }

            foreach (var jobDate in jobEvents)
            {
                var index = (jobDate.Date - startDate).Days;
                if (index >= 0 && index < days) jobCounts[index]++;
            }

            foreach (var ev in paymentEvents)
            {
                var index = (ev.Date.Date - startDate).Days;
                if (index < 0 || index >= days) continue;
                revenueMap[ev.PlanName][index] += ev.Amount;
            }

            data.Labels = labels;
            data.JobPostCounts = jobCounts.ToList();
            data.RevenueSeries = revenueMap.Select(kvp => new ManagerSeries
            {
                Label = kvp.Key,
                Values = kvp.Value.ToList()
            }).ToList();
        }

        private static void BuildWeeklySeries(
            ManagerPeriodData data,
            DateTime startDate,
            int weeks,
            List<PaymentEvent> paymentEvents,
            List<DateTime> jobEvents)
        {
            var labels = Enumerable.Range(1, weeks).Select(i => $"Tuần {i}").ToList();
            var jobCounts = new int[weeks];

            var planNames = paymentEvents.Select(p => p.PlanName).Distinct().ToList();
            if (planNames.Count == 0) planNames.Add("Chưa có dữ liệu");
            var revenueMap = planNames.ToDictionary(p => p, _ => new decimal[weeks]);

            foreach (var jobDate in jobEvents)
            {
                var index = Math.Min(weeks - 1, (jobDate.Date - startDate).Days / 7);
                if (index >= 0) jobCounts[index]++;
            }

            foreach (var ev in paymentEvents)
            {
                var index = Math.Min(weeks - 1, (ev.Date.Date - startDate).Days / 7);
                if (index < 0) continue;
                revenueMap[ev.PlanName][index] += ev.Amount;
            }

            data.Labels = labels;
            data.JobPostCounts = jobCounts.ToList();
            data.RevenueSeries = revenueMap.Select(kvp => new ManagerSeries
            {
                Label = kvp.Key,
                Values = kvp.Value.ToList()
            }).ToList();
        }

        private static void BuildMonthlySeries(
            ManagerPeriodData data,
            DateTime startDate,
            int months,
            List<PaymentEvent> paymentEvents,
            List<DateTime> jobEvents)
        {
            var labels = Enumerable.Range(0, months)
                .Select(i => $"Tháng {startDate.AddMonths(i).Month}")
                .ToList();
            var jobCounts = new int[months];

            var planNames = paymentEvents.Select(p => p.PlanName).Distinct().ToList();
            if (planNames.Count == 0) planNames.Add("Chưa có dữ liệu");
            var revenueMap = planNames.ToDictionary(p => p, _ => new decimal[months]);

            foreach (var jobDate in jobEvents)
            {
                var index = (jobDate.Year - startDate.Year) * 12 + jobDate.Month - startDate.Month;
                if (index >= 0 && index < months) jobCounts[index]++;
            }

            foreach (var ev in paymentEvents)
            {
                var index = (ev.Date.Year - startDate.Year) * 12 + ev.Date.Month - startDate.Month;
                if (index < 0 || index >= months) continue;
                revenueMap[ev.PlanName][index] += ev.Amount;
            }

            data.Labels = labels;
            data.JobPostCounts = jobCounts.ToList();
            data.RevenueSeries = revenueMap.Select(kvp => new ManagerSeries
            {
                Label = kvp.Key,
                Values = kvp.Value.ToList()
            }).ToList();
        }

        private sealed class PaymentEvent
        {
            public PaymentEvent(DateTime date, decimal amount, string planName)
            {
                Date = date;
                Amount = amount;
                PlanName = planName;
            }

            public DateTime Date { get; }
            public decimal Amount { get; }
            public string PlanName { get; }
        }
    }
}
