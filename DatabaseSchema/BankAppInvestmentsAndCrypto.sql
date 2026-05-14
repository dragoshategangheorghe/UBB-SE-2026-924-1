USE BankAppDb;
GO

-- Drop tables in reverse order to avoid foreign key constraint violations if old structures exist
IF OBJECT_ID('dbo.InvestmentTransaction', 'U') IS NOT NULL DROP TABLE dbo.InvestmentTransaction;
IF OBJECT_ID('dbo.InvestmentHolding', 'U') IS NOT NULL DROP TABLE dbo.InvestmentHolding;
IF OBJECT_ID('dbo.Portfolio', 'U') IS NOT NULL DROP TABLE dbo.Portfolio;
GO

-- ============================================================================
-- 1. Create Table: Portfolio
-- ============================================================================
CREATE TABLE dbo.Portfolio (
    id INT IDENTITY(1,1) NOT NULL,
    userId INT NOT NULL,
    totalValue DECIMAL(18, 2) NULL DEFAULT 0.00,
    totalGainLoss DECIMAL(18, 2) NULL DEFAULT 0.00,
    gainLossPercent DECIMAL(18, 4) NULL DEFAULT 0.0000,
    
    CONSTRAINT PK_Portfolio PRIMARY KEY CLUSTERED (id ASC),
    -- Relationship with your main User table
    CONSTRAINT FK_Portfolio_User FOREIGN KEY (userId) REFERENCES dbo.[User](id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 2. Create Table: InvestmentHolding
-- ============================================================================
CREATE TABLE dbo.InvestmentHolding (
    id INT IDENTITY(1,1) NOT NULL,
    portfolioId INT NOT NULL,
    ticker NVARCHAR(50) NOT NULL,
    assetType NVARCHAR(20) NOT NULL, -- Stores 'Stock' or 'Crypto' (Singular to match UI filters)
    quantity DECIMAL(18, 4) NOT NULL, -- Supports fractional decimal units for crypto positions (e.g., 0.50 ETH)
    avgPurchasePrice DECIMAL(18, 2) NOT NULL,
    currentPrice DECIMAL(18, 2) NOT NULL,
    
    CONSTRAINT PK_InvestmentHolding PRIMARY KEY CLUSTERED (id ASC),
    CONSTRAINT FK_InvestmentHolding_Portfolio FOREIGN KEY (portfolioId) REFERENCES dbo.Portfolio(id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 3. Create Table: InvestmentTransaction
-- ============================================================================
CREATE TABLE dbo.InvestmentTransaction (
    id INT IDENTITY(1,1) NOT NULL,
    holdingId INT NOT NULL,
    ticker NVARCHAR(50) NOT NULL,
    actionType NVARCHAR(20) NOT NULL, -- Stores actions like 'BUY' or 'SELL'
    quantity DECIMAL(18, 4) NOT NULL,
    pricePerUnit DECIMAL(18, 2) NOT NULL,
    fees DECIMAL(18, 2) NOT NULL,
    orderType NVARCHAR(20) NOT NULL DEFAULT 'Market',
    executedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT PK_InvestmentTransaction PRIMARY KEY CLUSTERED (id ASC),
    CONSTRAINT FK_InvestmentTransaction_InvestmentHolding FOREIGN KEY (holdingId) REFERENCES dbo.InvestmentHolding(id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 4. Insert Initial Seed Data for Testing
-- ============================================================================
-- This automatically seeds the exact starting portfolio data for User ID 1
IF EXISTS (SELECT 1 FROM dbo.[User] WHERE id = 1)
BEGIN
    INSERT INTO dbo.Portfolio (userId, totalValue, totalGainLoss, gainLossPercent)
    VALUES (1, 3200.00, 200.00, 0.0667);

    DECLARE @NewPortfolioId INT = SCOPE_IDENTITY();

    -- Insert standard initial Ethereum holding row (Crypto)
    INSERT INTO dbo.InvestmentHolding (portfolioId, ticker, assetType, quantity, avgPurchasePrice, currentPrice)
    VALUES (@NewPortfolioId, 'ETH', 'Crypto', 0.50, 2400.00, 2550.00);

    -- Insert standard initial Apple holding row (Stock)
    INSERT INTO dbo.InvestmentHolding (portfolioId, ticker, assetType, quantity, avgPurchasePrice, currentPrice)
    VALUES (@NewPortfolioId, 'AAPL', 'Stock', 10.00, 180.00, 192.50);
    
    PRINT 'Portfolio mock test balances seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Notice: Portfolio tables created, but seed records skipped because User ID 1 does not exist in dbo.[User].';
END
GO
