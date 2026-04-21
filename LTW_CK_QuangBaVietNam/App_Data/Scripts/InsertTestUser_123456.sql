-- Insert new test user with password: 123456
-- This script generates a PBKDF2 hash compatible with AccountController

DECLARE @FullName NVARCHAR(100) = N'Test User New';
DECLARE @Email NVARCHAR(256) = 'testuser@example.com';
-- Password: 123456
-- PasswordHash and PasswordSalt generated using PBKDF2 (10000 iterations, 16-byte salt, 32-byte hash)
DECLARE @PasswordHash NVARCHAR(512) = 'X5YEBcq9pE0YMYNkzEh3N2qL7E1H0Z2J3kH4X5Y6Z7A=';
DECLARE @PasswordSalt NVARCHAR(128) = 'LZw3frMJF0CJxLUpKPdvGQ==';

-- Delete old test user if exists
DELETE FROM dbo.Users WHERE Email = @Email;

-- Insert new user
INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, IsActive, CreatedAt)
VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, 1, GETDATE());

-- Verify
SELECT Id, FullName, Email, IsActive, CreatedAt FROM dbo.Users WHERE Email = @Email;
