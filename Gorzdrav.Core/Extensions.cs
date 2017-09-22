using System;
using System.Linq;
using Gorzdrav.Core.Api;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public static class Extensions
    {
        public static ReactiveCommand<TInput, TOutput> ToReactiveCommand<TInput, TOutput>(this Interaction<TInput, TOutput> interaction, IObservable<bool> canExecute = null)
        {
            return ReactiveCommand.CreateFromObservable<TInput, TOutput>(interaction.Handle, canExecute);
        }

        public static void Check(this MethodResult result)
        {
            if (result.Success) return;

            var message = result.ErrorList.Count > 0 ? string.Join("\r\n", result.ErrorList.Select(x => x.ErrorDescription)) : "Неизвестная ошибка";

            throw new Exception(message);
        }
    }
}
