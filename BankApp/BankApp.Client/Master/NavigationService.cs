using BankApp.Client.Views;
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
            NavigateInternal(_frame, typeof(TPage));
        }

        public void NavigateToContent<TPage>()
        {
            NavigateInternal(_contentFrame, typeof(TPage));
        }

        public void GoBack()
        {
            if (CanGoBack())
            {
                _frame?.GoBack();
            }
        }

        public bool CanGoBack()
        {
            return _frame?.CanGoBack ?? false;
        }

        private void NavigateInternal(Frame? frame, System.Type pageType)
        {
            if (frame == null)
            {
                return;
            }

            bool isPublicPage = pageType == typeof(LoginView) ||
                                pageType == typeof(RegisterView) ||
                                pageType == typeof(ForgotPasswordView) ||
                                pageType == typeof(TwoFactorView);

            if (!isPublicPage && (App.AuthService == null || !App.AuthService.IsAuthenticated()))
            {
                frame.Navigate(typeof(LoginView));
                return;
            }

            frame.Navigate(pageType);
        }
    }
}