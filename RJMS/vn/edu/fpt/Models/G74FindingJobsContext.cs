using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RJMS.Models;

public partial class G74FindingJobsContext : DbContext
{
    public G74FindingJobsContext()
    {
    }

    public G74FindingJobsContext(DbContextOptions<G74FindingJobsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<CandidateSkill> CandidateSkills { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyAddress> CompanyAddresses { get; set; }

    public virtual DbSet<CompanyImage> CompanyImages { get; set; }

    public virtual DbSet<Cv> Cvs { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<FollowedCompany> FollowedCompanies { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobCategory> JobCategories { get; set; }

    public virtual DbSet<JobSkill> JobSkills { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Recruiter> Recruiters { get; set; }

    public virtual DbSet<SavedJob> SavedJobs { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<WorkExperience> WorkExperiences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =localhost; database = G74-Finding-Jobs;uid=sa;pwd=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Admins_UserId").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Admin).HasForeignKey<Admin>(d => d.UserId);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasIndex(e => e.Cvid, "IX_Applications_CVId");

            entity.HasIndex(e => e.CandidateId, "IX_Applications_CandidateId");

            entity.HasIndex(e => e.JobId, "IX_Applications_JobId");

            entity.HasIndex(e => e.Status, "IX_Applications_Status");

            entity.Property(e => e.CoverLetter).HasColumnType("text");
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.ReviewNotes).HasColumnType("text");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Cv).WithMany(p => p.Applications)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Candidates_UserId").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CurrentPosition).HasMaxLength(100);
            entity.Property(e => e.CurrentSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.ExpectedSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HighestDegree).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Summary).HasColumnType("text");
            entity.Property(e => e.Title).HasMaxLength(1000);
            entity.Property(e => e.WorkingType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Candidate).HasForeignKey<Candidate>(d => d.UserId);
        });

        modelBuilder.Entity<CandidateSkill>(entity =>
        {
            entity.HasKey(e => new { e.CandidateId, e.SkillId });

            entity.HasIndex(e => e.SkillId, "IX_CandidateSkills_SkillId");

            entity.Property(e => e.Level).HasMaxLength(50);

            entity.HasOne(d => d.Candidate).WithMany(p => p.CandidateSkills).HasForeignKey(d => d.CandidateId);

            entity.HasOne(d => d.Skill).WithMany(p => p.CandidateSkills).HasForeignKey(d => d.SkillId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.TaxCode, "IX_Companies_TaxCode")
                .IsUnique()
                .HasFilter("([TaxCode] IS NOT NULL)");

            entity.Property(e => e.Benefits).HasColumnType("text");
            entity.Property(e => e.CompanySize).HasMaxLength(50);
            entity.Property(e => e.CoverImage).HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(200);
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.TaxCode).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(500);
        });

        modelBuilder.Entity<CompanyAddress>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_CompanyAddresses_CompanyId");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AddressType).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Ward).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyAddresses).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<CompanyImage>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_CompanyImages_CompanyId");

            entity.Property(e => e.Caption).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyImages).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Cv>(entity =>
        {
            entity.ToTable("CVs");

            entity.HasIndex(e => e.CandidateId, "IX_CVs_CandidateId");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.JsonData).HasColumnType("text");
            entity.Property(e => e.TemplateId).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Cvs).HasForeignKey(d => d.CandidateId);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasIndex(e => e.CandidateId, "IX_Educations_CandidateId");

            entity.Property(e => e.Degree).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("GPA");
            entity.Property(e => e.Major).HasMaxLength(255);
            entity.Property(e => e.School).HasMaxLength(255);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Educations).HasForeignKey(d => d.CandidateId);
        });

        modelBuilder.Entity<FollowedCompany>(entity =>
        {
            entity.HasKey(e => new { e.CandidateId, e.CompanyId });

            entity.HasIndex(e => e.CompanyId, "IX_FollowedCompanies_CompanyId");

            entity.HasOne(d => d.Candidate).WithMany(p => p.FollowedCompanies).HasForeignKey(d => d.CandidateId);

            entity.HasOne(d => d.Company).WithMany(p => p.FollowedCompanies).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasIndex(e => e.CompanyAddressId, "IX_Jobs_CompanyAddressId");

            entity.HasIndex(e => e.CompanyId, "IX_Jobs_CompanyId");

            entity.HasIndex(e => e.CreatedAt, "IX_Jobs_CreatedAt");

            entity.HasIndex(e => e.JobCategoryId, "IX_Jobs_JobCategoryId");

            entity.HasIndex(e => e.LocationId, "IX_Jobs_LocationId");

            entity.HasIndex(e => e.RecruiterId, "IX_Jobs_RecruiterId");

            entity.HasIndex(e => e.Status, "IX_Jobs_Status");

            entity.Property(e => e.Benefits).HasColumnType("text");
            entity.Property(e => e.DegreeRequired).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Gender).HasMaxLength(100);
            entity.Property(e => e.JobType).HasMaxLength(100);
            entity.Property(e => e.Level).HasMaxLength(100);
            entity.Property(e => e.MaxSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Requirements).HasColumnType("text");
            entity.Property(e => e.SalaryCurrency).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.WorkingType).HasMaxLength(100);

            entity.HasOne(d => d.CompanyAddress).WithMany(p => p.Jobs).HasForeignKey(d => d.CompanyAddressId);

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.JobCategory).WithMany(p => p.Jobs).HasForeignKey(d => d.JobCategoryId);

            entity.HasOne(d => d.Location).WithMany(p => p.Jobs).HasForeignKey(d => d.LocationId);

            entity.HasOne(d => d.Recruiter).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.RecruiterId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillId });

            entity.HasIndex(e => e.SkillId, "IX_JobSkills_SkillId");

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkills).HasForeignKey(d => d.JobId);

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkills).HasForeignKey(d => d.SkillId);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(e => e.CityName).HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.Link).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.SubscriptionId, "IX_Payments_SubscriptionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments).HasForeignKey(d => d.SubscriptionId);
        });

        modelBuilder.Entity<Recruiter>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Recruiters_CompanyId");

            entity.HasIndex(e => e.UserId, "IX_Recruiters_UserId").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.VerificationDocument).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.Recruiters).HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.User).WithOne(p => p.Recruiter).HasForeignKey<Recruiter>(d => d.UserId);
        });

        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.HasKey(e => new { e.CandidateId, e.JobId });

            entity.HasIndex(e => e.JobId, "IX_SavedJobs_JobId");

            entity.HasOne(d => d.Candidate).WithMany(p => p.SavedJobs).HasForeignKey(d => d.CandidateId);

            entity.HasOne(d => d.Job).WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasIndex(e => e.PlanId, "IX_Subscriptions_PlanId");

            entity.HasIndex(e => e.SubscriptionPlanId, "IX_Subscriptions_SubscriptionPlanId");

            entity.HasIndex(e => e.UserId, "IX_Subscriptions_UserId");

            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Plan).WithMany(p => p.SubscriptionPlans)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SubscriptionPlan).WithMany(p => p.SubscriptionSubscriptionPlans).HasForeignKey(d => d.SubscriptionPlanId);

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasIndex(e => e.CandidateId, "IX_WorkExperiences_CandidateId");

            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Position).HasMaxLength(255);

            entity.HasOne(d => d.Candidate).WithMany(p => p.WorkExperiences).HasForeignKey(d => d.CandidateId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
