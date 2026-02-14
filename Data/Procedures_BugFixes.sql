-- =============================================
-- Pre-Room & Bug Fixes Migration Script
-- =============================================

-- 1. Schema Update: Add Reason to tbl_Appointments
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_Appointments' AND COLUMN_NAME = 'Reason')
BEGIN
    ALTER TABLE tbl_Appointments ADD Reason NVARCHAR(500) NULL;
END;
GO

-- 2. SP: Book Appointment (Updated with Validation & Reason)
CREATE OR ALTER PROCEDURE sp_BookAppointment
    @PatientId INT,
    @DoctorId INT,
    @ScheduledTime DATETIME,
    @Reason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- A. Validate Doctor Availability (only if doctor has set availability)
    DECLARE @HasAvailability BIT = 0;
    IF EXISTS (SELECT 1 FROM tbl_DoctorAvailability WHERE DoctorId = @DoctorId AND IsActive = 1)
    BEGIN
        SET @HasAvailability = 1;
    END;

    IF @HasAvailability = 1
    BEGIN
        DECLARE @DayName NVARCHAR(20) = DATENAME(WEEKDAY, @ScheduledTime);
        DECLARE @TimePart TIME = CAST(@ScheduledTime AS TIME);
        DECLARE @DayOfWeekInt INT;

        SET @DayOfWeekInt = CASE @DayName
            WHEN 'Sunday' THEN 0
            WHEN 'Monday' THEN 1
            WHEN 'Tuesday' THEN 2
            WHEN 'Wednesday' THEN 3
            WHEN 'Thursday' THEN 4
            WHEN 'Friday' THEN 5
            WHEN 'Saturday' THEN 6
        END;

        IF NOT EXISTS (
            SELECT 1 FROM tbl_DoctorAvailability
            WHERE DoctorId = @DoctorId
              AND DayOfWeek = @DayOfWeekInt
              AND IsActive = 1
              AND @TimePart >= StartTime
              AND @TimePart <= EndTime
        )
        BEGIN
            THROW 50001, 'Doctor is not available at the selected time.', 1;
        END;
    END;

    -- B. Validate No Double Booking (Simple check)
    IF EXISTS (
        SELECT 1 FROM tbl_Appointments 
        WHERE DoctorId = @DoctorId 
          AND Status IN ('Confirmed', 'Pending') 
          AND ABS(DATEDIFF(MINUTE, ScheduledTime, @ScheduledTime)) < 15 -- Assuming 15 min slots
    )
    BEGIN
        THROW 50002, 'Doctor is already booked at this time.', 1;
    END;

    -- C. Insert Appointment
    INSERT INTO tbl_Appointments (PatientId, DoctorId, ScheduledTime, Status, Reason)
    VALUES (@PatientId, @DoctorId, @ScheduledTime, 'Confirmed', @Reason);

    SELECT SCOPE_IDENTITY();
END;
GO

-- 3. SP: Get User Appointments (Ensure Status is returned & Include Reason)
CREATE OR ALTER PROCEDURE sp_GetUserAppointments
    @UserId INT,
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Role = 'Doctor'
    BEGIN
        SELECT 
            a.Id,
            a.ScheduledTime,
            a.Status,
            p.FullName AS CounterpartName,
            r.RoomId,
            a.Reason
        FROM tbl_Appointments a
        JOIN tbl_Users p ON a.PatientId = p.Id
        LEFT JOIN tbl_ConsultationRooms r ON r.AppointmentId = a.Id
        WHERE a.DoctorId = @UserId
        ORDER BY a.ScheduledTime DESC;
    END
    ELSE
    BEGIN
        SELECT 
            a.Id,
            a.ScheduledTime,
            a.Status,
            d.FullName AS CounterpartName,
            r.RoomId,
            a.Reason
        FROM tbl_Appointments a
        JOIN tbl_Users d ON a.DoctorId = d.Id
        LEFT JOIN tbl_ConsultationRooms r ON r.AppointmentId = a.Id
        WHERE a.PatientId = @UserId
        ORDER BY a.ScheduledTime DESC;
    END
END;
GO

-- 4. SP: Get Room Details (For Pre-Room)
CREATE OR ALTER PROCEDURE sp_GetRoomDetails
    @RoomId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.RoomId,
        a.Id AS AppointmentId,
        a.ScheduledTime,
        a.Status,
        a.Reason,
        p.FullName AS PatientName,
        d.FullName AS DoctorName,
        p.Id AS PatientId,
        d.Id AS DoctorId
    FROM tbl_ConsultationRooms r
    JOIN tbl_Appointments a ON r.AppointmentId = a.Id
    JOIN tbl_Users p ON a.PatientId = p.Id
    JOIN tbl_Users d ON a.DoctorId = d.Id
    WHERE r.RoomId = @RoomId;
END;
GO

-- 5. SP: Fix EndConsultation (ensure CREATE OR ALTER)
CREATE OR ALTER PROCEDURE sp_EndConsultation
    @RoomId UNIQUEIDENTIFIER,
    @ActionBy INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AppointmentId INT;
    SELECT @AppointmentId = AppointmentId FROM tbl_ConsultationRooms WHERE RoomId = @RoomId;

    IF @AppointmentId IS NULL
        THROW 50001, 'Room not found', 1;

    IF NOT EXISTS (SELECT 1 FROM tbl_Appointments WHERE Id = @AppointmentId AND DoctorId = @ActionBy)
    BEGIN
        INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
        VALUES ('EndConsultation', @ActionBy, 'Failed', GETUTCDATE(), 'Unauthorized');
        THROW 50001, 'Unauthorized', 1;
    END

    BEGIN TRANSACTION;

    UPDATE tbl_ConsultationRooms SET IsActive = 0 WHERE RoomId = @RoomId;
    UPDATE tbl_Appointments SET Status = 'Completed' WHERE Id = @AppointmentId;

    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('EndConsultation', @ActionBy, 'Success', GETUTCDATE(), CAST(@RoomId AS NVARCHAR(50)));

    COMMIT TRANSACTION;
END;
GO
