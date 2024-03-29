﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gorzdrav.Core;
using Gorzdrav.Core.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;

namespace Gorzdrav.UI.Views
{
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
            yield return Interactions.Exceptions.RegisterHandler(ShowException);
            yield return Interactions.AddPatient.RegisterHandler(ShowAddPatientView);
            yield return Interactions.AddAppointment.RegisterHandler(ShowAddAppointmentView);
            yield return Interactions.ShowSettings.RegisterHandler(ShowSettings);

            yield return Observable.Return(Unit.Default).InvokeCommand(ViewModel, x => x.AddPatient);
        }

        private async Task ShowSettings(InteractionContext<SettingsViewModel, Unit> interaction)
        {
            var view = new SettingsView(this, interaction.Input);

            await this.ShowMetroDialogAsync(view);
            await view.WaitUntilUnloadedAsync();

            interaction.SetOutput(Unit.Default);
        }

        private async Task ShowException(InteractionContext<Exception, Unit> interaction)
        {
            await this.ShowMessageAsync("Ошибка", interaction.Input.Message);

            interaction.SetOutput(Unit.Default);
        }

        private async Task ShowAddAppointmentView(InteractionContext<PatientViewModel, Unit> interaction)
        {
            var viewModel = new AddAppointmentViewModel(interaction.Input);

            var view = new AddAppointmentView(this, viewModel);

            await this.ShowMetroDialogAsync(view);
            await view.WaitUntilUnloadedAsync();

            interaction.SetOutput(Unit.Default);
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
