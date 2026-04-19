SET XACT_ABORT ON;
BEGIN TRANSACTION;

/*
Goal:
- Move subscription architecture to 3 tables:
  1) SubscriptionPlans (base plan)
    2) SubscriptionPlanOptions (Monthly/Yearly)
  3) Subscriptions (link to PlanOptionId)

This script is idempotent-safe for schema creation and data backfill.
*/

/* 1) Create SubscriptionPlanOptions table */
IF OBJECT_ID('dbo.SubscriptionPlanOptions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SubscriptionPlanOptions
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PlanId INT NOT NULL,
        BillingCycle NVARCHAR(20) NOT NULL,
        Price DECIMAL(18,2) NULL,
        DurationDays INT NULL,
        IsActive BIT NULL CONSTRAINT DF_SubscriptionPlanOptions_IsActive DEFAULT(1),
        CreatedAt DATETIME NULL CONSTRAINT DF_SubscriptionPlanOptions_CreatedAt DEFAULT(GETDATE())
    );

    ALTER TABLE dbo.SubscriptionPlanOptions
    ADD CONSTRAINT FK_SubscriptionPlanOptions_SubscriptionPlans
        FOREIGN KEY (PlanId) REFERENCES dbo.SubscriptionPlans(Id) ON DELETE CASCADE;

    CREATE UNIQUE INDEX UX_SubscriptionPlanOptions_Plan_Cycle
        ON dbo.SubscriptionPlanOptions(PlanId, BillingCycle);
END

/* 2) Add PlanOptionId to Subscriptions */
IF COL_LENGTH('dbo.Subscriptions', 'PlanOptionId') IS NULL
BEGIN
    ALTER TABLE dbo.Subscriptions ADD PlanOptionId INT NULL;
END

IF COL_LENGTH('dbo.Subscriptions', 'SubscribedPrice') IS NULL
BEGIN
    ALTER TABLE dbo.Subscriptions ADD SubscribedPrice DECIMAL(18,2) NULL;
END

IF COL_LENGTH('dbo.Subscriptions', 'SubscribedBillingCycle') IS NULL
BEGIN
    ALTER TABLE dbo.Subscriptions ADD SubscribedBillingCycle NVARCHAR(20) NULL;
END

IF COL_LENGTH('dbo.Subscriptions', 'SubscribedDurationDays') IS NULL
BEGIN
    ALTER TABLE dbo.Subscriptions ADD SubscribedDurationDays INT NULL;
END

IF COL_LENGTH('dbo.Subscriptions', 'SubscribedPlanName') IS NULL
BEGIN
    ALTER TABLE dbo.Subscriptions ADD SubscribedPlanName NVARCHAR(200) NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Subscriptions_PlanOptions'
)
BEGIN
    ALTER TABLE dbo.Subscriptions
    ADD CONSTRAINT FK_Subscriptions_PlanOptions
        FOREIGN KEY (PlanOptionId) REFERENCES dbo.SubscriptionPlanOptions(Id);
END

/* 3) Capture existing plan rows (including duplicate month/year rows) */
IF OBJECT_ID('tempdb..#OldPlanRows') IS NOT NULL DROP TABLE #OldPlanRows;

SELECT
    p.Id AS OldPlanId,
    LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(p.Name, N' (Hàng tháng)', N''), N' (Hàng năm)', N''), N' (Monthly)', N''), N' (Yearly)', N''))) AS BaseName,
    CASE
        WHEN p.BillingCycle IN ('Monthly','Yearly') THEN p.BillingCycle
        WHEN p.Name LIKE N'%(Hàng năm)%' OR p.Name LIKE N'%(Yearly)%' THEN 'Yearly'
        ELSE 'Monthly'
    END AS BillingCycle,
    p.Price,
    p.DurationDays,
    p.Description,
    p.IsActive,
    p.IsArchived,
    p.CreatedAt
INTO #OldPlanRows
FROM dbo.SubscriptionPlans p;

/* 4) Keep one base row per BaseName and remap FK references to it */
IF OBJECT_ID('tempdb..#PlanKeeperMap') IS NOT NULL DROP TABLE #PlanKeeperMap;

