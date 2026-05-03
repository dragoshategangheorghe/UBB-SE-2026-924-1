namespace BankApp.Client.Utilities
{
    public abstract class BaseView<T> : Observer<T>
    {
        public void Update(T value)
        {
            OnStateChanged(value);
        }

        public abstract void OnStateChanged(T state);
        public abstract void ShowError(string msg);
        public abstract void ShowLoading();
        public abstract void HideLoading();
    }
}
