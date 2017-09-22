using System;
using System.Reactive;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public class ReactiveCommandEx : ReactiveCommand<Unit, Unit>
    {
        protected internal ReactiveCommandEx(Func<Unit, IObservable<Unit>> execute, IObservable<bool> canExecute, IScheduler outputScheduler) : base(execute, canExecute, outputScheduler)
        {
        }

        public static ReactiveCommand<Unit, Unit> CreateEmpty(IObservable<bool> canExecute = null)
        {
            return Create(() => { }, canExecute);
        }
    }
}