using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using RJMS.vn.edu.fpt.Middleware;
using RJMS.vn.edu.fpt.Jobs;
using Hangfire;
using Hangfire.SqlServer;


var builder = WebApplication.CreateBuilder(args);

// Configure to run on port 5000 (HTTP) and 5001 (HTTPS)
builder.WebHost.UseUrls("https://localhost:5000", "https://localhost:5001");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDbContext<FindingJobsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext"))
);

// ── Hangfire (Recurring Job for renewals) ──
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DBContext")));

builder.Services.AddHangfireServer();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<ICVRepository, CVRepository>();
builder.Services.AddScoped<ICVService, CVService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IJobCategoryService, JobCategoryService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICVRenderService, CVRenderService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<SubscriptionRenewalJob>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

var app = builder.Build();

// Seed roles and default accounts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FindingJobsDbContext>();

    // Ensure roles exist
    var roleNames = new[] { "Admin", "Candidate", "Recruiter" };
    foreach (var roleName in roleNames)
    {
        if (!db.Roles.Any(r => r.Name == roleName))
        {
            db.Roles.Add(new Role
            {
                Name = roleName,
                Description = $"{roleName} role",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
    db.SaveChanges();

    // Seed one account per role
    var seeds = new[]
    {
        new { Email = "admin@rjms.com",     Password = "Admin@123456",     RoleName = "Admin",     FirstName = "System", LastName = "Admin" },
        new { Email = "candidate@rjms.com", Password = "Candidate@123456", RoleName = "Candidate", FirstName = "Demo",   LastName = "Candidate" },
        new { Email = "recruiter@rjms.com", Password = "Recruiter@123456", RoleName = "Recruiter", FirstName = "Demo",   LastName = "Recruiter" },
    };

    foreach (var seed in seeds)
    {
        if (!db.Users.Any(u => u.Email == seed.Email))
        {
            var user = new User
            {
                Email = seed.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                FirstName = seed.FirstName,
                LastName = seed.LastName,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            db.SaveChanges();

            var role = db.Roles.First(r => r.Name == seed.RoleName);
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }

    // Seed Free Subscription Plan for Recruiters
    const string freePlanName = "Gói Miễn Phí";
    if (!db.SubscriptionPlans.Any(sp => sp.Name == freePlanName))
    {
        var freePlan = new SubscriptionPlan
        {
            Name = freePlanName,
            Price = 0,
            DurationDays = 30,
            Description = "Gói miễn phí dành cho nhà tuyển dụng mới. Cho phép đăng 3 tin tuyển dụng mỗi tháng, không bao gồm tính năng lọc CV bằng AI.",
            IsActive = true,
            BillingCycle = "Monthly",
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };
        db.SubscriptionPlans.Add(freePlan);
        db.SaveChanges();

        // Add plan features
        db.PlanFeatures.Add(new PlanFeature
        {
            PlanId = freePlan.Id,
            FeatureCode = "JOB_POSTING",
            FeatureLimit = 3 // 3 bài đăng mỗi tháng
        });
        db.PlanFeatures.Add(new PlanFeature
        {
            PlanId = freePlan.Id,
            FeatureCode = "CV_AI_FILTER",
            FeatureLimit = 0 // Không được lọc CV bằng AI
        });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ── Quota Check Middleware (Intersects job creation/CV parsing) ──
app.UseMiddleware<QuotaMiddleware>();

app.UseAuthorization();

// ── Hangfire Dashboard & Setup ──
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Important: Secure this in production. For now, allow internal access.
});

// Schedule the daily renewal job (runs at 01:00 AM every day)
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<SubscriptionRenewalJob>(
        "yearly-period-renewal",
        job => job.Execute(),
        Cron.Daily(1) 
    );
}

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<RJMS.vn.edu.fpt.Hubs.NotificationHub>("/notificationHub");
app.MapHub<RJMS.vn.edu.fpt.Hubs.ChatHub>("/chatHub");

app.Run();
