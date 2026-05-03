USE BankAppDb
GO


SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
DECLARE @SeedStart DATETIME2(0) = CAST(DATEADD(DAY, -10, CAST(@Now AS DATE)) AS DATETIME2(0));

DECLARE @CategorySeed TABLE
(
    Name VARCHAR(100) NOT NULL,
    Icon VARCHAR(50) NULL
);

INSERT INTO @CategorySeed (Name, Icon)
VALUES
    ('Groceries', 'cart'),
    ('Transport', 'car'),
    ('Utilities', 'bolt'),
    ('Shopping', 'bag'),
    ('Dining', 'coffee');

INSERT INTO Category (Name, Icon, IsSystem)
SELECT seed.Name, seed.Icon, 1
FROM @CategorySeed seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM Category existing
    WHERE existing.Name = seed.Name
);

DECLARE @SeedResults TABLE
(
    UserId INT NOT NULL,
    Email VARCHAR(255) NOT NULL,
    AccountId INT NOT NULL,
    CardId INT NOT NULL,
    InsertedTransactions INT NOT NULL
);

DECLARE @UserId INT;
DECLARE @Email VARCHAR(255);
DECLARE @FullName NVARCHAR(200);

DECLARE user_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT Id, Email, FullName
FROM [User]
ORDER BY Id;

OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @UserId, @Email, @FullName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @AccountId INT;
    DECLARE @CardId INT;
    DECLARE @InsertedTransactions INT = 0;
    DECLARE @UserSuffix VARCHAR(8) = RIGHT(REPLICATE('0', 8) + CAST(@UserId AS VARCHAR(8)), 8);
    DECLARE @AccountIban VARCHAR(34) = CONCAT('RO49BAPP', RIGHT(REPLICATE('0', 22) + CAST(@UserId AS VARCHAR(22)), 22));
    DECLARE @CardNumber VARCHAR(19) = CONCAT('55554444', @UserSuffix);
    DECLARE @CardholderName NVARCHAR(200) = LEFT(COALESCE(NULLIF(@FullName, N''), N'Demo User'), 200);

    SELECT TOP (1) @AccountId = Id
    FROM Account
    WHERE UserId = @UserId
    ORDER BY Id;

    IF @AccountId IS NULL
    BEGIN
        INSERT INTO Account
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
    END;

    SELECT TOP (1) @CardId = Id
    FROM Card
    WHERE UserId = @UserId
      AND AccountId = @AccountId
    ORDER BY SortOrder, Id;

    IF @CardId IS NULL
    BEGIN
        INSERT INTO Card
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
    END;

    IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE AccountId = @AccountId)
    BEGIN
        DECLARE @GroceriesId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Groceries' ORDER BY Id);
        DECLARE @TransportId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Transport' ORDER BY Id);
        DECLARE @UtilitiesId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Utilities' ORDER BY Id);
        DECLARE @ShoppingId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Shopping' ORDER BY Id);
        DECLARE @DiningId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Dining' ORDER BY Id);

        INSERT INTO [Transaction]
        (
            AccountId,
            CardId,
            TransactionRef,
            [Type],
            Direction,
            Amount,
            Currency,
            BalanceAfter,
            CounterpartyName,
            CounterpartyIBAN,
            MerchantName,
            CategoryId,
            [Description],
            Fee,
            ExchangeRate,
            Status,
            RelatedEntityType,
            RelatedEntityId,
            CreatedAt
        )
        VALUES
        (
            @AccountId,
            NULL,
            CONCAT('DEMO-', @UserSuffix, '-001'),
            'Salary',
            'Credit',
            5000.00,
            'RON',
            5000.00,
            N'PixelForge SRL',
            'RO61BTRL0000000000000001',
            NULL,
            NULL,
            N'March salary payment',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 8, @SeedStart)
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-002'),
            'CardPayment',
            'Debit',
            185.40,
            'RON',
            4814.60,
            NULL,
            NULL,
            N'Fresh Market Titan',
            @GroceriesId,
            N'Weekly groceries',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 12, DATEADD(DAY, 1, @SeedStart))
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-003'),
            'CardPayment',
            'Debit',
            28.00,
            'RON',
            4786.60,
            NULL,
            NULL,
            N'Metrorex',
            @TransportId,
            N'Metro card top-up',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 9, DATEADD(DAY, 2, @SeedStart))
        ),
        (
            @AccountId,
            NULL,
            CONCAT('DEMO-', @UserSuffix, '-004'),
            'Transfer',
            'Debit',
            312.75,
            'RON',
            4473.85,
            N'Electrica Furnizare',
            'RO94INGB0000000000000004',
            NULL,
            @UtilitiesId,
            N'Utilities bill payment',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 18, DATEADD(DAY, 3, @SeedStart))
        ),
        (
            @AccountId,
            NULL,
            CONCAT('DEMO-', @UserSuffix, '-005'),
            'Transfer',
            'Debit',
            450.00,
            'RON',
            4023.85,
            N'Alex Popescu',
            'RO09BAPP0000000000000005',
            NULL,
            NULL,
            N'Rent split transfer',
            2.50,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 11, DATEADD(DAY, 4, @SeedStart))
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-006'),
            'CardPayment',
            'Debit',
            249.99,
            'RON',
            3773.86,
            NULL,
            NULL,
            N'eMAG',
            @ShoppingId,
            N'Wireless headphones order',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 15, DATEADD(DAY, 5, @SeedStart))
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-007'),
            'CardPayment',
            'Debit',
            89.90,
            'RON',
            3773.86,
            NULL,
            NULL,
            N'Steam',
            @ShoppingId,
            N'Game purchase failed',
            0,
            NULL,
            'Failed',
            NULL,
            NULL,
            DATEADD(HOUR, 21, DATEADD(DAY, 6, @SeedStart))
        ),
        (
            @AccountId,
            NULL,
            CONCAT('DEMO-', @UserSuffix, '-008'),
            'Transfer',
            'Debit',
            120.00,
            'RON',
            3773.86,
            N'Orange Romania',
            'RO49BAPP0000000000000008',
            NULL,
            @UtilitiesId,
            N'Mobile bill reversed',
            0,
            NULL,
            'Reversed',
            NULL,
            NULL,
            DATEADD(HOUR, 10, DATEADD(DAY, 7, @SeedStart))
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-009'),
            'CardPayment',
            'Debit',
            75.20,
            'RON',
            3773.86,
            NULL,
            NULL,
            N'Nor Sky Casual Restaurant',
            @DiningId,
            N'Dinner payment pending settlement',
            0,
            NULL,
            'Pending',
            NULL,
            NULL,
            DATEADD(HOUR, 20, DATEADD(DAY, 8, @SeedStart))
        ),
        (
            @AccountId,
            NULL,
            CONCAT('DEMO-', @UserSuffix, '-010'),
            'Refund',
            'Credit',
            150.00,
            'RON',
            3923.86,
            N'eMAG Marketplace',
            'RO32BAPP0000000000000010',
            NULL,
            @ShoppingId,
            N'Order refund',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 13, DATEADD(DAY, 9, @SeedStart))
        ),
        (
            @AccountId,
            @CardId,
            CONCAT('DEMO-', @UserSuffix, '-011'),
            'CardPayment',
            'Debit',
            18.50,
            'RON',
            3905.36,
            NULL,
            NULL,
            N'Boiler Coffee Corner',
            @DiningId,
            N'Coffee and snack',
            0,
            NULL,
            'Completed',
            NULL,
            NULL,
            DATEADD(HOUR, 9, DATEADD(DAY, 10, @SeedStart))
        );

        SET @InsertedTransactions = 11;

        UPDATE Account
        SET Balance = 3905.36
        WHERE Id = @AccountId;
    END;

    INSERT INTO @SeedResults (UserId, Email, AccountId, CardId, InsertedTransactions)
    VALUES (@UserId, @Email, @AccountId, @CardId, @InsertedTransactions);

    FETCH NEXT FROM user_cursor INTO @UserId, @Email, @FullName;
END;

CLOSE user_cursor;
DEALLOCATE user_cursor;

COMMIT TRANSACTION;

SELECT
    UserId,
    Email,
    AccountId,
    CardId,
    InsertedTransactions
FROM @SeedResults
ORDER BY UserId;
