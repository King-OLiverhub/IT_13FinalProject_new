-- SQL Script to add missing columns to the patient_bill table
-- Run this script in SQL Server Management Studio (SSMS)

USE [YourDatabaseName]; -- Replace with your actual database name
GO

-- Add missing columns to the patient_bill table
ALTER TABLE patient_bill 
ADD id INT IDENTITY(1,1) PRIMARY KEY;

-- Add patient information columns
ALTER TABLE patient_bill 
ADD patient_name NVARCHAR(200) NOT NULL DEFAULT '';

ALTER TABLE patient_bill 
ADD patient_email NVARCHAR(100) NULL;

ALTER TABLE patient_bill 
ADD assessment_status NVARCHAR(50) NOT NULL DEFAULT 'Pending';

-- Add date columns
ALTER TABLE patient_bill 
ADD date_of_visit DATETIME2 NOT NULL DEFAULT GETUTCDATE();

ALTER TABLE patient_bill 
ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();

ALTER TABLE patient_bill 
ADD updated_at DATETIME2 NULL;

ALTER TABLE patient_bill 
ADD payment_date DATETIME2 NULL;

ALTER TABLE patient_bill 
ADD archived_at DATETIME2 NULL;

-- Add financial columns
ALTER TABLE patient_bill 
ADD total_amount DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE patient_bill 
ADD insurance_coverage DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE patient_bill 
ADD patient_responsibility DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add status and method columns
ALTER TABLE patient_bill 
ADD payment_status NVARCHAR(50) NOT NULL DEFAULT 'Unpaid';

ALTER TABLE patient_bill 
ADD payment_method NVARCHAR(50) NULL;

ALTER TABLE patient_bill 
ADD insurance_provider NVARCHAR(100) NULL;

-- Add archive columns
ALTER TABLE patient_bill 
ADD is_archived BIT NOT NULL DEFAULT 0;

GO

-- Insert sample data (optional)
INSERT INTO patient_bill (patient_name, patient_email, assessment_status, date_of_visit, total_amount, payment_status, payment_date)
VALUES 
('Himeko Murata', 'himeko_murata@gmail.com', 'For Discharge', DATEADD(day, -5, GETDATE()), 1500.00, 'Pending', NULL),
('Yae Miko', 'miko.yae@mail.com', 'For Discharge', DATEADD(day, -3, GETDATE()), 2300.50, 'Paid', DATEADD(day, -2, GETDATE())),
('Black Swan', 'ms.blkswan@mail.com', 'For Discharge', DATEADD(day, -1, GETDATE()), 890.75, 'Pending', NULL),
('Ruan Mei', 'therm_ei@gmail.com', 'For Discharge', GETDATE(), 3200.00, 'Unpaid', NULL),
('Bronya Rand', 'bronya.rr@mail.com', 'For Discharge', DATEADD(day, -4, GETDATE()), 1750.25, 'Paid', DATEADD(day, -3, GETDATE()));

GO

PRINT 'patient_bill table columns added successfully!';
PRINT 'Sample data inserted successfully!';
