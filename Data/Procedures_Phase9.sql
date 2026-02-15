-- =============================================
-- Phase 9: Logic Fixes (Pre-Room Status, etc)
-- =============================================

-- 1. SP: Join Room (Update Status to 'In Progress' when Doctor joins)
CREATE OR ALTER PROCEDURE sp_JoinRoom
    @RoomId UNIQUEIDENTIFIER,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsValid INT;
    EXEC @IsValid = sp_ValidateRoomAccess @RoomId, @UserId;

    IF @IsValid = 1
    BEGIN
        -- Check if user is the DOCTOR
        DECLARE @AppointmentId INT;
        DECLARE @DoctorId INT;
        
        SELECT @AppointmentId = r.AppointmentId, @DoctorId = a.DoctorId
        FROM tbl_ConsultationRooms r
        JOIN tbl_Appointments a ON r.AppointmentId = a.Id
        WHERE r.RoomId = @RoomId;
        
        IF @UserId = @DoctorId
        BEGIN
            -- Update status to In Progress
            UPDATE tbl_Appointments 
            SET Status = 'In Progress' 
            WHERE Id = @AppointmentId AND Status IN ('Confirmed', 'Pending');
        END

        INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
        VALUES ('JoinRoom', @UserId, 'Success', GETUTCDATE(), CAST(@RoomId AS NVARCHAR(50)));
        
        SELECT 1 AS Result;
    END
    ELSE
    BEGIN
        INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
        VALUES ('JoinRoom', @UserId, 'Failed', GETUTCDATE(), CAST(@RoomId AS NVARCHAR(50)));
        SELECT 0 AS Result;
    END
END;
GO
