-- Add missing columns to inventory table
-- This script adds the columns that the PharmacyInventory model expects

-- Check if column exists before adding to avoid errors
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'generic_name')
BEGIN
    ALTER TABLE inventory ADD generic_name NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'brand_name')
BEGIN
    ALTER TABLE inventory ADD brand_name NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'category')
BEGIN
    ALTER TABLE inventory ADD category NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'unit')
BEGIN
    ALTER TABLE inventory ADD unit NVARCHAR(50) NOT NULL DEFAULT 'Piece(s)';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'batch_number')
BEGIN
    ALTER TABLE inventory ADD batch_number NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'storage_requirements')
BEGIN
    ALTER TABLE inventory ADD storage_requirements NVARCHAR(200) NOT NULL DEFAULT 'Room temperature';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'prescription_required')
BEGIN
    ALTER TABLE inventory ADD prescription_required BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'barcode')
BEGIN
    ALTER TABLE inventory ADD barcode NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'is_archived')
BEGIN
    ALTER TABLE inventory ADD is_archived BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory' AND COLUMN_NAME = 'archived_at')
BEGIN
    ALTER TABLE inventory ADD archived_at DATETIME NULL;
END

-- Update existing records to have default values for new NOT NULL columns
UPDATE inventory SET unit = 'Piece(s)' WHERE unit IS NULL;
UPDATE inventory SET storage_requirements = 'Room temperature' WHERE storage_requirements IS NULL;
UPDATE inventory SET prescription_required = 0 WHERE prescription_required IS NULL;
UPDATE inventory SET is_archived = 0 WHERE is_archived IS NULL;
