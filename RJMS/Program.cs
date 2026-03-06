using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FindingJobsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext"))
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<ICandidateDashboardRepository, CandidateDashboardRepository>();
builder.Services.AddScoped<ICandidateDashboardService, CandidateDashboardService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

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

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
