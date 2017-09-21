﻿using System;
using System.Collections.Generic;
using System.Windows;
using Gorzdrav.Core.ViewModels;
using ReactiveUI;

namespace Gorzdrav.Views
{
    public partial class SettingsView : IViewFor<SettingsViewModel>
    {
        public SettingsView()
        {
            InitializeComponent();
            
            this.WhenActivated(Initialize);
        }

        private IEnumerable<IDisposable> Initialize()
        {
            yield return this.WhenAnyValue(x => x.DataContext).BindTo(this, x => x.ViewModel);
        }

        #region IViewFor

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SettingsViewModel)value;
        }

        public SettingsViewModel ViewModel
        {
            get => (SettingsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(SettingsViewModel), typeof(SettingsView));

        #endregion
    }
}
