using System;
using System.Reactive.Disposables;
using ReactiveUI;
using Splat;

namespace Gorzdrav.Core.ViewModels
{
    public abstract class BaseViewModel : ReactiveObject, IDisposable
    {
        private CompositeDisposable _cleanup;

        protected void InitCleanup(params IDisposable[] args)
        {
            _cleanup = new CompositeDisposable(args);
        }

        public void Dispose()
        {
            this.Log().Info($"Disposing {GetType().Name}");
            _cleanup?.Dispose();
        }
    }
}