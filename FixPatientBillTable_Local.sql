-- Fix patient_bill table for archive functionality in LOCAL database
-- Run this script in SQL Server Management Studio (SSMS) for your LOCAL database

USE [IT_13FinalProject]; -- Your local database name
GO

-- Check if table exists and add missing columns if needed
IF EXISTS (SELECT * FROM sysobjects WHERE name='patient_bill' AND xtype='U')
BEGIN
    -- Add archived_at column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=OBJECT_ID('patient_bill') AND name='archived_at')
    BEGIN
        ALTER TABLE patient_bill ADD archived_at DATETIME2 NULL;
        PRINT 'Added archived_at column';
    END
    
    -- Add updated_at column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=OBJECT_ID('patient_bill') AND name='updated_at')
    BEGIN
        ALTER TABLE patient_bill ADD updated_at DATETIME2 NULL;
        PRINT 'Added updated_at column';
    END
    
    -- Add is_archived column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=OBJECT_ID('patient_bill') AND name='is_archived')
    BEGIN
        ALTER TABLE patient_bill ADD is_archived BIT NOT NULL DEFAULT 0;
        PRINT 'Added is_archived column';
    END
    
    -- Add created_at column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=OBJECT_ID('patient_bill') AND name='created_at')
    BEGIN
        ALTER TABLE patient_bill ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added created_at column';
    END
    
    -- Drop foreign key constraint if it exists and causes issues
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_patient_bill_patients')
    BEGIN
        ALTER TABLE patient_bill DROP CONSTRAINT FK_patient_bill_patients;
        PRINT 'Dropped FK_patient_bill_patients constraint';
    END
    
    PRINT 'Local patient_bill table updated successfully!';
END
ELSE
BEGIN
    PRINT 'patient_bill table does not exist in local database';
END

GO

-- Show current table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'patient_bill'
ORDER BY ORDINAL_POSITION;

GO
