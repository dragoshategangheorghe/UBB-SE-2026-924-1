namespace BankApp.Client.Utilities
{
    public interface Observer<T>
    {
        void Update(T value);
    }
}
