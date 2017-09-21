using System;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public static class Extensions
    {
        public static ReactiveCommand<TInput, TOutput> ToReactiveCommand<TInput, TOutput>(this Interaction<TInput, TOutput> interaction, IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.CreateFromObservable<TInput, TOutput>(interaction.Handle, canExecute);
        }
    }
}
