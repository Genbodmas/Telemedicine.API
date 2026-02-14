-- =========================================
-- Doctor Availability Schema & Procedures
-- =========================================

-- Table: tbl_DoctorAvailability
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_DoctorAvailability')
BEGIN
    CREATE TABLE tbl_DoctorAvailability (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        DoctorId        INT NOT NULL,
        DayOfWeek       INT NOT NULL,        -- 0=Sunday, 1=Monday, ..., 6=Saturday
        StartTime       TIME NOT NULL,
        EndTime         TIME NOT NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        CreatedAt       DATETIME DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Avail_Doctor FOREIGN KEY (DoctorId) REFERENCES tbl_Users(Id),
        CONSTRAINT CK_Day CHECK (DayOfWeek >= 0 AND DayOfWeek <= 6),
        CONSTRAINT CK_Time CHECK (EndTime > StartTime)
    );
END;
GO

-- SP: Set availability (upsert by DoctorId + DayOfWeek)
CREATE OR ALTER PROCEDURE sp_SetDoctorAvailability
    @DoctorId   INT,
    @DayOfWeek  INT,
    @StartTime  TIME,
    @EndTime    TIME,
    @IsActive   BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate doctor role
    IF NOT EXISTS (SELECT 1 FROM tbl_Users WHERE Id = @DoctorId AND Role = 'Doctor')
        THROW 50001, 'User is not a doctor.', 1;

    -- Upsert
    IF EXISTS (SELECT 1 FROM tbl_DoctorAvailability WHERE DoctorId = @DoctorId AND DayOfWeek = @DayOfWeek)
    BEGIN
        UPDATE tbl_DoctorAvailability
        SET StartTime = @StartTime, EndTime = @EndTime, IsActive = @IsActive
        WHERE DoctorId = @DoctorId AND DayOfWeek = @DayOfWeek;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_DoctorAvailability (DoctorId, DayOfWeek, StartTime, EndTime, IsActive)
        VALUES (@DoctorId, @DayOfWeek, @StartTime, @EndTime, @IsActive);
    END

    -- Audit
    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('SET_AVAILABILITY', @DoctorId, 'Success', GETUTCDATE(), CONCAT('Day: ', @DayOfWeek, ', ', CAST(@StartTime AS VARCHAR), '-', CAST(@EndTime AS VARCHAR)));

    SELECT 1 AS Result;
END;
GO

-- SP: Get availability for a specific doctor
CREATE OR ALTER PROCEDURE sp_GetDoctorAvailability
    @DoctorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, DayOfWeek, CAST(StartTime AS VARCHAR(5)) AS StartTime, CAST(EndTime AS VARCHAR(5)) AS EndTime, IsActive
    FROM tbl_DoctorAvailability
    WHERE DoctorId = @DoctorId
    ORDER BY DayOfWeek;
END;
GO

-- SP: Delete a specific availability slot
CREATE OR ALTER PROCEDURE sp_DeleteDoctorAvailability
    @Id         INT,
    @DoctorId   INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tbl_DoctorAvailability WHERE Id = @Id AND DoctorId = @DoctorId;
    SELECT @@ROWCOUNT AS Deleted;
END;
GO
