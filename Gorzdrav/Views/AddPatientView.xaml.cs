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
    public partial class AddPatientView : IViewFor<AddPatientViewModel>
    {
        public AddPatientView(MetroWindow owningWindow) : base(owningWindow, null)
        {
            InitializeComponent();

            DataContext = ViewModel = new AddPatientViewModel();

            this.WhenActivated(Initialize);
        }

        private IEnumerable<IDisposable> Initialize()
        {
            yield return Observable.Return(Unit.Default).InvokeCommand(ViewModel.GetDistricts);
            
            yield return this.WhenAnyObservable(x => x.ViewModel.Select).Subscribe(_ => RequestCloseAsync());
            yield return this.WhenAnyObservable(x => x.ViewModel.Cancel).Subscribe(_ => RequestCloseAsync());
        }

        #region IViewFor

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AddPatientViewModel)value;
        }

        public AddPatientViewModel ViewModel
        {
            get => (AddPatientViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(AddPatientViewModel), typeof(AddPatientView));

        #endregion
    }
}
