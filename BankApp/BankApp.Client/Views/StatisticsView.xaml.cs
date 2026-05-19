using System;
using BankApp.Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    public sealed partial class StatisticsView : Page
    {
        public StatisticsView()
        {
            InitializeComponent();
            ViewModel = new StatisticsViewModel(App.StatisticsService, App.AuthService);
            DataContext = ViewModel;
            Loaded += StatisticsView_Loaded;
            Unloaded += StatisticsView_Unloaded;
        }

        public StatisticsViewModel ViewModel { get; }

        private async void StatisticsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.IsLoading && !ViewModel.HasData)
            {
                await ViewModel.LoadAsync();
            }
        }

        private void StatisticsView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
