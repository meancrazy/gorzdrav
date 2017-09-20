using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Gorzdrav.Core.Api;
using Gorzdrav.Core.ViewModels;
using ReactiveUI;
using Splat;

namespace Gorzdrav.Core
{
    public abstract class Bootstrapper : IEnableLogger
    {
        public void Initialize()
        {
            var dependencyResolver = Locator.CurrentMutable;

            Splat.NLog.Helpers.UseNLog();
            dependencyResolver.InitializeReactiveUI();

            this.Log().Info("Starting");

            InitializeCore(dependencyResolver);
            InitializeViews(dependencyResolver);

            Exit = ReactiveCommand.Create<Unit, Unit>(_ =>
            {
                this.Log().Info("Shutdown");

                dependencyResolver.GetService<IDbService>().Dispose();
                
                return Unit.Default;
            });
        }

        private void InitializeCore(IMutableDependencyResolver dependencyResolver)
        {
            dependencyResolver.RegisterLazySingleton(() => new DbService(), typeof(IDbService));
            
            dependencyResolver.RegisterLazySingleton(() => new HubServiceClient("BasicHttpBinding_IHubService", Consts.Url), typeof(IHubService));
        }

        protected abstract void InitializeViews(IMutableDependencyResolver dependencyResolver);

        public ReactiveCommand<Unit, Unit> Exit { get; private set; }
    }

    public class Repository : IDisposable
    {
        private readonly IDisposable _cleanup;

        public Repository(IHubService service)
        {
           service = service ?? Locator.CurrentMutable.GetService<IHubService>();

            var d0 = FillDistricts(service);
            

            _cleanup = new CompositeDisposable(d0);
        }

        private IDisposable FillDistricts(IHubService service)
        {
            return Observable.FromAsync(async () =>
                {
                    var result = await service.GetDistrictListAsync(Consts.Id, null);
                    return result.Select(x => new DistrictViewModel { Id = x.IdDistrict, Name = x.DistrictName });
                })
                .Subscribe(list =>
                {
                    Districts.AddRange(list);

                });
        }

        public ReactiveList<DistrictViewModel> Districts { get; } = new ReactiveList<DistrictViewModel>();

        public void Dispose()
        {
            _cleanup?.Dispose();
        }
    }
}
