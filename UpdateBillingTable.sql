-- SQL Script to add missing columns to the billing table
-- Run this script in SQL Server Management Studio (SSMS)

USE [YourDatabaseName]; -- Replace with your actual database name
GO

-- Add missing columns to the billing table
ALTER TABLE billing 
ADD id INT IDENTITY(1,1) PRIMARY KEY;

-- Add patient information columns
ALTER TABLE billing 
ADD patient_name NVARCHAR(200) NOT NULL DEFAULT '';

ALTER TABLE billing 
ADD patient_email NVARCHAR(100) NULL;

ALTER TABLE billing 
ADD assessment_status NVARCHAR(50) NOT NULL DEFAULT 'Pending';

-- Add date columns
ALTER TABLE billing 
ADD date_of_visit DATETIME2 NOT NULL DEFAULT GETUTCDATE();

ALTER TABLE billing 
ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();

ALTER TABLE billing 
ADD updated_at DATETIME2 NULL;

ALTER TABLE billing 
ADD payment_date DATETIME2 NULL;

ALTER TABLE billing 
ADD archived_at DATETIME2 NULL;

-- Add financial columns
ALTER TABLE billing 
ADD total_amount DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE billing 
ADD insurance_coverage DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE billing 
ADD patient_responsibility DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add status and method columns
ALTER TABLE billing 
ADD payment_status NVARCHAR(50) NOT NULL DEFAULT 'Unpaid';

ALTER TABLE billing 
ADD payment_method NVARCHAR(50) NULL;

ALTER TABLE billing 
ADD insurance_provider NVARCHAR(100) NULL;

-- Add archive columns
ALTER TABLE billing 
ADD is_archived BIT NOT NULL DEFAULT 0;

GO

-- Insert sample data (optional)
INSERT INTO billing (patient_name, patient_email, assessment_status, date_of_visit, total_amount, payment_status, payment_date)
VALUES 
('Himeko Murata', 'himeko_murata@gmail.com', 'For Discharge', DATEADD(day, -5, GETDATE()), 1500.00, 'Pending', NULL),
('Yae Miko', 'miko.yae@mail.com', 'For Discharge', DATEADD(day, -3, GETDATE()), 2300.50, 'Paid', DATEADD(day, -2, GETDATE())),
('Black Swan', 'ms.blkswan@mail.com', 'For Discharge', DATEADD(day, -1, GETDATE()), 890.75, 'Pending', NULL),
('Ruan Mei', 'therm_ei@gmail.com', 'For Discharge', GETDATE(), 3200.00, 'Unpaid', NULL),
('Bronya Rand', 'bronya.rr@mail.com', 'For Discharge', DATEADD(day, -4, GETDATE()), 1750.25, 'Paid', DATEADD(day, -3, GETDATE()));

GO

PRINT 'Billing table columns added successfully!';
PRINT 'Sample data inserted successfully!';
