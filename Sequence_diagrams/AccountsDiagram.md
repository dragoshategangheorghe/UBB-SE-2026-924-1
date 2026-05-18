sequenceDiagram
    autonumber
    actor U as User (Browser)
    participant M as Auth Middleware
    participant C as AccountsController
    participant S as AccountService
    participant DB as Database (Repository)
    participant L as ILogger
    
    U->>M: GET /Accounts/Index
    
    rect rgb(230, 240, 255)
        Note over M,C: Security Phase [Authorize]
        M->>M: Validate Auth Cookie
        M-->>C: Access Granted (Valid User ID)
    end
    
    rect rgb(240, 255, 240)
        Note over C,DB: Business Logic Phase
        C->>L: LogInfo("User accessed Accounts page")
        C->>S: GetAccountDashboardAsync(userId)
        
        S->>DB: FetchAccountsByUserId(userId)
        DB-->>S: Return List<AccountEntity>
        
        S->>DB: FetchRecentTransactions(userId)
        DB-->>S: Return List<TransactionEntity>
        
        Note over S: Calculate Total Balances<br/>Apply Business Rules<br/>Map Entities -> ViewModel
        
        S-->>C: Return AccountDashboardViewModel
    end
    
    rect rgb(255, 240, 240)
        Note over C,U: Presentation Phase
        alt Success (Data Found)
            C->>C: Render View("Index", viewModel)
            C-->>U: Return HTML (200 OK)
        else Error (e.g., DB Connection Failed)
            C->>L: LogError(Exception)
            C-->>U: Return View("Error") / 500 Internal Error
        end
    end
