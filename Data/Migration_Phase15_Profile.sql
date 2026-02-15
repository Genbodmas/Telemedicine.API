IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'Bio')
BEGIN
    ALTER TABLE tbl_Users ADD Bio NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE tbl_Users ADD PhoneNumber NVARCHAR(20) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'Address')
BEGIN
    ALTER TABLE tbl_Users ADD Address NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'Specialty')
BEGIN
    ALTER TABLE tbl_Users ADD Specialty NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'ProfilePictureUrl')
BEGIN
    ALTER TABLE tbl_Users ADD ProfilePictureUrl NVARCHAR(500) NULL;
END
GO
