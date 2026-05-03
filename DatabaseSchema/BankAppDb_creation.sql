CREATE DATABASE BankAppDb
go
USE BankAppDb
GO

CREATE TABLE [User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(512) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(20),
    DateOfBirth DATE,
    [Address] NVARCHAR(MAX),
    Nationality VARCHAR(100),
    PreferredLanguage VARCHAR(5) DEFAULT 'en',
    Is2FAEnabled BIT DEFAULT 0,
    Preferred2FAMethod VARCHAR(20),
    IsLocked BIT DEFAULT 0,
    LockoutEnd DATETIME2 NULL,
    FailedLoginAttempts INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE [Session] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Token VARCHAR(512) NOT NULL,
    DeviceInfo VARCHAR(255),
    Browser VARCHAR(100),
    IpAddress VARCHAR(45),
    LastActiveAt DATETIME2,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE OAuthLink (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Provider VARCHAR(20) NOT NULL,
    ProviderUserId VARCHAR(255) NOT NULL,
    ProviderEmail VARCHAR(255),
    LinkedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Account (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    AccountName VARCHAR(100),
    IBAN VARCHAR(34) NOT NULL UNIQUE,
    Currency VARCHAR(3) NOT NULL,
    Balance DECIMAL(18,2) DEFAULT 0,
    AccountType VARCHAR(20) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Card (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL FOREIGN KEY REFERENCES Account(Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    CardNumber VARCHAR(19) NOT NULL,
    CardholderName NVARCHAR(200) NOT NULL,
    ExpiryDate DATE NOT NULL,
    CVV VARCHAR(4) NOT NULL,
    CardType VARCHAR(20) NOT NULL,
    CardBrand VARCHAR(20),
    Status VARCHAR(20) DEFAULT 'Active',
    DailyTransactionLimit DECIMAL(18,2),
    MonthlySpendingCap DECIMAL(18,2),
    AtmWithdrawalLimit DECIMAL(18,2),
    ContactlessLimit DECIMAL(18,2),
    IsContactlessEnabled BIT DEFAULT 1,
    IsOnlineEnabled BIT DEFAULT 1,
    SortOrder INT DEFAULT 0,
    CancelledAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Category (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Icon VARCHAR(50),
    IsSystem BIT DEFAULT 1
);

CREATE TABLE [Transaction] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL FOREIGN KEY REFERENCES Account(Id),
    CardId INT NULL FOREIGN KEY REFERENCES Card(Id),
    TransactionRef VARCHAR(50) NOT NULL UNIQUE,
    [Type] VARCHAR(30) NOT NULL,
    Direction VARCHAR(10) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    BalanceAfter DECIMAL(18,2) NOT NULL,
    CounterpartyName NVARCHAR(200),
    CounterpartyIBAN VARCHAR(34),
    MerchantName NVARCHAR(200),
    CategoryId INT NULL FOREIGN KEY REFERENCES Category(Id),
    [Description] NVARCHAR(MAX),
    Fee DECIMAL(18,2) DEFAULT 0,
    ExchangeRate DECIMAL(18,6),
    Status VARCHAR(20) NOT NULL,
    RelatedEntityType VARCHAR(50),
    RelatedEntityId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Notification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Title NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Type] VARCHAR(30) NOT NULL,
    Channel VARCHAR(20) NOT NULL,
    IsRead BIT DEFAULT 0,
    RelatedEntityType VARCHAR(50),
    RelatedEntityId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE NotificationPreference (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Category VARCHAR(30) NOT NULL,
    PushEnabled BIT DEFAULT 1,
    EmailEnabled BIT DEFAULT 1,
    SmsEnabled BIT DEFAULT 0,
    MinAmountThreshold DECIMAL(18,2)
);

CREATE TABLE PasswordResetToken (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    TokenHash VARCHAR(512) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    UsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE TransactionCategoryOverride (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId INT NOT NULL FOREIGN KEY REFERENCES [Transaction](Id),
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Category(Id)
);
CREATE TABLE Loan (
    id INT PRIMARY KEY IDENTITY(1,1),
    userId INT NOT NULL,
    loanType NVARCHAR(50),
    principal DECIMAL(18,2),
    outstandingBalance DECIMAL(18,2),
    interestRate DECIMAL(5,2),
    monthlyInstallment DECIMAL(18,2),
    remainingMonths INT,
    loanStatus NVARCHAR(30),
    termInMonths INT,
    startDate DATETIME2,
    CONSTRAINT FK_Loan_User FOREIGN KEY (userId) REFERENCES [User](Id)
);
GO

CREATE TABLE LoanApplication (
    id INT PRIMARY KEY IDENTITY(1,1),
    userId INT NOT NULL,
    loanType NVARCHAR(50),
    desiredAmount DECIMAL(18,2),
    preferredTermMonths INT,
    purpose NVARCHAR(255),
    applicationStatus NVARCHAR(30),
    rejectionReason NVARCHAR(255),
    CONSTRAINT FK_LoanApplication_User FOREIGN KEY (userId) REFERENCES [User](Id)
);
GO

CREATE TABLE AmortizationRow (
    id INT PRIMARY KEY IDENTITY(1,1),
    loanId INT NOT NULL,
    installmentNumber INT,
    dueDate DATETIME2,
    principalPortion DECIMAL(18,2),
    interestPortion DECIMAL(18,2),
    remainingBalance DECIMAL(18,2),
    CONSTRAINT FK_AmortizationRow_Loan FOREIGN KEY (loanId) REFERENCES Loan(id)
);
GO

CREATE TABLE SavingsAccount (
    id INT PRIMARY KEY IDENTITY(1,1),
    userId INT NOT NULL,
    savingsType NVARCHAR(50),
    balance DECIMAL(18,2),
    accruedInterest DECIMAL(18,2),
    apy DECIMAL(18,2),
    maturityDate DATE,
    accountStatus NVARCHAR(30),
    createdAt DATETIME2,
    updatedAt DATETIME2,
    accountName NVARCHAR(100),
    fundingAccountId INT,
    targetAmount DECIMAL(18,2),
    targetDate DATE,
    CONSTRAINT FK_SavingsAccount_User FOREIGN KEY (userId) REFERENCES [User](Id),
    CONSTRAINT FK_SavingsAccount_FundingAccount FOREIGN KEY (fundingAccountId) REFERENCES Account(Id)
);
GO

CREATE TABLE SavingsTransaction (
    id INT PRIMARY KEY IDENTITY(1,1),
    accountId INT NOT NULL,
    transactionType NVARCHAR(20) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    balanceAfter DECIMAL(18,2) NOT NULL,
    source NVARCHAR(50),
    description NVARCHAR(255),
    createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SavingsTransaction_SavingsAccount FOREIGN KEY (accountId) REFERENCES SavingsAccount(id)
);
GO

CREATE TABLE InterestLog (
    id INT PRIMARY KEY IDENTITY(1,1),
    accountId INT NOT NULL,
    interestAmount DECIMAL(18,2) NOT NULL,
    balanceBefore DECIMAL(18,2) NOT NULL,
    balanceAfter DECIMAL(18,2) NOT NULL,
    rateApplied DECIMAL(5,4) NOT NULL,
    periodMonth NVARCHAR(7) NOT NULL,
    creditedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_InterestLog_SavingsAccount FOREIGN KEY (accountId) REFERENCES SavingsAccount(id),
    CONSTRAINT UQ_InterestLog_AccountPeriod UNIQUE (accountId, periodMonth)
);
GO

CREATE TABLE AutoDeposit (
    id INT PRIMARY KEY IDENTITY(1,1),
    savingsAccountId INT NOT NULL,
    frequency NVARCHAR(50),
    amount DECIMAL(18,2),
    isActive BIT,
    nextRunDate DATE,
    sourceAccountId INT,
    dayOfMonth INT,
    dayOfWeek INT,
    updatedAt DATETIME2,
    CONSTRAINT FK_AutoDeposit_SavingsAccount FOREIGN KEY (savingsAccountId) REFERENCES SavingsAccount(id),
    CONSTRAINT FK_AutoDeposit_SourceAccount FOREIGN KEY (sourceAccountId) REFERENCES Account(Id),
    CONSTRAINT CK_AutoDeposit_DayOfMonth CHECK (dayOfMonth IS NULL OR (dayOfMonth >= 1 AND dayOfMonth <= 28))
);
GO

CREATE TABLE Portfolio (
    id INT PRIMARY KEY IDENTITY(1,1),
    userId INT,
    totalValue DECIMAL(18,2),
    totalGainLoss DECIMAL(18,2),
    gainLossPercent DECIMAL(18,2),
    CONSTRAINT FK_Portfolio_User FOREIGN KEY (userId) REFERENCES [User](Id)
);
GO

CREATE TABLE InvestmentHolding (
    id INT PRIMARY KEY IDENTITY(1,1),
    portfolioId INT NOT NULL,
    ticker NVARCHAR(50),
    assetType NVARCHAR(50),
    quantity DECIMAL(18,2),
    avgPurchasePrice DECIMAL(18,2),
    currentPrice DECIMAL(18,2),
    unrealizedGainLoss DECIMAL(18,2),
    CONSTRAINT FK_InvestmentHolding_Portfolio FOREIGN KEY (portfolioId) REFERENCES Portfolio(id)
);
GO

CREATE TABLE InvestmentTransaction (
    id INT PRIMARY KEY IDENTITY(1,1),
    holdingId INT NOT NULL,
    ticker NVARCHAR(50),
    actionType NVARCHAR(20),
    quantity DECIMAL(18,2),
    pricePerUnit DECIMAL(18,2),
    fees DECIMAL(18,2),
    orderType NVARCHAR(20),
    executedAt DATETIME2,
    CONSTRAINT FK_InvestmentTransaction_Holding FOREIGN KEY (holdingId) REFERENCES InvestmentHolding(id)
);
GO

CREATE TABLE ChatSession (
    id INT PRIMARY KEY IDENTITY(1,1),
    userId INT,
    issueCategory NVARCHAR(50),
    sessionStatus NVARCHAR(30),
    rating INT,
    startedAt DATETIME2,
    endedAt DATETIME2,
    feedback NVARCHAR(255),
    CONSTRAINT FK_ChatSession_User FOREIGN KEY (userId) REFERENCES [User](Id)
);
GO

CREATE TABLE ChatMessage (
    id INT PRIMARY KEY IDENTITY(1,1),
    sessionId INT NOT NULL,
    senderType NVARCHAR(20),
    content NVARCHAR(MAX),
    sentAt DATETIME2,
    CONSTRAINT FK_ChatMessage_Session FOREIGN KEY (sessionId) REFERENCES ChatSession(id)
);
GO

CREATE TABLE ChatAttachment (
    id INT PRIMARY KEY IDENTITY(1,1),
    messageId INT NOT NULL,
    attachmentName NVARCHAR(255),
    fileType NVARCHAR(50),
    fileSizeBytes INT,
    storageUrl NVARCHAR(255),
    CONSTRAINT FK_ChatAttachment_Message FOREIGN KEY (messageId) REFERENCES ChatMessage(id)
);
GO

CREATE TABLE UserCardPreference
(
    UserId INT NOT NULL PRIMARY KEY,
    SortOption VARCHAR(50) NOT NULL CONSTRAINT DF_UserCardPreference_SortOption DEFAULT 'custom',
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_UserCardPreference_UpdatedAt DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserCardPreference_User FOREIGN KEY (UserId) REFERENCES [User](Id)
);

