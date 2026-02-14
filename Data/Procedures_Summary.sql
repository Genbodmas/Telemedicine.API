-- =========================================
-- Consultation Summary Procedures
-- =========================================

-- SP: Get Notes for an Appointment/Room
CREATE OR ALTER PROCEDURE sp_GetConsultationNotes
    @RoomId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        n.Id,
        n.Content,
        n.CreatedAt,
        u.FullName AS DoctorName
    FROM tbl_DoctorNotes n
    JOIN tbl_Appointments a ON n.AppointmentId = a.Id
    JOIN tbl_Users u ON a.DoctorId = u.Id
    JOIN tbl_ConsultationRooms r ON r.AppointmentId = a.Id
    WHERE r.RoomId = @RoomId
    ORDER BY n.CreatedAt DESC;
END;
GO
