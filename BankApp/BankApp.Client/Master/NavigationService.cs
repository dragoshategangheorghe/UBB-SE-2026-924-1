using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Master
{
    public class NavigationService : INavigationService
    {
        private Frame? _frame;
        private Frame? _contentFrame;

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void SetContentFrame(Frame frame)
        {
            _contentFrame = frame;
        }

        public void NavigateTo<TPage>()
        {
            _frame?.Navigate(typeof(TPage));
        }

        public void NavigateToContent<TPage>()
        {
            _contentFrame?.Navigate(typeof(TPage));
        }

        public void GoBack()
        {
            if (CanGoBack())
                _frame?.GoBack();
        }

        public bool CanGoBack()
        {
            return _frame?.CanGoBack ?? false;
        }
    }
}