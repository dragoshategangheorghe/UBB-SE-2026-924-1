using BankApp.Client.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client
{
    public sealed partial class MainWindow : Window
    {
        public Frame PublicRootFrame => RootFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            App.NavigationService.SetFrame(RootFrame);

            // Start on the login page
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}