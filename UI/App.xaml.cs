using System.Windows;
using Gorzdrav.Core;
using Gorzdrav.Core.Api;
using Gorzdrav.Core.ViewModels;
using Gorzdrav.UI.Views;
using ReactiveUI;
using Splat;

namespace Gorzdrav.UI
{
    public partial class App
    {
        private void Initialize(object sender, StartupEventArgs args)
        {
            Locator.CurrentMutable.InitializeReactiveUI();
            Locator.CurrentMutable.RegisterConstant(new HubServiceClient("BasicHttpBinding_IHubService", Consts.Url), typeof(IHubService));
            
            var viewModel = new MainViewModel();
            var mainView = new MainView(viewModel);

            mainView.Show();
        }
    }
}
