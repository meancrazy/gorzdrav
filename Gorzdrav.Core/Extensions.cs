using System;
using System.Reactive;
using System.Reactive.Concurrency;
using LiteDB;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public static class Extensions
    {
        public static EntityBuilder<T> IgnoreRx<T>(this EntityBuilder<T> builder) where T : ReactiveObject
        {
            return builder.Ignore(x => x.Changed)
                          .Ignore(x => x.Changing)
                          .Ignore(x => x.ThrownExceptions);
        }

        public static ReactiveCommand<TInput, TOutput> ToReactiveCommand<TInput, TOutput>(this Interaction<TInput, TOutput> interaction, IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.CreateFromObservable<TInput, TOutput>(interaction.Handle, canExecute);
        }

        public static DateTime GetMonth(this DateTime d)
        {
            return new DateTime(d.Year, d.Month, 1, 0, 0, 0);
        }
    }

    public class ReactiveCommandEx : ReactiveCommand<Unit, Unit>
    {
        protected internal ReactiveCommandEx(Func<Unit, IObservable<Unit>> execute, IObservable<bool> canExecute, IScheduler outputScheduler) : base(execute, canExecute, outputScheduler)
        {
        }

        public static ReactiveCommand<Unit, Unit> CreateEmpty(IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.Create(() => { }, canExecute);
        }
    }
}
