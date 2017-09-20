using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Gorzdrav.Core.ViewModels;
using MahApps.Metro.Controls;
using ReactiveUI;

namespace Gorzdrav.Views
{
    public partial class AddAppointmentView : IViewFor<AddAppointmentViewModel>
    {
        public AddAppointmentView(MetroWindow owningWindow, AddAppointmentViewModel viewModel) : base(owningWindow, null)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;
            
            this.WhenActivated(Initialize);
        }

        private IEnumerable<IDisposable> Initialize()
        {
            yield return this.WhenAnyObservable(x => x.ViewModel.Select).Subscribe(_ => RequestCloseAsync());
            yield return this.WhenAnyObservable(x => x.ViewModel.Cancel).Subscribe(_ => RequestCloseAsync());

            yield return Observable.Return(Unit.Default).InvokeCommand(ViewModel.GetSpecialties);
        }

        #region IViewFor

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AddAppointmentViewModel)value;
        }

        public AddAppointmentViewModel ViewModel
        {
            get => (AddAppointmentViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(AddAppointmentViewModel), typeof(AddAppointmentView));

        #endregion
    }
}
