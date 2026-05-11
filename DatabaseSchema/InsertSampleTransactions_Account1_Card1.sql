/*
  BankAppDb schema (singular tables): Account, Card, Category, dbo.[Transaction]

  Sample transactions for AccountId = 1, CardId = 1, Currency = RON.
  - Inbound credit uses CardId NULL (allowed by FK).
  - Card purchases use CardId = @CardId.

  TransactionRef values are unique; script is safe to re-run (skips existing refs).

  If you use EF plural tables (Transactions, Accounts, Categories), rename targets in this script accordingly.
*/

USE BankAppDb;
GO

SET NOCOUNT ON;

DECLARE @AccountId   INT = 1;
DECLARE @CardId      INT = 1;
DECLARE @Currency    CHAR(3) = 'RON';

DECLARE @CategoryId INT =
(
    SELECT TOP (1) c.Id
    FROM dbo.Category AS c
    ORDER BY c.Id
);

IF @CategoryId IS NULL
BEGIN
    INSERT INTO dbo.Category (Name, Icon, IsSystem)
    VALUES ('General', 'default', 1);

    SET @CategoryId = SCOPE_IDENTITY();
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE TransactionRef = N'TXN-ACC1-2026-001')
BEGIN
    INSERT INTO dbo.[Transaction]
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
        N'TXN-ACC1-2026-001',
        N'Transfer',
        N'Credit',
        3000.00,
        @Currency,
        3000.00,
        N'Salary payer RO',
        N'RO61BTRL0000000000000001',
        NULL,
        @CategoryId,
        N'Salary / inbound transfer',
        0,
        NULL,
        N'Completed',
        NULL,
        NULL,
        DATEADD(HOUR, -72, SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE TransactionRef = N'TXN-ACC1-2026-002')
BEGIN
    INSERT INTO dbo.[Transaction]
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
        @CardId,
        N'TXN-ACC1-2026-002',
        N'CardPayment',
        N'Debit',
        125.50,
        @Currency,
        2874.50,
        NULL,
        NULL,
        N'Carrefour Orhideea',
        @CategoryId,
        N'Grocery shopping',
        0,
        NULL,
        N'Completed',
        NULL,
        NULL,
        DATEADD(HOUR, -48, SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE TransactionRef = N'TXN-ACC1-2026-003')
BEGIN
    INSERT INTO dbo.[Transaction]
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
        @CardId,
        N'TXN-ACC1-2026-003',
        N'CardPayment',
        N'Debit',
        42.00,
        @Currency,
        2832.50,
        NULL,
        NULL,
        N'Metrorex',
        @CategoryId,
        N'Transport top-up',
        0,
        NULL,
        N'Completed',
        NULL,
        NULL,
        DATEADD(HOUR, -24, SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[Transaction] WHERE TransactionRef = N'TXN-ACC1-2026-004')
BEGIN
    INSERT INTO dbo.[Transaction]
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
        @CardId,
        N'TXN-ACC1-2026-004',
        N'CardPayment',
        N'Debit',
        89.99,
        @Currency,
        2742.51,
        NULL,
        NULL,
        N'Altex',
        @CategoryId,
        N'Electronics',
        0,
        NULL,
        N'Completed',
        NULL,
        NULL,
        SYSUTCDATETIME()
    );
END;

UPDATE dbo.Account
SET Balance = 2742.51
WHERE Id = @AccountId;

PRINT N'Done: up to 4 rows in dbo.[Transaction]; dbo.Account balance set to 2742.51 for Id = ' + CAST(@AccountId AS VARCHAR(10)) + N'.';
GO
