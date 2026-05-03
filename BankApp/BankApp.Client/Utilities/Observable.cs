using System.Collections.Generic;

namespace BankApp.Client.Utilities
{
    public class Observable<T>
    {
        public T Value { get; private set; }
        private List<Observer<T>> _observers;

        public Observable(T value)
        {
            _observers = new List<Observer<T>>();
            Value = value;
        }

        public void SetValue(T value)
        {
            Value = value;
            NotifyObservers();
        }

        public void AddObserver(Observer<T> observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(Observer<T> observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Update(Value);
            }
        }
    }
}
