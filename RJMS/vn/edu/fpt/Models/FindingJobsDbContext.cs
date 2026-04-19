using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RJMS.vn.edu.fpt.Models;

public partial class FindingJobsDbContext : DbContext
{
    public FindingJobsDbContext()
    {
    }

    public FindingJobsDbContext(DbContextOptions<FindingJobsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Candidate> Candidates { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Cv> Cvs { get; set; }
    public virtual DbSet<CvTemplate> CvTemplates { get; set; }
    public virtual DbSet<TemplateCategory> TemplateCategories { get; set; }
    public virtual DbSet<CvData> CvDataSet { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobCategory> JobCategories { get; set; }

    public virtual DbSet<JobSkill> JobSkills { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlanFeature> PlanFeatures { get; set; }

    public virtual DbSet<Recruiter> Recruiters { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SubscriptionPlanOption> SubscriptionPlanOptions { get; set; }

    public virtual DbSet<SubscriptionPeriod> SubscriptionPeriods { get; set; }

    public virtual DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }

    public virtual DbSet<SavedJob> SavedJobs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<CompanyFollower> CompanyFollowers { get; set; }
    public virtual DbSet<SavedJob> SavedJobs { get; set; }
    public virtual DbSet<AppNotification> Notifications { get; set; }
    public virtual DbSet<NotificationReference> NotificationReferences { get; set; }
    
    // Chat & Messaging
    public virtual DbSet<Conversation> Conversations { get; set; }
    public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageRead> MessageReads { get; set; }
    public virtual DbSet<MessageAttachment> MessageAttachments { get; set; }
    public virtual DbSet<ConversationJob> ConversationJobs { get; set; }
    public virtual DbSet<CompanyLocation> CompanyLocations { get; set; }
    public virtual DbSet<RecruiterLocation> RecruiterLocations { get; set; }
    public virtual DbSet<JobRecruiter> JobRecruiters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var congfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if(!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(congfig.GetConnectionString("DBContext"));
        }
}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC072D030AC9");

            entity.Property(e => e.CoverLetter).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Cvid).HasColumnName("CVId");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Applications)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Candi__6E01572D");

            entity.HasOne(d => d.Cv).WithMany(p => p.Applications)
                .HasForeignKey(d => d.Cvid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicatio__CVId__6EF57B66");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobId__6D0D32F4");
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Candidat__3214EC07AE15A529");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CurrentSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExpectedSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsLookingForJob).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Summary).HasColumnType("text");
            entity.Property(e => e.Title).HasMaxLength(1000);

            entity.HasOne(d => d.User).WithMany(p => p.Candidates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Candidate__UserI__5165187F");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Companie__3214EC0756E23280");

            entity.Property(e => e.Benefits).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CompanySize).HasMaxLength(50);
            entity.Property(e => e.CoverImage).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(200);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.TaxCode).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(500);
            // Note: ProvinceCode/ProvinceName/WardCode/WardName/Address removed - now in CompanyLocations
        });

        modelBuilder.Entity<Cv>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CVs__3214EC0781BB1121");
            entity.ToTable("CVs");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.CvType).HasMaxLength(20).HasDefaultValue("UPLOAD");
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.LegacyFilePath).HasMaxLength(500).HasColumnName("LegacyFilePath");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);

            entity.HasOne(d => d.Candidate).WithMany(p => p.Cvs)
                .HasForeignKey(d => d.CandidateId)
                .HasConstraintName("FK__CVs__CandidateId__693CA210");

            entity.HasOne(d => d.Template).WithMany()
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_CVs_Template")
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.CvData).WithOne(p => p.Cv)
                .HasForeignKey<CvData>(d => d.CvId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CvTemplate>(entity =>
        {
            entity.ToTable("CvTemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category)
                  .WithMany(p => p.CvTemplates)
                  .HasForeignKey(d => d.CategoryId)
                  .HasConstraintName("FK_CvTemplates_Category");
        });

        modelBuilder.Entity<TemplateCategory>(entity =>
        {
            entity.ToTable("TemplateCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CvData>(entity =>
        {
            entity.ToTable("CvData");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JsonData).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Jobs__3214EC072AAD8B7F");

            entity.Property(e => e.ApplicationCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.JobType).HasMaxLength(50);
            entity.Property(e => e.MaxSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Requirements).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Benefits).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.PublishDate).HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Jobs__CompanyId__5CD6CB2B");

            entity.HasOne(d => d.JobCategory).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobCategoryId)
                .HasConstraintName("FK__Jobs__JobCategor__5EBF139D");
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobCateg__3214EC07DB38AAF5");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__JobCateg__Parent__XYZ");
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.SkillId }).HasName("PK__JobSkill__689C99DA7AECB0EE");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Job).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JobSkills__JobId__6383C8BA");

            entity.HasOne(d => d.Skill).WithMany(p => p.JobSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK__JobSkills__Skill__6477ECF3");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC07EAA4C6B7");

            entity.Property(e => e.CityName).HasMaxLength(100);
            entity.Property(e => e.WardName).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.DetailAddress).HasMaxLength(500);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07C5F62577");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Subscr__787EE5A0");
        });

        modelBuilder.Entity<Recruiter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Recruite__3214EC072F662ABB");

            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Recruiters)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK__Recruiter__Compa__4CA06362");

            entity.HasOne(d => d.User).WithMany(p => p.Recruiters)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Recruiter__UserI__4BAC3F29");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC072D714566");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6207833B3").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skills__3214EC0747BFC952");

            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07BAC16FC4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.SubscribedPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubscribedBillingCycle).HasMaxLength(20);
            entity.Property(e => e.SubscribedPlanName).HasMaxLength(200);

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__PlanI__75A278F5");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__UserI__74AE54BC");

            entity.HasOne(d => d.Company).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Subscriptions_Companies");

            entity.HasOne(d => d.PlanOption).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanOptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Subscriptions_PlanOptions");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07847E2993");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<SubscriptionPlanOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SubscriptionPlanOptions");

            entity.HasIndex(e => new { e.PlanId, e.BillingCycle })
                .IsUnique()
                .HasDatabaseName("UX_SubscriptionPlanOptions_Plan_Cycle");

            entity.Property(e => e.BillingCycle).HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanOptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SubscriptionPlanOptions_SubscriptionPlans");
        });

        modelBuilder.Entity<SubscriptionUsage>(entity =>
        {
            entity.ToTable("SubscriptionUsage");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeatureCode).HasMaxLength(100);
            
            entity.HasOne(d => d.Period)
                .WithMany(p => p.SubscriptionUsages)
                .HasForeignKey(d => d.PeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.ToTable("SavedJobs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CandidateId, e.JobId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Candidate)
                .WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Job)
                .WithMany(p => p.SavedJobs)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC072889622F");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349C22B116").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EmailConfirmed).HasDefaultValue(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760ADF3AA5EFA");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__4222D4EF");
        });

        modelBuilder.Entity<CompanyFollower>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.UserId });
            entity.Property(p => p.FollowedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Company)
                .WithMany(p => p.Followers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User)
                .WithMany(p => p.FollowingCompanies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppNotification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationReference>(entity =>
        {
            entity.ToTable("NotificationReferences");
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Notification)
                .WithMany(p => p.References)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(d => d.Sender)
                .WithMany()
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MessageRead>(entity =>
        {
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompanyLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasIndex(e => new { e.CompanyId, e.LocationId }).IsUnique();

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyLocations)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Location).WithMany(p => p.CompanyLocations)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RecruiterLocation>(entity =>
        {
            entity.HasKey(e => new { e.RecruiterId, e.CompanyLocationId });
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Recruiter).WithMany(p => p.RecruiterLocations)
                .HasForeignKey(d => d.RecruiterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CompanyLocation).WithMany(p => p.RecruiterLocations)
                .HasForeignKey(d => d.CompanyLocationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<JobRecruiter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.RecruiterId });
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsPrimary).HasDefaultValue(true);

            entity.HasOne(d => d.Job).WithMany(p => p.JobRecruiters)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Recruiter).WithMany(p => p.JobRecruiters)
                .HasForeignKey(d => d.RecruiterId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.CompanyLocation).WithMany(p => p.JobRecruiters)
                .HasForeignKey(d => d.CompanyLocationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
