use BankAppDb
go
select * from Users
select * from Accounts
select * from Cards
    DECLARE @FullName varchar(255)='test1'
    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @SeedStart DATETIME2(0) = CAST(DATEADD(DAY, -10, CAST(@Now AS DATE)) AS DATETIME2(0));
    DECLARE @UserId INT=1
    DECLARE @AccountId INT=1;
    DECLARE @CardId INT;
    DECLARE @InsertedTransactions INT = 0;
    DECLARE @UserSuffix VARCHAR(8) = RIGHT(REPLICATE('0', 8) + CAST(@UserId AS VARCHAR(8)), 8);
    DECLARE @AccountIban VARCHAR(34) = CONCAT('RO49BAPP', RIGHT(REPLICATE('0', 22) + CAST(@UserId AS VARCHAR(22)), 22));
    DECLARE @CardNumber VARCHAR(19) = CONCAT('55554444', @UserSuffix);
    DECLARE @CardholderName NVARCHAR(200) = LEFT(COALESCE(NULLIF(@FullName, N''), N'Demo User'), 200);
            INSERT INTO Cards
        (
            AccountId,
            UserId,
            CardNumber,
            CardholderName,
            ExpiryDate,
            CVV,
            CardType,
            CardBrand,
            Status,
            DailyTransactionLimit,
            MonthlySpendingCap,
            AtmWithdrawalLimit,
            ContactlessLimit,
            IsContactlessEnabled,
            IsOnlineEnabled,
            SortOrder,
            CreatedAt
        )
        VALUES
        (
            @AccountId,
            @UserId,
            @CardNumber,
            @CardholderName,
            '2030-12-31',
            '123',
            'Debit',
            'Mastercard',
            'Active',
            1500,
            5000,
            1000,
            200,
            1,
            1,
            0,
            @SeedStart
        );

        SET @CardId = SCOPE_IDENTITY();