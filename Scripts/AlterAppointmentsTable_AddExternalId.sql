-- Add external_id column for syncing appointments between CloudConnection and DefaultConnection
-- Run on BOTH databases.

IF NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'appointments'
      AND COLUMN_NAME = 'external_id')
BEGIN
    ALTER TABLE dbo.appointments ADD external_id UNIQUEIDENTIFIER NULL;
END

-- Optional: enforce uniqueness when values are present
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_appointments_external_id'
      AND object_id = OBJECT_ID('dbo.appointments'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_appointments_external_id
    ON dbo.appointments(external_id)
    WHERE external_id IS NOT NULL;
END

PRINT 'external_id column/index ensured.';
