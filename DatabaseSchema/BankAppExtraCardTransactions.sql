USE BankAppDb
GO


SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @UserId INT = 1;
DECLARE @AccountId INT = 1;
DECLARE @Card2Id INT = 2;
DECLARE @Card3Id INT = 3;

DECLARE @GroceriesId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Groceries' ORDER BY Id);
DECLARE @TransportId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Transport' ORDER BY Id);
DECLARE @ShoppingId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Shopping' ORDER BY Id);
DECLARE @DiningId INT = (SELECT TOP (1) Id FROM Category WHERE Name = 'Dining' ORDER BY Id);

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'DEMO-00000001-012')
BEGIN
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
        @Card2Id,
        'DEMO-00000001-012',
        'CardPayment',
        'Debit',
        132.40,
        'RON',
        3772.96,
        NULL,
        NULL,
        N'Mega Fresh Market',
        @GroceriesId,
        N'Groceries paid with second demo card',
        0,
        NULL,
        'Completed',
        NULL,
        NULL,
        '2026-03-28T18:30:00'
    );
END;

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'DEMO-00000001-013')
BEGIN
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
        @Card2Id,
        'DEMO-00000001-013',
        'CardPayment',
        'Debit',
        46.80,
        'RON',
        3726.16,
        NULL,
        NULL,
        N'City Taxi',
        @TransportId,
        N'Taxi ride paid with second demo card',
        0,
        NULL,
        'Completed',
        NULL,
        NULL,
        '2026-03-29T09:15:00'
    );
END;

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'DEMO-00000001-014')
BEGIN
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
        @Card3Id,
        'DEMO-00000001-014',
        'CardPayment',
        'Debit',
        520.00,
        'RON',
        3206.16,
        NULL,
        NULL,
        N'TechHub',
        @ShoppingId,
        N'Electronics purchase paid with third demo card',
        0,
        NULL,
        'Completed',
        NULL,
        NULL,
        '2026-03-31T14:20:00'
    );
END;

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'DEMO-00000001-015')
BEGIN
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
        @Card3Id,
        'DEMO-00000001-015',
        'Refund',
        'Credit',
        120.00,
        'RON',
        3326.16,
        N'TechHub Returns',
        'RO12BAPP0000000000000015',
        NULL,
        @ShoppingId,
        N'Refund received on third demo card',
        0,
        NULL,
        'Completed',
        NULL,
        NULL,
        '2026-04-01T10:40:00'
    );
END;

IF NOT EXISTS (SELECT 1 FROM [Transaction] WHERE TransactionRef = 'DEMO-00000001-016')
BEGIN
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
        @Card3Id,
        'DEMO-00000001-016',
        'CardPayment',
        'Debit',
        58.90,
        'RON',
        3267.26,
        NULL,
        NULL,
        N'Bistro 27',
        @DiningId,
        N'Lunch paid with third demo card',
        0,
        NULL,
        'Completed',
        NULL,
        NULL,
        '2026-04-02T08:50:00'
    );
END;

UPDATE Account
SET Balance = 3267.26
WHERE Id = @AccountId;

COMMIT TRANSACTION;

SELECT
    c.Id,
    c.CardNumber,
    c.CardBrand,
    c.CardType,
    COUNT(t.Id) AS TxCount
FROM Card c
LEFT JOIN [Transaction] t ON t.CardId = c.Id
WHERE c.UserId = @UserId
GROUP BY c.Id, c.CardNumber, c.CardBrand, c.CardType
ORDER BY c.Id;
