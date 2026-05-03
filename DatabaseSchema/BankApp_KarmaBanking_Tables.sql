USE BankAppDb;
GO

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
