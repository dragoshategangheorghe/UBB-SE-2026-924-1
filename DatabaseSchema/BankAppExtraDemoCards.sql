USE BankAppDb
GO


SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @UserId INT;
DECLARE @AccountId INT;

SELECT TOP (1)
    @UserId = u.Id
FROM [User] u
WHERE u.Email = 'test1@example.com'
ORDER BY u.Id;

IF @UserId IS NULL
BEGIN
    RAISERROR('User test1@example.com was not found.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

SELECT TOP (1)
    @AccountId = a.Id
FROM Account a
WHERE a.UserId = @UserId
ORDER BY a.Id;

IF @AccountId IS NULL
BEGIN
    RAISERROR('No account was found for test1@example.com.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM Card WHERE CardNumber = '4111222200000002')
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
        '4111222200000002',
        'Jodasd dd',
        '2029-08-31',
        '456',
        'Debit',
        'Visa',
        'Active',
        3000,
        7000,
        1500,
        300,
        1,
        1,
        1,
        SYSUTCDATETIME()
    );
END;

IF NOT EXISTS (SELECT 1 FROM Card WHERE CardNumber = '5333444400000003')
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
        '5333444400000003',
        'Jodasd dd',
        '2031-04-30',
        '789',
        'Credit',
        'Mastercard',
        'Active',
        5000,
        12000,
        1000,
        250,
        0,
        1,
        2,
        SYSUTCDATETIME()
    );
END;

COMMIT TRANSACTION;

SELECT
    c.Id,
    c.CardholderName,
    c.CardNumber,
    c.CardBrand,
    c.CardType,
    c.Status,
    c.SortOrder
FROM Card c
WHERE c.UserId = @UserId
ORDER BY c.SortOrder, c.Id;
