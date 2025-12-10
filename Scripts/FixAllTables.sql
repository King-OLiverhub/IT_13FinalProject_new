-- Complete database schema fix script
-- Run this script directly on your database to add all missing columns

-- Fix inventory table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'generic_name')
BEGIN
    ALTER TABLE inventory ADD generic_name NVARCHAR(200) NULL;
    PRINT 'Added inventory.generic_name';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'brand_name')
BEGIN
    ALTER TABLE inventory ADD brand_name NVARCHAR(200) NULL;
    PRINT 'Added inventory.brand_name';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'category')
BEGIN
    ALTER TABLE inventory ADD category NVARCHAR(100) NULL;
    PRINT 'Added inventory.category';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'unit')
BEGIN
    ALTER TABLE inventory ADD unit NVARCHAR(50) NOT NULL DEFAULT 'Piece(s)';
    PRINT 'Added inventory.unit';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'batch_number')
BEGIN
    ALTER TABLE inventory ADD batch_number NVARCHAR(100) NULL;
    PRINT 'Added inventory.batch_number';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'storage_requirements')
BEGIN
    ALTER TABLE inventory ADD storage_requirements NVARCHAR(200) NOT NULL DEFAULT 'Room temperature';
    PRINT 'Added inventory.storage_requirements';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'prescription_required')
BEGIN
    ALTER TABLE inventory ADD prescription_required BIT NOT NULL DEFAULT 0;
    PRINT 'Added inventory.prescription_required';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'barcode')
BEGIN
    ALTER TABLE inventory ADD barcode NVARCHAR(100) NULL;
    PRINT 'Added inventory.barcode';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'is_archived')
BEGIN
    ALTER TABLE inventory ADD is_archived BIT NOT NULL DEFAULT 0;
    PRINT 'Added inventory.is_archived';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'archived_at')
BEGIN
    ALTER TABLE inventory ADD archived_at DATETIME NULL;
    PRINT 'Added inventory.archived_at';
END

-- Fix patients table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'patients' AND COLUMN_NAME = 'is_archived')
BEGIN
    ALTER TABLE patients ADD is_archived BIT NOT NULL DEFAULT 0;
    PRINT 'Added patients.is_archived';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'patients' AND COLUMN_NAME = 'archived_at')
BEGIN
    ALTER TABLE patients ADD archived_at DATETIME NULL;
    PRINT 'Added patients.archived_at';
END

-- Fix health_records table (only add columns that are actually needed)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'health_records' AND COLUMN_NAME = 'doctor_initial_remarks')
BEGIN
    ALTER TABLE health_records ADD doctor_initial_remarks NVARCHAR(MAX) NULL;
    PRINT 'Added health_records.doctor_initial_remarks';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'health_records' AND COLUMN_NAME = 'special_instructions')
BEGIN
    ALTER TABLE health_records ADD special_instructions NVARCHAR(MAX) NULL;
    PRINT 'Added health_records.special_instructions';
END

-- Update existing records with default values (only if columns exist)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'unit')
BEGIN
    UPDATE inventory SET unit = 'Piece(s)' WHERE unit IS NULL;
    PRINT 'Updated inventory.unit default values';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'storage_requirements')
BEGIN
    UPDATE inventory SET storage_requirements = 'Room temperature' WHERE storage_requirements IS NULL;
    PRINT 'Updated inventory.storage_requirements default values';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'prescription_required')
BEGIN
    UPDATE inventory SET prescription_required = 0 WHERE prescription_required IS NULL;
    PRINT 'Updated inventory.prescription_required default values';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'is_archived')
BEGIN
    UPDATE inventory SET is_archived = 0 WHERE is_archived IS NULL;
    PRINT 'Updated inventory.is_archived default values';
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'patients' AND COLUMN_NAME = 'is_archived')
BEGIN
    UPDATE patients SET is_archived = 0 WHERE is_archived IS NULL;
    PRINT 'Updated patients.is_archived default values';
END

PRINT 'Database schema update completed successfully!';
