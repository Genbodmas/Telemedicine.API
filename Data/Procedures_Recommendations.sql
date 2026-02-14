-- =========================================
-- Recommendations Schema & Procedures
-- =========================================

-- Table: tbl_Recommendations
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_Recommendations')
BEGIN
    CREATE TABLE tbl_Recommendations (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        AppointmentId   INT NOT NULL,
        Details         NVARCHAR(MAX) NOT NULL,
        CreatedAt       DATETIME DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Rec_Appt FOREIGN KEY (AppointmentId) REFERENCES tbl_Appointments(Id) ON DELETE CASCADE
    );
END;
GO

-- SP: Add Recommendation (Doctor only)
CREATE OR ALTER PROCEDURE sp_AddRecommendation
    @AppointmentId INT,
    @DoctorId      INT,
    @Details       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate User is Doctor for this appointment
    IF NOT EXISTS (SELECT 1 FROM tbl_Appointments WHERE Id = @AppointmentId AND DoctorId = @DoctorId)
    BEGIN
        THROW 50001, 'Unauthorized: Not the assigned doctor', 1;
    END

    -- Upsert (Update if exists, else Insert)
    IF EXISTS (SELECT 1 FROM tbl_Recommendations WHERE AppointmentId = @AppointmentId)
    BEGIN
        UPDATE tbl_Recommendations
        SET Details = @Details, CreatedAt = GETUTCDATE()
        WHERE AppointmentId = @AppointmentId;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Recommendations (AppointmentId, Details, CreatedAt)
        VALUES (@AppointmentId, @Details, GETUTCDATE());
    END

    -- Audit
    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('AddRecommendation', @DoctorId, 'Success', GETUTCDATE(), CAST(@AppointmentId AS NVARCHAR(50)));
    
    SELECT 1 AS Result;
END;
GO

-- SP: Get Recommendation
CREATE OR ALTER PROCEDURE sp_GetRecommendation
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Details, CreatedAt FROM tbl_Recommendations WHERE AppointmentId = @AppointmentId;
END;
GO

-- SP: Add Recommendation by RoomId
CREATE OR ALTER PROCEDURE sp_AddRecommendationByRoom
    @RoomId UNIQUEIDENTIFIER,
    @DoctorId INT,
    @Details NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AppointmentId INT;
    SELECT @AppointmentId = AppointmentId FROM tbl_ConsultationRooms WHERE RoomId = @RoomId;

    IF @AppointmentId IS NULL
        THROW 50001, 'Room not found', 1;

    EXEC sp_AddRecommendation @AppointmentId, @DoctorId, @Details;
END;
GO

-- SP: Get Recommendation by RoomId (Convenience)
CREATE OR ALTER PROCEDURE sp_GetRecommendationByRoom
    @RoomId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AppointmentId INT;
    SELECT @AppointmentId = AppointmentId FROM tbl_ConsultationRooms WHERE RoomId = @RoomId;

    SELECT Details, CreatedAt FROM tbl_Recommendations WHERE AppointmentId = @AppointmentId;
END;
GO
