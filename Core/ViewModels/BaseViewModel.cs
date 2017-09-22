using System;
using System.Reactive.Disposables;
using Gorzdrav.Core.Api;
using ReactiveUI;
using Splat;

namespace Gorzdrav.Core.ViewModels
{
    public abstract class BaseViewModel : ReactiveObject, IDisposable
    {
        private CompositeDisposable _cleanup;
        protected readonly IHubService Service;

        protected BaseViewModel()
        {
            Service =  Locator.CurrentMutable.GetService<IHubService>();
        }

        protected void InitCleanup(params IDisposable[] args)
        {
            _cleanup = new CompositeDisposable(args);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
         
        ~BaseViewModel()
        { 
            Dispose(false);
        }
        
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            
            if (_cleanup == null)
                return;

            _cleanup.Dispose();
            _cleanup = null;
        }
    }
}