WITH normalized AS
(
    SELECT
        OldPlanId,
        BaseName,
        MIN(OldPlanId) OVER (PARTITION BY BaseName) AS KeeperPlanId
    FROM #OldPlanRows
)
SELECT *
INTO #PlanKeeperMap
FROM normalized;

/* Update keeper plan name to clean base name */
UPDATE kp
SET kp.Name = map.BaseName
FROM dbo.SubscriptionPlans kp
JOIN #PlanKeeperMap map ON kp.Id = map.KeeperPlanId;

IF COL_LENGTH('dbo.SubscriptionPlans', 'BillingCycle') IS NOT NULL
BEGIN
    UPDATE kp
    SET kp.BillingCycle = 'Flexible'
    FROM dbo.SubscriptionPlans kp
    JOIN #PlanKeeperMap map ON kp.Id = map.KeeperPlanId;
END

/* Move PlanFeatures to keeper if needed */
UPDATE pf
SET pf.PlanId = map.KeeperPlanId
FROM dbo.PlanFeatures pf
JOIN #PlanKeeperMap map ON pf.PlanId = map.OldPlanId
WHERE map.OldPlanId <> map.KeeperPlanId;

/* Move SubscriptionPeriods to keeper plan */
UPDATE sp
SET sp.PlanId = map.KeeperPlanId
FROM dbo.SubscriptionPeriods sp
JOIN #PlanKeeperMap map ON sp.PlanId = map.OldPlanId
WHERE map.OldPlanId <> map.KeeperPlanId;

/* Move Subscriptions to keeper plan */
UPDATE s
SET s.PlanId = map.KeeperPlanId
FROM dbo.Subscriptions s
JOIN #PlanKeeperMap map ON s.PlanId = map.OldPlanId
WHERE map.OldPlanId <> map.KeeperPlanId;

/* Delete duplicate plans (non-keeper) */
DELETE p
FROM dbo.SubscriptionPlans p
JOIN #PlanKeeperMap map ON p.Id = map.OldPlanId
WHERE map.OldPlanId <> map.KeeperPlanId;

/* 5) Backfill options from old plan rows into keeper plans */
;WITH oldRowsMapped AS
(
    SELECT
        map.KeeperPlanId AS PlanId,
        old.OldPlanId,
        old.BillingCycle,
        old.Price,
        CASE
            WHEN old.BillingCycle = 'Yearly' AND (old.DurationDays IS NULL OR old.DurationDays <= 31) THEN 365
            WHEN old.BillingCycle = 'Monthly' AND (old.DurationDays IS NULL OR old.DurationDays > 365) THEN 30
            ELSE ISNULL(old.DurationDays, CASE WHEN old.BillingCycle = 'Yearly' THEN 365 ELSE 30 END)
        END AS DurationDays,
        old.IsActive,
        old.CreatedAt
    FROM #OldPlanRows old
    JOIN #PlanKeeperMap map ON old.OldPlanId = map.OldPlanId
),
dedup AS
(
    SELECT
        m.PlanId,
        m.BillingCycle,
        m.Price,
        m.DurationDays,
        m.IsActive,
        m.CreatedAt,
        ROW_NUMBER() OVER
        (
            PARTITION BY m.PlanId, m.BillingCycle
            ORDER BY ISNULL(m.CreatedAt, '19000101') DESC, m.OldPlanId DESC
        ) AS rn
    FROM oldRowsMapped m
)
INSERT INTO dbo.SubscriptionPlanOptions (PlanId, BillingCycle, Price, DurationDays, IsActive, CreatedAt)
SELECT
    d.PlanId,
    d.BillingCycle,
    d.Price,
    d.DurationDays,
    d.IsActive,
    ISNULL(d.CreatedAt, GETDATE())
FROM dedup d
WHERE d.rn = 1
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.SubscriptionPlanOptions o
      WHERE o.PlanId = d.PlanId
        AND o.BillingCycle = d.BillingCycle
  );

/* 6) Backfill Subscriptions.PlanOptionId using PlanId + matching cycle */
UPDATE s
SET s.PlanOptionId = o.Id
FROM dbo.Subscriptions s
JOIN dbo.SubscriptionPlanOptions o ON o.PlanId = s.PlanId
WHERE s.PlanOptionId IS NULL
  AND o.BillingCycle = (
      CASE
          WHEN s.EndDate IS NOT NULL AND s.StartDate IS NOT NULL AND DATEDIFF(DAY, s.StartDate, s.EndDate) >= 330 THEN 'Yearly'
          ELSE 'Monthly'
      END
  );

