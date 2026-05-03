using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BankApp.Client.Utilities
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private readonly SynchronizationContext? _synchronizationContext;

        protected BaseViewModel()
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
