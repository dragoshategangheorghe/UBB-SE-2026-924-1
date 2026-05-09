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
