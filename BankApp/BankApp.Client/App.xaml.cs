using BankApp.Client.Master;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Implementations;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.State;
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    public partial class App : Application
    {
        /// <summary>
        /// HTTP transport: composition root only. Views and view models use App.*Service.
        /// </summary>
        private static readonly ApiService HttpApi = new ApiService();

        private static readonly ILoansApiService LoansHttpRepo = new LoansApiService(HttpApi);
        private static readonly ILoanDialogStateApiService LoanDialogHttp = new LoanDialogStateApiService(HttpApi);
        private static readonly ILoanApplicationPresentationApiService LoanApplicationPresentationHttp =
            new LoanApplicationPresentationApiService(HttpApi);

        private static readonly ISavingsApiService SavingsHttpRepo = new SavingsApiService(HttpApi);
        private static readonly ISavingsUiRulesApiService SavingsUiRulesHttp = new SavingsUiRulesApiService(HttpApi);
        private static readonly ISavingsPresentationApiService SavingsPresentationHttp = new SavingsPresentationApiService(HttpApi);
        private static readonly ISavingsWorkflowApiService SavingsWorkflowHttp = new SavingsWorkflowApiService(HttpApi);

        private static readonly ICardApiService CardHttpRepo = new CardApiService(HttpApi);
        private static readonly ITransactionApiService TransactionHttpRepo = new TransactionApiService(HttpApi);
        private static readonly IStatisticsApiService StatisticsHttpRepo = new StatisticsApiService(HttpApi);
        private static readonly IChatApiService ChatHttpRepo = new ChatApiService(HttpApi);

        private static readonly IAuthApiService AuthHttpRepo = new AuthApiService(HttpApi);
        private static readonly IDashboardApiService DashboardHttpRepo = new DashboardApiService(HttpApi);
        private static readonly IProfileApiService ProfileHttpRepo = new ProfileApiService(HttpApi);
        private static readonly IInvestmentsApiService InvestmentsHttpRepo = new InvestmentsApiService(HttpApi);

        public static Window? MainAppWindow { get; private set; }
        public static NavigationService NavigationService { get; private set; } = new NavigationService();

        public static IDashboardService DashboardService { get; private set; } = new DashboardService(DashboardHttpRepo);
        public static IAuthService AuthService { get; private set; } = new AuthService(AuthHttpRepo);
        public static IProfileService ProfileService { get; private set; } = new ProfileService(ProfileHttpRepo);
        public static IInvestmentsService InvestmentsService { get; private set; } = new InvestmentsService(InvestmentsHttpRepo);

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
