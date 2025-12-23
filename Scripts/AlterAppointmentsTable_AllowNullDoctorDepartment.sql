-- Make doctor_id and department_id nullable so the app can save appointments
-- Use this if dbo.appointments has doctor_id/department_id as NOT NULL but the UI doesn't collect them yet.

-- Check current nullability
SELECT c.name AS column_name, c.is_nullable
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.appointments')
  AND c.name IN ('doctor_id','department_id');

BEGIN TRY
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.appointments') AND name = 'doctor_id' AND is_nullable = 0)
    BEGIN
        ALTER TABLE dbo.appointments ALTER COLUMN doctor_id INT NULL;
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.appointments') AND name = 'department_id' AND is_nullable = 0)
    BEGIN
        ALTER TABLE dbo.appointments ALTER COLUMN department_id INT NULL;
    END

    PRINT 'doctor_id/department_id updated to NULLABLE where needed.';
END TRY
BEGIN CATCH
    PRINT 'Failed altering nullability: ' + ERROR_MESSAGE();
END CATCH;
