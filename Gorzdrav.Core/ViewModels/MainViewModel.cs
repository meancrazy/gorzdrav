using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Gorzdrav.Core.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

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

        #endregion

        #region Commands
        
        public ReactiveCommand<Unit, List<HistoryVisit>> GetAppointments { get; }

        public ReactiveCommand<PatientViewModel, AppointmentViewModel> AddAppointment { get; }

        public ReactiveCommand<Unit, Unit> DeleteAppointment { get; }

        public ReactiveCommand<Unit, PatientViewModel> AddPatient { get; }

        #endregion

        public MainViewModel(IHubService service = null) : base(service)
        {
            Settings = new SettingsViewModel();

            var hasPatient = this.WhenAnyValue(x => x.Settings.SelectedPatient).Select(x => x != null);

            GetAppointments = ReactiveCommand.CreateFromTask(GetAppointmentsImpl, hasPatient);

            var d1 = GetAppointments.Subscribe(list =>
            {
                using (HistoryVisits.SuppressChangeNotifications())
                {
                    HistoryVisits.Clear();
                    HistoryVisits.AddRange(list);
                }
            });

            var d2 = this.WhenAnyValue(x => x.Settings.SelectedPatient).Where(x => x != null).Select(x => Unit.Default).InvokeCommand(this, x => x.GetAppointments);
            
            var d3 = HistoryVisits.ShouldReset.Subscribe();

            var canInitialize = this.WhenAnyValue(x => x.Settings.SelectedPatient).Select(x => x == null).DistinctUntilChanged();
            
            AddPatient = Interactions.AddPatient.ToReactiveCommand(canInitialize);
            var d4 = AddPatient.BindTo(this, x => x.Settings.SelectedPatient);

            var canDeleteAppointment = this.WhenAnyValue(x => x.HistoryVisit).Select(x => x != null);
            DeleteAppointment = ReactiveCommand.CreateFromTask(DeleteAppointmentImpl, canDeleteAppointment);
            var d5 = DeleteAppointment.Subscribe(x => HistoryVisits.Remove(HistoryVisit));
            
            AddAppointment = Interactions.AddAppointment.ToReactiveCommand(hasPatient);

            var d6 = GetAppointments.ThrownExceptions
                                    .Merge(AddPatient.ThrownExceptions)
                                    .Merge(DeleteAppointment.ThrownExceptions)
                                    .Merge(AddAppointment.ThrownExceptions)
                                    .SelectMany(x => Interactions.Exceptions.Handle(x))
                                    .Subscribe();

            InitCleanup(d1, d2, d3, d4, d5, d6);
        }

        private async Task DeleteAppointmentImpl()
        {
            await Service.CreateClaimForRefusalAsync(Settings.SelectedPatient.Clinic.Id, Settings.SelectedPatient.Id, HistoryVisit.IdAppointment, Consts.Id, null);
        }

        private async Task<List<HistoryVisit>> GetAppointmentsImpl()
        {
            this.Log().Info($"GetAppointments({Settings.SelectedPatient.FullName}, {Settings.SelectedPatient.Clinic.Name})");
            var history = await Service.GetPatientHistoryAsync(Settings.SelectedPatient.Clinic.Id, Settings.SelectedPatient.Id, Consts.Id, null);

            return history.ListHistoryVisit;
        }
    }
}
