use BankAppDb
go
select * from Users
select * from Accounts
select * from Cards
    DECLARE @FullName varchar(255)='test1'
    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @SeedStart DATETIME2(0) = CAST(DATEADD(DAY, -10, CAST(@Now AS DATE)) AS DATETIME2(0));
    DECLARE @UserId INT=1
    DECLARE @AccountId INT;
    DECLARE @CardId INT;
    DECLARE @InsertedTransactions INT = 0;
    DECLARE @UserSuffix VARCHAR(8) = RIGHT(REPLICATE('0', 8) + CAST(@UserId AS VARCHAR(8)), 8);
    DECLARE @AccountIban VARCHAR(34) = CONCAT('RO49BAPP', RIGHT(REPLICATE('0', 22) + CAST(@UserId AS VARCHAR(22)), 22));
    DECLARE @CardNumber VARCHAR(19) = CONCAT('55554444', @UserSuffix);
    DECLARE @CardholderName NVARCHAR(200) = LEFT(COALESCE(NULLIF(@FullName, N''), N'Demo User'), 200);
        INSERT INTO Accounts
        (
            UserId,
            AccountName,
            IBAN,
            Currency,
            Balance,
            AccountType,
            Status,
            CreatedAt
        )
        VALUES
        (
            @UserId,
            'Main Checking',
            @AccountIban,
            'RON',
            0,
            'Checking',
            'Active',
            @SeedStart
        );

        SET @AccountId = SCOPE_IDENTITY();