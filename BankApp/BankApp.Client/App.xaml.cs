using System;
using System.Globalization;
using BankApp.Client.Master;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.State;
using BankApp.Client.ViewModels; // Added for the new ViewModel
using BankApp.Models.Entities;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    public partial class App : Application
    {
        // --- Static Repositories (Composition Root) ---
        private static readonly ApiService HttpApi = new ApiService();

        private static readonly ILoansRepoProxy LoansHttpRepo = new LoansRepoProxy(HttpApi);
        private static readonly ILoanDialogStateRepoProxy LoanDialogHttp = new LoanDialogStateRepoProxy(HttpApi);
        private static readonly ILoanApplicationPresentationRepoProxy LoanApplicationPresentationHttp =
            new LoanApplicationPresentationRepoProxy(HttpApi);

        private static readonly ISavingsRepoProxy SavingsHttpRepo = new SavingsRepoProxy(HttpApi);
        private static readonly ISavingsUiRulesRepoProxy SavingsUiRulesHttp = new SavingsUiRulesRepoProxy(HttpApi);
        private static readonly ISavingsPresentationRepoProxy SavingsPresentationHttp = new SavingsPresentationRepoProxy(HttpApi);
        private static readonly ISavingsWorkflowRepoProxy SavingsWorkflowHttp = new SavingsWorkflowRepoProxy(HttpApi);

        private static readonly ICardRepoProxy CardHttpRepo = new CardRepoProxy(HttpApi);
        private static readonly ITransactionRepoProxy TransactionHttpRepo = new TransactionRepoProxy(HttpApi);
        private static readonly IStatisticsRepoProxy StatisticsHttpRepo = new StatisticsRepoProxy(HttpApi);
        private static readonly IChatRepoProxy ChatHttpRepo = new ChatRepoProxy(HttpApi);

        private static readonly IAuthRepoProxy AuthHttpRepo = new AuthRepoProxy(HttpApi);
        private static readonly IDashboardRepoProxy DashboardHttpRepo = new DashboardRepoProxy(HttpApi);
        private static readonly IProfileRepoProxy ProfileHttpRepo = new ProfileRepoProxy(HttpApi);
        private static readonly IInvestmentsRepoProxy InvestmentsHttpRepo = new InvestmentsRepoProxy(HttpApi);
        private static readonly IAccountRepoProxy AccountHttpRepo = new AccountRepoProxy(HttpApi);

        // --- Static Services (Accessible via App.ServiceName) ---
        public static Window? MainAppWindow { get; private set; }
        public static NavigationService NavigationService { get; private set; } = new NavigationService();

        public static IDashboardService DashboardService { get; private set; } = new DashboardService(DashboardHttpRepo);
        public static IAuthService AuthService { get; private set; } = new AuthService(AuthHttpRepo);
        public static IProfileService ProfileService { get; private set; } = new ProfileService(ProfileHttpRepo);
        public static INotificationClientService NotificationClientService { get; private set; } = new NotificationClientService();
        public static IInvestmentsService InvestmentsService { get; private set; } = new InvestmentsService(InvestmentsHttpRepo);
        public static IAccountService AccountService { get; private set; } = new AccountService(AccountHttpRepo);

        public static ILoansService LoansService { get; private set; } =
            new LoansService(LoansHttpRepo, LoanDialogHttp, LoanApplicationPresentationHttp);

        public static ISavingsService SavingsService { get; private set; } =
            new SavingsService(SavingsHttpRepo, SavingsUiRulesHttp, SavingsPresentationHttp, SavingsWorkflowHttp);

        public static ICardService CardService { get; private set; } = new CardService(CardHttpRepo);

        public static ITransactionHistoryService TransactionHistoryService { get; private set; } =
            new TransactionHistoryService(TransactionHttpRepo);

        public static IStatisticsService StatisticsService { get; private set; } = new StatisticsService(StatisticsHttpRepo);

        public static IChatService ChatService { get; private set; } = new ChatService(ChatHttpRepo);

        public static ITransactionHistorySessionState TransactionHistorySessionState { get; private set; } =
            new TransactionHistorySessionState();

        // --- Dependency Injection Provider ---
        // This fixes the CS1061 error in the CryptoTradingView.xaml.cs
        public IServiceProvider Services { get; }

        public App()
        {
            this.InitializeComponent();

            // 1. Setup Dependency Injection for modern ViewModels
            var serviceCollection = new ServiceCollection();

            // Register the Services so they can be injected into ViewModels
            serviceCollection.AddSingleton<IInvestmentsService>(InvestmentsService);
            serviceCollection.AddSingleton<IAccountService>(AccountService);

            // Register ViewModels
            serviceCollection.AddTransient<CryptoTradingViewModel>();
            serviceCollection.AddTransient<InvestmentsViewModel>();

            Services = serviceCollection.BuildServiceProvider();

            // 2. Setup Culture
            CultureInfo culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }
}