using BankApp.Client.Master;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.State;
using BankApp.Client.Utilities;
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    public partial class App : Application
    {
        public static Window? MainAppWindow { get; private set; }
        public static ApiService ApiService { get; private set; } = new ApiService();
        public static NavigationService NavigationService { get; private set; } = new NavigationService();
        public static IDashboardService DashboardService { get; private set; } = new DashboardService(ApiService);
        public static IAuthService AuthService { get; private set; } = new AuthService(ApiService);
        public static IProfileService ProfileService { get; private set; } = new ProfileService(ApiService);
        public static IInvestmentsService InvestmentsService { get; private set; } = new InvestmentsService(ApiService);
        public static ILoansApiService LoansRepoProxy { get; private set; } = new LoansApiService(ApiService);
        public static ILoansService LoansService { get; private set; } = new LoansService(LoansRepoProxy);
        public static ISavingsApiService SavingsRepoProxy { get; private set; } = new SavingsApiService(ApiService);
        public static ISavingsService SavingsService { get; private set; } = new SavingsService(SavingsRepoProxy);
        public static ICardApiService CardRepoProxy { get; private set; } = new CardApiService(ApiService);
        public static ICardService CardService { get; private set; } = new CardService(CardRepoProxy);
        public static ITransactionApiService TransactionRepoProxy { get; private set; } = new TransactionApiService(ApiService);
        public static ITransactionHistoryService TransactionHistoryService { get; private set; } = new TransactionHistoryService(TransactionRepoProxy);
        public static IStatisticsApiService StatisticsRepoProxy { get; private set; } = new StatisticsApiService(ApiService);
        public static IStatisticsService StatisticsService { get; private set; } = new StatisticsService(StatisticsRepoProxy);
        public static IChatApiService ChatRepoProxy { get; private set; } = new ChatApiService(ApiService);
        public static IChatService ChatService { get; private set; } = new ChatService(ChatRepoProxy);
        public static ITransactionHistorySessionState TransactionHistorySessionState { get; private set; } = new TransactionHistorySessionState();

        private Window? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            MainAppWindow = _window;
            _window.Activate();
        }
    }
}
