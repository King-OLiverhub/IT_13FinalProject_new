-- Create patient_bill table in cloud database
-- Run this script in your cloud database (SQL Server)

USE [YourCloudDatabaseName]; -- Replace with your cloud database name
GO

-- Create patient_bill table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='patient_bill' AND xtype='U')
BEGIN
    CREATE TABLE patient_bill (
        id INT IDENTITY(1,1) PRIMARY KEY,
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
    
    PRINT 'patient_bill table created successfully!';
END
ELSE
BEGIN
    PRINT 'patient_bill table already exists';
END

GO

-- Check if table was created and show structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'patient_bill'
ORDER BY ORDINAL_POSITION;

GO

-- Insert sample data if table is empty
IF NOT EXISTS (SELECT 1 FROM patient_bill)
BEGIN
    INSERT INTO patient_bill (
        patient_name, 
        patient_email, 
        assessment_status, 
        date_of_visit, 
        total_amount, 
        insurance_coverage, 
        patient_responsibility, 
        payment_status, 
        payment_method, 
        insurance_provider
    ) VALUES 
    ('John Doe', 'john.doe@email.com', 'Discharged', DATEADD(day, -5, GETUTCDATE()), 5000.00, 3000.00, 2000.00, 'Paid', 'Credit Card', 'Health Insurance Co'),
    ('Jane Smith', 'jane.smith@email.com', 'Pending', DATEADD(day, -2, GETUTCDATE()), 3500.00, 2000.00, 1500.00, 'Unpaid', NULL, 'Medicare Plus'),
    ('Robert Johnson', 'robert.j@email.com', 'For Discharge', DATEADD(day, -1, GETUTCDATE()), 7500.00, 5000.00, 2500.00, 'Partial', 'Cash', 'Blue Cross');
    
    PRINT 'Sample data inserted into patient_bill table!';
END
ELSE
BEGIN
    PRINT 'patient_bill table already has data';
END

GO

-- Show sample data
SELECT TOP 5 * FROM patient_bill ORDER BY created_at DESC;

GO
