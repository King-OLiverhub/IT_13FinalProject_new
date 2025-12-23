-- Adds missing columns needed by the app to existing dbo.appointments table
-- Safe to run multiple times.

-- Patient display name (if not already present)
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'patient_name')
BEGIN
    ALTER TABLE dbo.appointments ADD patient_name NVARCHAR(200) NOT NULL DEFAULT('');
END

-- Reason (used in UI list/search)
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'reason')
BEGIN
    ALTER TABLE dbo.appointments ADD reason NVARCHAR(500) NOT NULL DEFAULT('');
END

-- Tags (optional)
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'tags')
BEGIN
    ALTER TABLE dbo.appointments ADD tags NVARCHAR(500) NULL;
END

-- Cancellation flags
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'is_cancelled')
BEGIN
    ALTER TABLE dbo.appointments ADD is_cancelled BIT NOT NULL DEFAULT(0);
END

IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'cancelled_at')
BEGIN
    ALTER TABLE dbo.appointments ADD cancelled_at DATETIME2 NULL;
END

-- Audit timestamps
IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'created_at')
BEGIN
    ALTER TABLE dbo.appointments ADD created_at DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME());
END

IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'updated_at')
BEGIN
    ALTER TABLE dbo.appointments ADD updated_at DATETIME2 NULL;
END

-- Backfill patient_name for existing rows if patient_id exists and patients table exists
IF EXISTS (
        SELECT *
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = 'appointments'
          AND COLUMN_NAME = 'patient_id')
   AND EXISTS (
        SELECT *
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = 'patients')
   AND EXISTS (
        SELECT *
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = 'patients'
          AND COLUMN_NAME = 'patient_id')
   AND EXISTS (
        SELECT *
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = 'patients'
          AND COLUMN_NAME = 'first_name')
   AND EXISTS (
        SELECT *
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = 'patients'
          AND COLUMN_NAME = 'last_name')
BEGIN
    BEGIN TRY
        DECLARE @sql NVARCHAR(MAX) = N'
UPDATE a
SET patient_name = LTRIM(RTRIM(ISNULL(p.first_name, '''') + '' '' + ISNULL(p.last_name, '''')))
FROM dbo.appointments a
INNER JOIN dbo.patients p ON p.patient_id = a.patient_id
WHERE (a.patient_name IS NULL OR a.patient_name = '''');
';
        EXEC sys.sp_executesql @sql;
    END TRY
    BEGIN CATCH
        PRINT 'Skipped patient_name backfill due to: ' + ERROR_MESSAGE();
    END CATCH
END

PRINT 'Done updating dbo.appointments.';
