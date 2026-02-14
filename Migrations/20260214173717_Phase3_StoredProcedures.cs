using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemedicine.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_StoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- sp_ValidateRoomAccess
CREATE PROCEDURE sp_ValidateRoomAccess
    @RoomId UNIQUEIDENTIFIER,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 
        FROM tbl_ConsultationRooms R
        JOIN tbl_Appointments A ON R.AppointmentId = A.Id
        WHERE R.RoomId = @RoomId 
          AND R.IsActive = 1
          AND (A.DoctorId = @UserId OR A.PatientId = @UserId)
    )
        RETURN 1;
    ELSE
        RETURN 0;
END
GO

-- sp_CreateConsultationRoom
CREATE PROCEDURE sp_CreateConsultationRoom
    @AppointmentId INT,
    @ActionBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RoomId UNIQUEIDENTIFIER = NEWID();

    -- Validate User is Doctor for this appointment
    IF NOT EXISTS (SELECT 1 FROM tbl_Appointments WHERE Id = @AppointmentId AND DoctorId = @ActionBy)
    BEGIN
        INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
        VALUES ('CreateRoom', @ActionBy, 'Failed', GETUTCDATE(), 'Unauthorized: Not the assigned doctor');
        THROW 50001, 'Unauthorized', 1;
    END

    BEGIN TRANSACTION;
    
    INSERT INTO tbl_ConsultationRooms (RoomId, AppointmentId, IsActive, CreatedAt)
    VALUES (@RoomId, @AppointmentId, 1, GETUTCDATE());

    UPDATE tbl_Appointments SET Status = 'Active' WHERE Id = @AppointmentId;

    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('CreateRoom', @ActionBy, 'Success', GETUTCDATE(), CAST(@RoomId AS NVARCHAR(50)));

    COMMIT TRANSACTION;

    SELECT @RoomId AS RoomId;
END
GO

-- sp_EndConsultation
CREATE PROCEDURE sp_EndConsultation
    @RoomId UNIQUEIDENTIFIER,
    @ActionBy INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate User is Doctor
    DECLARE @AppointmentId INT;
    SELECT @AppointmentId = AppointmentId FROM tbl_ConsultationRooms WHERE RoomId = @RoomId;

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
END
GO

-- sp_JoinRoom (Wrapper for audit)
CREATE PROCEDURE sp_JoinRoom
    @RoomId UNIQUEIDENTIFIER,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsValid INT;
    EXEC @IsValid = sp_ValidateRoomAccess @RoomId, @UserId;

    IF @IsValid = 1
    BEGIN
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
END
GO

-- sp_AddChat
CREATE PROCEDURE sp_AddChat
    @RoomId UNIQUEIDENTIFIER,
    @SenderId INT,
    @Message NVARCHAR(MAX),
    @FileUrl NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsValid INT;
    EXEC @IsValid = sp_ValidateRoomAccess @RoomId, @SenderId;

    IF @IsValid = 0
        THROW 50001, 'Unauthorized', 1;

    INSERT INTO tbl_Chats (RoomId, SenderId, Message, FileUrl, Timestamp)
    VALUES (@RoomId, @SenderId, @Message, @FileUrl, GETUTCDATE());

    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('SendMessage', @SenderId, 'Success', GETUTCDATE(), CAST(@RoomId AS NVARCHAR(50)));
END
GO

-- sp_AddDoctorNote
CREATE PROCEDURE sp_AddDoctorNote
    @AppointmentId INT,
    @DoctorId INT,
    @Content NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate User is Doctor for this appointment
    IF NOT EXISTS (SELECT 1 FROM tbl_Appointments WHERE Id = @AppointmentId AND DoctorId = @DoctorId)
    BEGIN
        INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
        VALUES ('AddNote', @DoctorId, 'Failed', GETUTCDATE(), 'Unauthorized: Not the assigned doctor');
        THROW 50001, 'Unauthorized', 1;
    END

    INSERT INTO tbl_DoctorNotes (AppointmentId, Content, CreatedAt)
    VALUES (@AppointmentId, @Content, GETUTCDATE());

    INSERT INTO tbl_Audits (Action, ActionBy, ActionStatus, ActionAt, Details)
    VALUES ('AddNote', @DoctorId, 'Success', GETUTCDATE(), CAST(@AppointmentId AS NVARCHAR(50)));
END
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP PROCEDURE IF EXISTS sp_AddDoctorNote;
DROP PROCEDURE IF EXISTS sp_AddChat;
DROP PROCEDURE IF EXISTS sp_JoinRoom;
DROP PROCEDURE IF EXISTS sp_EndConsultation;
DROP PROCEDURE IF EXISTS sp_CreateConsultationRoom;
DROP PROCEDURE IF EXISTS sp_ValidateRoomAccess;
");
        }
    }
}
