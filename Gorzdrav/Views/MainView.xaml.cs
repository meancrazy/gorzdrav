using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gorzdrav.Core;
using Gorzdrav.Core.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;

namespace Gorzdrav.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : IViewFor<MainViewModel>
    {
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;

            this.WhenActivated(Initialize);
        }

        private IEnumerable<IDisposable> Initialize()
        {
            yield return Interactions.AddPatient.RegisterHandler(ShowAddPatientView);
            yield return Interactions.AddAppointment.RegisterHandler(ShowAddAppointmentView);

            yield return Observable.Return(Unit.Default).InvokeCommand(ViewModel, x => x.AddPatient);
        }

        private async Task ShowAddAppointmentView(InteractionContext<PatientViewModel, AppointmentViewModel> interaction)
        {
            var viewModel = new AddAppointmentViewModel(interaction.Input);

            var view = new AddAppointmentView(this, viewModel);

            await this.ShowMetroDialogAsync(view);
            await view.WaitUntilUnloadedAsync();

            interaction.SetOutput(viewModel.SelectedAppointment);
        }

        private async Task ShowAddPatientView(InteractionContext<Unit, PatientViewModel> interaction)
        {
            var view = new AddPatientView(this);

            await this.ShowMetroDialogAsync(view);
            await view.WaitUntilUnloadedAsync();

            interaction.SetOutput(view.ViewModel.SelectedPatient);
        }

        #region IViewFor

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainView));

        #endregion
    }
}
