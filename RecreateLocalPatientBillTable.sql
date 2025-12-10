-- Fix local database to allow manual ID insertion for patient_bill table
-- Run this script in SQL Server Management Studio (SSMS) for your LOCAL database

USE [IT_13FinalProject];
GO

-- Drop and recreate patient_bill table to allow manual ID insertion
IF EXISTS (SELECT * FROM sysobjects WHERE name='patient_bill' AND xtype='U')
BEGIN
    -- Drop existing table
    DROP TABLE patient_bill;
    PRINT 'Dropped existing patient_bill table';
END

-- Recreate table without identity column to allow manual ID insertion
CREATE TABLE patient_bill (
    id INT PRIMARY KEY, -- Remove IDENTITY to allow manual ID setting
    patient_id INT NULL,
    patient_name NVARCHAR(200) NOT NULL,
    patient_email NVARCHAR(100) NULL,
    assessment_status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    date_of_visit DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    insurance_coverage DECIMAL(18,2) NOT NULL DEFAULT 0,
    patient_responsibility DECIMAL(18,2) NOT NULL DEFAULT 0,
    payment_status NVARCHAR(50) NOT NULL DEFAULT 'Unpaid',
    payment_method NVARCHAR(50) NULL,
    payment_date DATETIME2 NULL,
    insurance_provider NVARCHAR(100) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME2 NULL,
    is_archived BIT NOT NULL DEFAULT 0,
    archived_at DATETIME2 NULL
);

PRINT 'Created patient_bill table without identity column';

GO

-- Copy existing data from cloud database if you want to sync
-- Uncomment and modify this section if you need to copy data from cloud
/*
-- Insert existing data from cloud database (modify connection details as needed)
INSERT INTO patient_bill (
    id, patient_id, patient_name, patient_email, assessment_status, 
    date_of_visit, total_amount, insurance_coverage, patient_responsibility,
    payment_status, payment_method, payment_date, insurance_provider,
    created_at, updated_at, is_archived, archived_at
)
SELECT 
    id, patient_id, patient_name, patient_email, assessment_status,
    date_of_visit, total_amount, insurance_coverage, patient_responsibility,
    payment_status, payment_method, payment_date, insurance_provider,
    created_at, updated_at, is_archived, archived_at
FROM [db34495].[dbo].[patient_bill]; -- Modify with your cloud database name

PRINT 'Synced data from cloud database';
*/

GO

-- Show table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'patient_bill'
ORDER BY ORDINAL_POSITION;

GO

PRINT 'Local database patient_bill table is now ready for manual ID insertion!';