/* Fallback if still null: pick Monthly first, then any option */
UPDATE s
SET s.PlanOptionId = pick.OptionId
FROM dbo.Subscriptions s
CROSS APPLY
(
    SELECT TOP 1 o.Id AS OptionId
    FROM dbo.SubscriptionPlanOptions o
    WHERE o.PlanId = s.PlanId
    ORDER BY CASE WHEN o.BillingCycle = 'Monthly' THEN 0 ELSE 1 END, o.Id
) pick
WHERE s.PlanOptionId IS NULL;

/* 7) Snapshot current purchased data into Subscriptions */
IF COL_LENGTH('dbo.Subscriptions', 'SubscribedPrice') IS NOT NULL
   AND COL_LENGTH('dbo.Subscriptions', 'SubscribedBillingCycle') IS NOT NULL
   AND COL_LENGTH('dbo.Subscriptions', 'SubscribedDurationDays') IS NOT NULL
   AND COL_LENGTH('dbo.Subscriptions', 'SubscribedPlanName') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
        UPDATE s
        SET s.SubscribedPrice = o.Price,
            s.SubscribedBillingCycle = o.BillingCycle,
            s.SubscribedDurationDays = o.DurationDays,
            s.SubscribedPlanName = p.Name
        FROM dbo.Subscriptions s
        JOIN dbo.SubscriptionPlanOptions o ON o.Id = s.PlanOptionId
        JOIN dbo.SubscriptionPlans p ON p.Id = s.PlanId
        WHERE s.PlanOptionId IS NOT NULL;
    ';
END

/* 8) Remove legacy columns from SubscriptionPlans (data now lives in options) */
IF COL_LENGTH('dbo.SubscriptionPlans', 'DurationDays') IS NOT NULL
BEGIN
    DECLARE @dfDurationDays sysname;
    SELECT @dfDurationDays = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t
        ON t.object_id = c.object_id
    WHERE t.name = 'SubscriptionPlans'
      AND SCHEMA_NAME(t.schema_id) = 'dbo'
      AND c.name = 'DurationDays';

    IF @dfDurationDays IS NOT NULL
        EXEC('ALTER TABLE dbo.SubscriptionPlans DROP CONSTRAINT [' + @dfDurationDays + ']');

    ALTER TABLE dbo.SubscriptionPlans DROP COLUMN DurationDays;
END

IF COL_LENGTH('dbo.SubscriptionPlans', 'BillingCycle') IS NOT NULL
BEGIN
    DECLARE @dfBillingCycle sysname;
    SELECT @dfBillingCycle = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t
        ON t.object_id = c.object_id
    WHERE t.name = 'SubscriptionPlans'
      AND SCHEMA_NAME(t.schema_id) = 'dbo'
      AND c.name = 'BillingCycle';

    IF @dfBillingCycle IS NOT NULL
        EXEC('ALTER TABLE dbo.SubscriptionPlans DROP CONSTRAINT [' + @dfBillingCycle + ']');

    ALTER TABLE dbo.SubscriptionPlans DROP COLUMN BillingCycle;
END

IF COL_LENGTH('dbo.SubscriptionPlans', 'Version') IS NOT NULL
BEGIN
    DECLARE @dfVersion sysname;
    SELECT @dfVersion = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t
        ON t.object_id = c.object_id
    WHERE t.name = 'SubscriptionPlans'
      AND SCHEMA_NAME(t.schema_id) = 'dbo'
      AND c.name = 'Version';

    IF @dfVersion IS NOT NULL
        EXEC('ALTER TABLE dbo.SubscriptionPlans DROP CONSTRAINT [' + @dfVersion + ']');

    ALTER TABLE dbo.SubscriptionPlans DROP COLUMN Version;
END

COMMIT TRANSACTION;
PRINT 'Migration completed: SubscriptionPlans + SubscriptionPlanOptions + Subscriptions(PlanOptionId)';
