-- Insert a test user into dbo.Users for login testing
-- Replace the values for @FullName, @Email, @PasswordHash and @PasswordSalt
-- You can generate @PasswordHash and @PasswordSalt using the provided Tools/GenerateUserSql.cs utility.

SET NOCOUNT ON;

DECLARE @FullName NVARCHAR(100) = N'Test User';
DECLARE @Email NVARCHAR(256) = 'testuser@example.com';
DECLARE @PasswordHash NVARCHAR(512) = '<PASSWORD_HASH_BASE64>'; -- replace with generated base64 hash
DECLARE @PasswordSalt NVARCHAR(128) = '<SALT_BASE64>'; -- replace with generated base64 salt

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, IsActive, CreatedAt)
    VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, 1, GETDATE());
    PRINT 'Inserted test user: ' + @Email;
END
ELSE
BEGIN
    PRINT 'User with this email already exists.';
END
GO

-- Example usage:
-- 1) Generate hash/salt using Tools\GenerateUserSql.exe:
--    GenerateUserSql.exe "Test User" testuser@example.com "Test@1234"
-- 2) Replace <PASSWORD_HASH_BASE64> and <SALT_BASE64> above with the values printed by the tool.
-- 3) Run this script on the database pointed by DefaultConnection (e.g. aspnet-LTW_CK_QuangBaVietNam) in SSMS.
