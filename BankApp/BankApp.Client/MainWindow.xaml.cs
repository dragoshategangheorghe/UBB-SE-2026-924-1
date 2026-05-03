using Microsoft.UI.Xaml;
using BankApp.Client.Views;

namespace BankApp.Client
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            App.NavigationService.SetFrame(RootFrame);

            // Start on the login page
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}