using System;
using System.Globalization;
using System.Threading;
using BankApp.Client.Master;
using BankApp.Client.RepoProxies;
using BankApp.Client.State;
using BankApp.Client.ViewModels;
using BankApp.Models.Entities;
using global::BankApp.Client.RepoProxies.Implementations;
using global::BankApp.Client.RepoProxies.Interfaces;
using global::BankApp.Client.Services.Implementations;
using global::BankApp.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    public partial class App : Application
    {
        private static readonly ApiService HttpApi = new ApiService();

        // --- Repositories ---
        private static readonly ILoansRepoProxy LoansHttpRepo = new LoansRepoProxy(HttpApi);
        private static readonly ILoanDialogStateRepoProxy LoanDialogHttp = new LoanDialogStateRepoProxy(HttpApi);
        private static readonly ILoanApplicationPresentationRepoProxy LoanApplicationPresentationHttp = new LoanApplicationPresentationRepoProxy(HttpApi);
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

        // --- Static Services ---
        public static Window? MainAppWindow { get; private set; }
        public static NavigationService NavigationService { get; private set; } = new NavigationService();

        public static IDashboardService DashboardService { get; private set; } = new DashboardService(DashboardHttpRepo);
        public static IAuthService AuthService { get; private set; } = new AuthService(AuthHttpRepo);
        public static IProfileService ProfileService { get; private set; } = new ProfileService(ProfileHttpRepo);
        public static INotificationClientService NotificationClientService { get; private set; } = new NotificationClientService();

        // FIX: Pass AuthService into InvestmentsService constructor here
        public static IInvestmentsService InvestmentsService { get; private set; } = new InvestmentsService(InvestmentsHttpRepo, AuthService);

        public static IAccountService AccountService { get; private set; } = new AccountService(AccountHttpRepo);
        public static ILoansService LoansService { get; private set; } = new LoansService(LoansHttpRepo, LoanDialogHttp, LoanApplicationPresentationHttp);
        public static ISavingsService SavingsService { get; private set; } = new SavingsService(SavingsHttpRepo, SavingsUiRulesHttp, SavingsPresentationHttp, SavingsWorkflowHttp);
        public static ICardService CardService { get; private set; } = new CardService(CardHttpRepo);
        public static ITransactionHistoryService TransactionHistoryService { get; private set; } = new TransactionHistoryService(TransactionHttpRepo);
        public static IStatisticsService StatisticsService { get; private set; } = new StatisticsService(StatisticsHttpRepo);
        public static IChatService ChatService { get; private set; } = new ChatService(ChatHttpRepo);
        public static ITransactionHistorySessionState TransactionHistorySessionState { get; private set; } = new TransactionHistorySessionState();

        public IServiceProvider Services { get; }

        public App()
        {
            this.InitializeComponent();

            var serviceCollection = new ServiceCollection();

            // Register the static instances into DI
            serviceCollection.AddSingleton<IAuthService>(AuthService);
            serviceCollection.AddSingleton<IInvestmentsService>(InvestmentsService);
            serviceCollection.AddSingleton<IAccountService>(AccountService);

            // Register ViewModels
            serviceCollection.AddTransient<CryptoTradingViewModel>();
            serviceCollection.AddTransient<InvestmentsViewModel>();

            Services = serviceCollection.BuildServiceProvider();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }
}