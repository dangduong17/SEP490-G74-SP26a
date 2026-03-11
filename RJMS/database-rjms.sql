IF DB_ID(N'G74-Finding-Jobs2') IS NULL
CREATE DATABASE [G74-Finding-Jobs2]
GO

USE [G74-Finding-Jobs2]
GO


/* ================================
ROLES
================================ */
CREATE TABLE Roles
(
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE()
)


/* ================================
USERS
================================ */
CREATE TABLE Users
(
    Id INT IDENTITY PRIMARY KEY,

    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,

    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),

    Phone NVARCHAR(20),
    Avatar NVARCHAR(500),

    IsActive BIT DEFAULT 1,
    EmailConfirmed BIT DEFAULT 0,

    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
)


/* ================================
USER ROLES
================================ */
CREATE TABLE UserRoles
(
    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    AssignedAt DATETIME2 DEFAULT GETDATE(),

    PRIMARY KEY (UserId, RoleId),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
)


/* ================================
COMPANIES
================================ */
CREATE TABLE Companies
(
    Id INT IDENTITY PRIMARY KEY,

    Name NVARCHAR(255) NOT NULL,

    Logo NVARCHAR(500),
    CoverImage NVARCHAR(500),

    TaxCode NVARCHAR(100),
    CompanySize NVARCHAR(50),
    Industry NVARCHAR(200),

    Website NVARCHAR(500),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),

    Description TEXT,
    Benefits TEXT,

    IsVerified BIT DEFAULT 0,
    VerifiedAt DATETIME2,

    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
)


/* ================================
RECRUITERS
================================ */
CREATE TABLE Recruiters
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,
    CompanyId INT,

    FullName NVARCHAR(255),
    Phone NVARCHAR(20),
    Position NVARCHAR(100),

    Avatar NVARCHAR(500),

    IsVerified BIT DEFAULT 0,
    VerifiedAt DATETIME2,

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
)


/* ================================
CANDIDATES
================================ */
CREATE TABLE Candidates
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,

    FullName NVARCHAR(255),
    DateOfBirth DATETIME2,
    Gender NVARCHAR(10),

    Address NVARCHAR(500),
    City NVARCHAR(100),

    Phone NVARCHAR(20),
    Avatar NVARCHAR(500),

    Title NVARCHAR(1000),

    CurrentSalary DECIMAL(18,2),
    ExpectedSalary DECIMAL(18,2),

    YearsOfExperience INT,

    Summary TEXT,

    IsLookingForJob BIT DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id)
)


/* ================================
JOB CATEGORIES
================================ */
CREATE TABLE JobCategories
(
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500)
)


/* ================================
LOCATIONS
================================ */
CREATE TABLE Locations
(
    Id INT IDENTITY PRIMARY KEY,
    CityName NVARCHAR(100) NOT NULL
)


/* ================================
SKILLS
================================ */
CREATE TABLE Skills
(
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100)
)


/* ================================
JOBS
================================ */
CREATE TABLE Jobs
(
    Id INT IDENTITY PRIMARY KEY,

    Title NVARCHAR(500) NOT NULL,

    CompanyId INT NOT NULL,
    RecruiterId INT NOT NULL,

    JobCategoryId INT,
    LocationId INT,

    Description TEXT,
    Requirements TEXT,
    Benefits TEXT,

    JobType NVARCHAR(50),

    MinSalary DECIMAL(18,2),
    MaxSalary DECIMAL(18,2),

    NumberOfPositions INT,

    ApplicationDeadline DATETIME2,
    ExpiryDate DATETIME2,

    Status NVARCHAR(50),

    ViewCount INT DEFAULT 0,
    ApplicationCount INT DEFAULT 0,

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (RecruiterId) REFERENCES Recruiters(Id),
    FOREIGN KEY (JobCategoryId) REFERENCES JobCategories(Id),
    FOREIGN KEY (LocationId) REFERENCES Locations(Id)
)


/* ================================
JOB SKILLS
================================ */
CREATE TABLE JobSkills
(
    JobId INT NOT NULL,
    SkillId INT NOT NULL,

    IsRequired BIT DEFAULT 1,

    PRIMARY KEY (JobId, SkillId),

    FOREIGN KEY (JobId) REFERENCES Jobs(Id) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skills(Id) ON DELETE CASCADE
)


/* ================================
CVS
================================ */
CREATE TABLE CVs
(
    Id INT IDENTITY PRIMARY KEY,

    CandidateId INT NOT NULL,

    Title NVARCHAR(255),
    FilePath NVARCHAR(500),

    ViewCount INT DEFAULT 0,

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (CandidateId) REFERENCES Candidates(Id) ON DELETE CASCADE
)


