using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BankApp.Client.Utilities
{
    public abstract partial class BaseViewModel : ObservableObject, IDisposable
    {
        private readonly SynchronizationContext? _synchronizationContext;

        protected BaseViewModel()
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        public new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);
        }

        protected void SetState<T>(Observable<T> observable, T value)
        {
            observable.SetValue(value);
        }

        protected void RunOnUiThread(Action action)
        {
            if (_synchronizationContext == null)
            {
                action();
                return;
            }

            _synchronizationContext.Post(_ => action(), null);
        }

        public abstract void Dispose();
    }
}