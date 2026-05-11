namespace BankApp.Client.Utilities
{
    public interface IAppObserver<T>
    {
        void Update(T value);
    }
}
