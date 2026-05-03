USE BankAppDb
GO


IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = 'UserCardPreference'
)
BEGIN
    CREATE TABLE UserCardPreference
    (
        UserId INT NOT NULL PRIMARY KEY,
        SortOption VARCHAR(50) NOT NULL CONSTRAINT DF_UserCardPreference_SortOption DEFAULT 'custom',
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_UserCardPreference_UpdatedAt DEFAULT GETUTCDATE(),
        CONSTRAINT FK_UserCardPreference_User FOREIGN KEY (UserId) REFERENCES [User](Id)
    );
END;
