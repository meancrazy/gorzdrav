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
        #region Fields
        
        private readonly IHubService _service;
        private IDbService _dbService;

        #endregion

        #region Properties

        [Reactive]
        public PatientViewModel Patient { get; set; }

        public ReactiveList<HistoryVisit> HistoryVisits { get; } = new ReactiveList<HistoryVisit>();

        [Reactive]
        public HistoryVisit HistoryVisit { get; set; }

        #endregion

        #region Commands
        
        public ReactiveCommand<Unit, List<HistoryVisit>> GetAppointments { get; }

        public ReactiveCommand<PatientViewModel, AppointmentViewModel> AddAppointment { get; }

        public ReactiveCommand<HistoryVisit, HistoryVisit> DeleteAppointment { get; }

        public ReactiveCommand<Unit, PatientViewModel> AddPatient { get; }

        #endregion

        public MainViewModel(IHubService service = null, IDbService dbService = null)
        {
            _service = service ?? Locator.CurrentMutable.GetService<IHubService>();

            _dbService = dbService ?? Locator.CurrentMutable.GetService<IDbService>();

            Patient = _dbService.SelectedPatient;

            var hasPatient = this.WhenAnyValue(x => x.Patient).Select(x => x != null);

            GetAppointments = ReactiveCommand.CreateFromTask(GetAppointmentsImpl, hasPatient);

            var d1 = GetAppointments.Subscribe(list =>
            {
                using (HistoryVisits.SuppressChangeNotifications())
                {
                    HistoryVisits.Clear();
                    HistoryVisits.AddRange(list);
                }
            });

            var d2 = this.WhenAnyValue(x => x.Patient).Where(x => x != null).Select(x => Unit.Default).InvokeCommand(this, x => x.GetAppointments);
            
            var d3 = HistoryVisits.ShouldReset.Subscribe();

            var d4 = this.WhenAnyValue(x => x.Patient)
                         .Skip(1)
                         .DistinctUntilChanged()
                         .BindTo(_dbService, x => x.SelectedPatient);

            var canInitialize = this.WhenAnyValue(x => x.Patient).Select(x => x == null).DistinctUntilChanged();
            
            AddPatient = Interactions.AddPatient.ToReactiveCommand(canInitialize);
            var d5 = AddPatient.BindTo(this, x => x.Patient);

            var canDeleteAppointment = this.WhenAnyValue(x => x.HistoryVisit).Select(x => x != null);
            DeleteAppointment = ReactiveCommand.CreateFromTask<HistoryVisit, HistoryVisit>(DeleteAppointmentImpl, canDeleteAppointment);
            var d6 = DeleteAppointment.Where(x => x != null).Subscribe(x => HistoryVisits.Remove(HistoryVisit));
            
            AddAppointment = Interactions.AddAppointment.ToReactiveCommand(hasPatient);

            InitCleanup(d1, d2, d3, d4, d5, d6);
        }

        private async Task<HistoryVisit> DeleteAppointmentImpl(HistoryVisit historyVisit)
        {
            if (historyVisit == null)
                return null;

            var result = await _service.CreateClaimForRefusalAsync(Patient.Clinic.Id, Patient.Id, historyVisit.IdAppointment, Consts.Id, null);
            return result.Success ? historyVisit : null;
        }

        private async Task<List<HistoryVisit>> GetAppointmentsImpl()
        {
            this.Log().Info($"GetAppointments({Patient.FullName}, {Patient.Clinic.Name})");
            var history = await _service.GetPatientHistoryAsync(Patient.Clinic.Id, Patient.Id, Consts.Id, null);

            return history.ListHistoryVisit;
        }
    }
}
