-- Create appointments table (Cloud + Local)
-- Run on BOTH CloudConnection and DefaultConnection databases.

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'appointments')
BEGIN
    CREATE TABLE appointments (
        appointment_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        patient_id INT NULL,
        patient_name NVARCHAR(200) NOT NULL,
        start_datetime DATETIME2 NOT NULL,
        duration_minutes INT NOT NULL CONSTRAINT DF_appointments_duration DEFAULT (30),
        type NVARCHAR(100) NOT NULL CONSTRAINT DF_appointments_type DEFAULT ('Consultation'),
        status NVARCHAR(50) NOT NULL CONSTRAINT DF_appointments_status DEFAULT ('SCHEDULED'),
        reason NVARCHAR(500) NOT NULL CONSTRAINT DF_appointments_reason DEFAULT (''),
        notes NVARCHAR(MAX) NULL,
        tags NVARCHAR(500) NULL,
        is_cancelled BIT NOT NULL CONSTRAINT DF_appointments_is_cancelled DEFAULT (0),
        cancelled_at DATETIME2 NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_appointments_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NULL
    );

    -- Optional FK if patients table exists with patient_id
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'patients')
       AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'patients' AND COLUMN_NAME = 'patient_id')
    BEGIN
        BEGIN TRY
            ALTER TABLE appointments
            ADD CONSTRAINT FK_appointments_patients
            FOREIGN KEY (patient_id) REFERENCES patients(patient_id)
            ON DELETE SET NULL;
        END TRY
        BEGIN CATCH
            -- Ignore FK creation errors (e.g., existing data mismatch)
        END CATCH
    END
END
ELSE
BEGIN
    PRINT 'appointments table already exists.';
END
