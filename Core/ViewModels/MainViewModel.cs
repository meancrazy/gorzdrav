using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Gorzdrav.Core.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties

        [Reactive]
        public SettingsViewModel Settings { get; set; }

        public ReactiveList<HistoryVisit> HistoryVisits { get; } = new ReactiveList<HistoryVisit>();

        [Reactive]
        public HistoryVisit HistoryVisit { get; set; }

        public extern bool Initialized { [ObservableAsProperty] get; }

        #endregion

        #region Commands
        
        public ReactiveCommand<Unit, List<HistoryVisit>> GetAppointments { get; }

        public ReactiveCommand<PatientViewModel, Unit> AddAppointment { get; }

        public ReactiveCommand<HistoryVisit, Unit> DeleteAppointment { get; }

        public ReactiveCommand<Unit, PatientViewModel> AddPatient { get; }

        public ReactiveCommand<SettingsViewModel, Unit> ShowSettings { get; }

        #endregion

        public MainViewModel()
        {
            Settings = new SettingsViewModel();

            var hasPatient = this.WhenAnyValue(x => x.Settings.SelectedPatient).Select(x => x != null);

            GetAppointments = ReactiveCommand.CreateFromTask(GetAppointmentsImpl, hasPatient);

            var d0 = GetAppointments.Subscribe(list =>
            {
                using (HistoryVisits.SuppressChangeNotifications())
                {
                    HistoryVisits.Clear();
                    HistoryVisits.AddRange(list);
                }
            });

            var d1 = this.WhenAnyValue(x => x.Settings.SelectedPatient).Where(x => x != null).Select(x => Unit.Default).InvokeCommand(this, x => x.GetAppointments);
            
            var d2 = HistoryVisits.ShouldReset.Subscribe();

            var canInitialize = this.WhenAnyValue(x => x.Settings.SelectedPatient).Select(x => x == null);

            var d3 = canInitialize.ToPropertyEx(this, x => x.Initialized);

            AddPatient = Interactions.AddPatient.ToReactiveCommand(canInitialize);
            var d4 = AddPatient.BindTo(this, x => x.Settings.SelectedPatient);

            var canDeleteAppointment = this.WhenAnyValue(x => x.HistoryVisit).Select(x => x != null);
            DeleteAppointment = ReactiveCommand.CreateFromTask<HistoryVisit, Unit>(DeleteAppointmentImpl, canDeleteAppointment);
            var d5 = DeleteAppointment.Subscribe(x => HistoryVisits.Remove(HistoryVisit));
            
            AddAppointment = Interactions.AddAppointment.ToReactiveCommand(hasPatient);

            var d6 = AddAppointment.InvokeCommand(GetAppointments);

            ShowSettings = Interactions.ShowSettings.ToReactiveCommand();

            var d7 = GetAppointments.ThrownExceptions
                                    .Merge(DeleteAppointment.ThrownExceptions)
                                    .Merge(AddAppointment.ThrownExceptions)
                                    .SelectMany(x => Interactions.Exceptions.Handle(x))
                                    .Subscribe();

            InitCleanup(d0, d1, d2, d3, d4, d5, d6, d7);
        }

        private async Task<Unit> DeleteAppointmentImpl(HistoryVisit historyVisit)
        {
            var result = await Service.CreateClaimForRefusalAsync(Settings.SelectedPatient.Clinic.Id, Settings.SelectedPatient.Id, historyVisit.IdAppointment, Consts.Id, null);

            result.Check();

            return Unit.Default;
        }

        private async Task<List<HistoryVisit>> GetAppointmentsImpl()
        {
            var result = await Service.GetPatientHistoryAsync(Settings.SelectedPatient.Clinic.Id, Settings.SelectedPatient.Id, Consts.Id, null);

            result.Check();

            return result.ListHistoryVisit;
        }
    }
}
