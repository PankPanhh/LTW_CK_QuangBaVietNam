IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [PasswordHash] NVARCHAR(512) NOT NULL,
        [PasswordSalt] NVARCHAR(128) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT(1),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT(GETDATE()),
        [UpdatedAt] DATETIME NULL,
        [LastLoginAt] DATETIME NULL
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Users_Email'
      AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Users_Email]
        ON [dbo].[Users]([Email]);
END
GO
