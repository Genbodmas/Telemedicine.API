-- sp_GetChatHistory
CREATE OR ALTER PROCEDURE sp_GetChatHistory
    @RoomId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        c.SenderId,
        c.Message,
        c.FileUrl,
        c.Timestamp,
        u.FullName AS SenderName
    FROM tbl_Chats c
    JOIN tbl_Users u ON c.SenderId = u.Id
    WHERE c.RoomId = @RoomId
    ORDER BY c.Timestamp ASC;
END;
GO
