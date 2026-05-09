namespace BankApp.Client.ViewModels
{
    /// <summary>
    /// Compatibility shim for XAML type discovery.
    /// </summary>
    public class Observable<T> : BankApp.Client.Utilities.Observable<T>
    {
        public Observable(T value)
            : base(value)
        {
        }
    }
}

