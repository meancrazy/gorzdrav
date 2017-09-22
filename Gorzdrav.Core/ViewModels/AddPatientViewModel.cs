using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Gorzdrav.Core.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class AddPatientViewModel : BaseViewModel
    {

        public ReactiveList<DistrictViewModel> Districts { get; } = new ReactiveList<DistrictViewModel>();

        [Reactive]
        public DistrictViewModel District { get; set; }

        public ReactiveList<ClinicViewModel> Clinics { get; } = new ReactiveList<ClinicViewModel>();

        [Reactive]
        public ClinicViewModel Clinic { get; set; }

        [Reactive]
        public string Name { get; set; }

        public ReactiveList<PatientViewModel> Patients { get; } = new ReactiveList<PatientViewModel>();

        [Reactive]
        public PatientViewModel Patient { get; set; }

        public extern PatientViewModel SelectedPatient { [ObservableAsProperty]get; }

        #region Commands

        public ReactiveCommand<Unit, IEnumerable<DistrictViewModel>> GetDistricts { get; }

        public ReactiveCommand<DistrictViewModel, IEnumerable<ClinicViewModel>> GetClinics { get; }

        public ReactiveCommand<Unit, IEnumerable<PatientViewModel>> SearchPatients { get; }

        public ReactiveCommand<Unit, PatientViewModel> Select { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }

        #endregion

        public AddPatientViewModel()
        {
            GetDistricts = ReactiveCommand.CreateFromTask(GetDistricsImpl);
            var d0 = GetDistricts.Subscribe(districts =>
            {
                using (Districts.SuppressChangeNotifications())
                {
                    Districts.Clear();
                    Districts.AddRange(districts);
                }
            });

            GetClinics = ReactiveCommand.CreateFromTask<DistrictViewModel, IEnumerable<ClinicViewModel>>(GetClinicsImpl);
            var d1 = GetClinics.Subscribe(clinics =>
            {
                using (Clinics.SuppressChangeNotifications())
                {
                    Clinics.Clear();
                    Clinics.AddRange(clinics);
                }
            });

            var d2 = this.WhenAnyValue(x => x.District).Where(x => x != null).InvokeCommand(this, x => x.GetClinics);

            SearchPatients = ReactiveCommand.CreateFromTask(SearchPatientsImpl);
            var d3 = SearchPatients.Subscribe(patients =>
            {
                using (Patients.SuppressChangeNotifications())
                {
                    Patients.Clear();
                    Patients.AddRange(patients);
                }
            });

            var d4 = this.WhenAnyValue(x => x.Name, x => x.Clinic, (name, clinic) => new { name, clinic } )
                         .Where(x => x.clinic != null && x.name?.Length > 10)
                         .Throttle(TimeSpan.FromMilliseconds(500))
                         .Select(_ => Unit.Default)
                         .InvokeCommand(this, x => x.SearchPatients);

            var canSelect = this.WhenAnyValue(x => x.Patient).Select(x => x != null);
            Select = ReactiveCommand.Create<Unit, PatientViewModel>(_ => Patient, canSelect);
            var d5 = Select.ToPropertyEx(this, x => x.SelectedPatient);

            Cancel = ReactiveCommandEx.CreateEmpty();
            
            var d6 = Districts.ShouldReset.Merge(Clinics.ShouldReset).Merge(Patients.ShouldReset).Subscribe();

            var d7 = GetDistricts.ThrownExceptions
                                 .Merge(GetClinics.ThrownExceptions)
                                 .Merge(SearchPatients.ThrownExceptions)
                                 .SelectMany(x => Interactions.Exceptions.Handle(x))
                                 .Subscribe();

            InitCleanup(d0, d1, d2, d3, d4, d5, d6, d7);
        }

        private async Task<IEnumerable<PatientViewModel>> SearchPatientsImpl()
        {
            var names = Name.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var patient = new Patient();
            if (names.Length > 0)
                patient.Surname = $"{names[0]}%";

            if (names.Length > 1)
                patient.Name = $"{names[1]}%";

            if (names.Length > 2)
                patient.SecondName = $"{names[2]}%";

            var result = await Service.SearchTop10PatientAsync(patient, Clinic.Id, Consts.Id, null);

            result.Check();
            
            return result.ListPatient.Select(x => new PatientViewModel{Id = x.IdPat, Clinic = Clinic, Name = x.Name, Surname = x.Surname, SecondName = x.SecondName, Birthday = x.Birthday});
        }

        private async Task<IEnumerable<ClinicViewModel>> GetClinicsImpl(DistrictViewModel district)
        {
            var result = await Service.GetLPUListAsync(district.Id, Consts.Id, null);
            
            result.Check();

            return result.ListLPU.Select(x => new ClinicViewModel{ Id = x.IdLPU, Name = x.LPUFullName, District = district});
        }

        private async Task<IEnumerable<DistrictViewModel>> GetDistricsImpl()
        {
            var result = await Service.GetDistrictListAsync(Consts.Id, null);
            
            return result.Select(x => new DistrictViewModel { Id = x.IdDistrict, Name = x.DistrictName });
        }
    }
}
