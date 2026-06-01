namespace BankingApp.Desktop.Shared;

using System;
using System.Collections.Generic;
using System.Text;

public interface IAppObserver<T>
{
    public void Update(T value);
}

public class Observable<T>
{
    public T Value { get; private set; }
    private List<IAppObserver<T>> _observers;

    public Observable(T value)
    {
        _observers = new List<IAppObserver<T>>();
        Value = value;
    }

    public void SetValue(T value)
    {
        Value = value;
        NotifyObservers();
    }

    public void AddObserver(IAppObserver<T> observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(IAppObserver<T> observer)
    {
        _observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (IAppObserver<T> observer in _observers)
        {
            observer.Update(Value);
        }
    }
}
