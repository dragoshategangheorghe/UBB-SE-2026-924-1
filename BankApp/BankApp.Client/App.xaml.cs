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
        public static ApiService ApiService { get; private set; } = new ApiService();
        public static NavigationService NavigationService { get; private set; } = new NavigationService();
        public static ICardApiService CardApiService { get; private set; } = new CardApiService(ApiService);
        public static ITransactionApiService TransactionApiService { get; private set; } = new TransactionApiService(ApiService);
        public static IStatisticsApiService StatisticsApiService { get; private set; } = new StatisticsApiService(ApiService);
        public static ITransactionHistorySessionState TransactionHistorySessionState { get; private set; } = new TransactionHistorySessionState();

        private Window? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