/* ================================
APPLICATIONS
================================ */
CREATE TABLE Applications
(
    Id INT IDENTITY PRIMARY KEY,

    JobId INT NOT NULL,
    CandidateId INT NOT NULL,
    CVId INT NOT NULL,

    CoverLetter TEXT,

    Status NVARCHAR(50),

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (JobId) REFERENCES Jobs(Id),
    FOREIGN KEY (CandidateId) REFERENCES Candidates(Id),
    FOREIGN KEY (CVId) REFERENCES CVs(Id)
)


/* ================================
SUBSCRIPTION PLANS
================================ */
CREATE TABLE SubscriptionPlans
(
    Id INT IDENTITY PRIMARY KEY,

    Name NVARCHAR(100),
    Price DECIMAL(18,2),

    DurationDays INT,

    Description NVARCHAR(MAX),

    IsActive BIT
)


/* ================================
SUBSCRIPTIONS
================================ */
CREATE TABLE Subscriptions
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,
    PlanId INT NOT NULL,

    StartDate DATETIME2,
    EndDate DATETIME2,

    Status NVARCHAR(50),

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(Id)
)


/* ================================
PAYMENTS
================================ */
CREATE TABLE Payments
(
    Id INT IDENTITY PRIMARY KEY,

    SubscriptionId INT NOT NULL,

    Amount DECIMAL(18,2),

    PaymentDate DATETIME2,

    TransactionId NVARCHAR(100),

    Status NVARCHAR(50),

    PaymentMethod NVARCHAR(50),

    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
)



/* =====================================================
UPDATE TABLE: SubscriptionPlans
===================================================== */

ALTER TABLE SubscriptionPlans
ADD BillingCycle NVARCHAR(20) DEFAULT 'MONTH'
GO

ALTER TABLE SubscriptionPlans
ADD Version INT DEFAULT 1
GO

ALTER TABLE SubscriptionPlans
ADD CreatedAt DATETIME2 DEFAULT GETDATE()
GO



/* =====================================================
UPDATE TABLE: Subscriptions
===================================================== */

ALTER TABLE Subscriptions
ADD AutoRenew BIT DEFAULT 1
GO



/* =====================================================
CREATE TABLE: PlanFeatures
===================================================== */

CREATE TABLE PlanFeatures
(
    Id INT IDENTITY PRIMARY KEY,

    PlanId INT NOT NULL,

    FeatureCode NVARCHAR(100) NOT NULL,

    FeatureLimit INT NOT NULL,

    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(Id)
)
GO


/* =====================================================
CREATE TABLE: Invoices
===================================================== */

CREATE TABLE Invoices
(
    Id INT IDENTITY PRIMARY KEY,

    SubscriptionId INT NOT NULL,

    PaymentId INT NOT NULL,

    InvoiceNumber NVARCHAR(100) NOT NULL,

    Amount DECIMAL(18,2),

    InvoiceDate DATETIME2 DEFAULT GETDATE(),

    DueDate DATETIME2,

    Status NVARCHAR(50) DEFAULT 'PENDING',

    Description NVARCHAR(MAX),

    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),

    FOREIGN KEY (PaymentId) REFERENCES Payments(Id)
)
GO



/* =====================================================
CREATE TABLE: SubscriptionPeriods
===================================================== */

CREATE TABLE SubscriptionPeriods
(
    Id INT IDENTITY PRIMARY KEY,

    SubscriptionId INT NOT NULL,

    PlanId INT NOT NULL,

    PeriodStart DATETIME2 NOT NULL,

    PeriodEnd DATETIME2 NOT NULL,

    FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id),

    FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(Id)
)
GO



/* =====================================================
CREATE TABLE: SubscriptionUsage
===================================================== */

CREATE TABLE SubscriptionUsage
(
    Id INT IDENTITY PRIMARY KEY,

    PeriodId INT NOT NULL,

    FeatureCode NVARCHAR(100) NOT NULL,

    UsedCount INT DEFAULT 0,

    FOREIGN KEY (PeriodId) REFERENCES SubscriptionPeriods(Id)
)
GO

ALTER TABLE Companies 
ADD ProvinceCode INT NULL,
    ProvinceName NVARCHAR(100) NULL,
    WardCode INT NULL,
    WardName NVARCHAR(100) NULL,
    Address NVARCHAR(500) NULL;