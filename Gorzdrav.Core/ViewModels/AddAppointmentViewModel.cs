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
    public class AddAppointmentViewModel : BaseViewModel
    {
        #region Fields
        
        private readonly PatientViewModel _patient;

        #endregion

        #region Properties

        public ReactiveList<SpecialtyViewModel> Specialties { get; } = new ReactiveList<SpecialtyViewModel>();

        [Reactive]
        public SpecialtyViewModel Specialty { get; set; }

        public ReactiveList<DoctorViewModel> Doctors { get; } = new ReactiveList<DoctorViewModel>();

        [Reactive]
        public DoctorViewModel Doctor { get; set; }

        public ReactiveList<AppointmentViewModel> Appointments { get; } = new ReactiveList<AppointmentViewModel>();

        [Reactive]
        public AppointmentViewModel Appointment { get; set; }

        public extern AppointmentViewModel SelectedAppointment { [ObservableAsProperty]get; }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, IEnumerable<SpecialtyViewModel>> GetSpecialties { get; }

        public ReactiveCommand<SpecialtyViewModel, IEnumerable<DoctorViewModel>> GetDoctors { get; }
        
        public ReactiveCommand<Unit, IEnumerable<AppointmentViewModel>> GetAppointments { get; }

        public ReactiveCommand<Unit, AppointmentViewModel> Select { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }


        #endregion

        public AddAppointmentViewModel(PatientViewModel patient, IHubService service = null) : base(service)
        {
            _patient = patient;

            GetSpecialties = ReactiveCommand.CreateFromTask(GetSpecialtiesImpl);

            var d0 = Specialties.ShouldReset.Subscribe();
            var d1 = GetSpecialties.Subscribe(list =>
            {
                using (Specialties.SuppressChangeNotifications())
                {
                    Specialties.Clear();
                    Specialties.AddRange(list);
                }
            });

            GetDoctors = ReactiveCommand.CreateFromTask<SpecialtyViewModel, IEnumerable<DoctorViewModel>>(GetDoctorsImpl);

            var d2 = Doctors.ShouldReset.Subscribe();
            var d3 = GetDoctors.Subscribe(list =>
            {
                using (Doctors.SuppressChangeNotifications())
                {
                    Doctors.Clear();

                    if (list == null)
                    {
                        return;
                    }

                    Doctors.AddRange(list);

                    if (Doctors.Count(x => x.Tickets > 0) == 1)
                    {
                        Doctor = Doctors.First(x => x.Tickets > 0);
                    }
                }
            });

            var d4 = this.WhenAnyValue(x => x.Specialty).InvokeCommand(this, x => x.GetDoctors);

            GetAppointments = ReactiveCommand.CreateFromTask(GetAppointmentsImpl);
            var d5 = GetAppointments.Subscribe(list =>
            {
                using (Appointments.SuppressChangeNotifications())
                {
                    Appointments.Clear();

                    if (list == null)
                    {
                        return;
                    }

                    Appointments.AddRange(list);
                }
            });

            var d6 = this.WhenAnyValue(x => x.Doctor)
                         .DistinctUntilChanged()
                         .Throttle(TimeSpan.FromSeconds(0.1))
                         .Select(_ => Unit.Default)
                         .InvokeCommand(this, x => x.GetAppointments);

            var canSelect = this.WhenAnyValue(x => x.Appointment).Select(x => x != null);
            Select = ReactiveCommand.Create<Unit, AppointmentViewModel>(_ => Appointment, canSelect);
            var d7 = Select.ToPropertyEx(this, x => x.SelectedAppointment);
            
            Cancel = ReactiveCommandEx.CreateEmpty();

            var d8 = Specialties.ShouldReset.Merge(Doctors.ShouldReset).Subscribe();

            var d9 = GetSpecialties.ThrownExceptions
                                   .Merge(GetDoctors.ThrownExceptions)
                                   .Merge(GetAppointments.ThrownExceptions)
                                   .SelectMany(x => Interactions.Exceptions.Handle(x))
                                   .Subscribe();

            InitCleanup(d0, d1, d2, d3, d5, d4, d6, d7, d8, d9);
        }

        private async Task<IEnumerable<AppointmentViewModel>> GetAppointmentsImpl()
        {
            if (Doctor == null)
                return null;

            var result = await Service.GetAvaibleAppointmentsAsync(Doctor.Id, _patient.Clinic.Id, _patient.Id, DateTime.Today, DateTime.Today.AddMonths(1).AddDays(-1), Consts.Id, null);

            return result.ListAppointments.Select(x => new AppointmentViewModel(x.IdAppointment, x.VisitStart, Doctor));
        }

        private async Task<IEnumerable<DoctorViewModel>> GetDoctorsImpl(SpecialtyViewModel specialty)
        {
            if (specialty == null)
                return null;

            var result = await Service.GetDoctorListAsync(specialty.Id, _patient.Clinic.Id, _patient.Id, Consts.Id, null);
            return result.Docs.Select(x => new DoctorViewModel(x.IdDoc, x.Name, Specialty, x.CountFreeParticipantIE));
        }

        private async Task<IEnumerable<SpecialtyViewModel>> GetSpecialtiesImpl()
        {
            var result = await Service.GetSpesialityListAsync(_patient.Clinic.Id, _patient.Id, Consts.Id, null);

            return result.ListSpesiality.Select(x => new SpecialtyViewModel(x.IdSpesiality, x.NameSpesiality, x.CountFreeParticipantIE));
        }
    }
}