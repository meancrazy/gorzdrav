using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Gorzdrav.Core.ViewModels;
using Gorzdrav.Views;
using ReactiveUI;

namespace Gorzdrav
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Initialize(object sender, StartupEventArgs args)
        {
            var bootstrapper = new AppBootstrapper();
            bootstrapper.Initialize();

            Observable.FromEventPattern<ExitEventHandler, ExitEventArgs>(e => Exit += e, e => Exit -= e)
                      .Select(_ => Unit.Default)
                      .InvokeCommand(bootstrapper.Exit);

            var viewModel = new MainViewModel();

            var mainView = new MainView(viewModel);

            mainView.Show();
        }
    }
}
