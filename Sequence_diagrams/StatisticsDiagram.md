sequenceDiagram
    autonumber
    actor U as User (Browser)
    participant M as Auth Middleware
    participant C as StatisticsController
    participant S as StatisticsService
    participant DB as Database (Repository)
    
    
    U->>M: GET /Statistics/Index
    M->>C: Validate Cookie & Route
    C-->>U: Return Base HTML View (Load Bootstrap & jQuery)
    
    rect rgb(255, 250, 230)
        Note over U,C: AJAX Phase (jQuery API Call)
        U->>M: GET /Statistics/GetChartData (AJAX)
        M->>C: Validate Cookie & Route
        
        C->>S: GetMonthlyStatisticsAsync(userId)
        
        S->>DB: FetchTransactionsByDateRange(userId, Last6Months)
        DB-->>S: Return List<TransactionEntity>
        
        Note over S: Group by Category<br/>Calculate Income vs. Expenses<br/>Format data for Charts
        
        S-->>C: Return ChartDataViewModel
        
        C-->>U: Return JSON Response (200 OK)
    end
    
    Note over U: jQuery parses the JSON<br/>and dynamically renders<br/>the Statistics Dashboard
