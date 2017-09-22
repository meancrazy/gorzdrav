using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        
        #endregion

        #region Commands

        public ReactiveCommand<Unit, IEnumerable<SpecialtyViewModel>> GetSpecialties { get; }

        public ReactiveCommand<SpecialtyViewModel, IEnumerable<DoctorViewModel>> GetDoctors { get; }
        
        public ReactiveCommand<DoctorViewModel, IEnumerable<AppointmentViewModel>> GetAppointments { get; }

        public ReactiveCommand<AppointmentViewModel, Unit> Select { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }


        #endregion

        public AddAppointmentViewModel(PatientViewModel patient)
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
            
            GetAppointments = ReactiveCommand.CreateFromTask<DoctorViewModel, IEnumerable<AppointmentViewModel>>(GetAppointmentsImpl);
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
                         .InvokeCommand(this, x => x.GetAppointments);

            var canSelect = this.WhenAnyValue(x => x.Appointment).Select(x => x != null);
            Select = ReactiveCommand.CreateFromTask<AppointmentViewModel, Unit>(SelectImpl, canSelect);
            
            Cancel = ReactiveCommandEx.CreateEmpty();

            var d7 = Specialties.ShouldReset.Merge(Doctors.ShouldReset).Subscribe();

            var d8 = GetSpecialties.ThrownExceptions
                                   .Merge(GetDoctors.ThrownExceptions)
                                   .Merge(GetAppointments.ThrownExceptions)
                                   .Merge(Select.ThrownExceptions)
                                   .SelectMany(x => Interactions.Exceptions.Handle(x))
                                   .Subscribe();

            InitCleanup(d0, d1, d2, d3, d5, d4, d6, d7, d8);
        }

        private async Task<Unit> SelectImpl(AppointmentViewModel appointment)
        {
            var result = await Service.SetAppointmentAsync(appointment.Id, _patient.Clinic.Id, _patient.Id, null, Consts.Id, null);

            result.Check();

            return Unit.Default;
        }

        private async Task<IEnumerable<AppointmentViewModel>> GetAppointmentsImpl(DoctorViewModel doctor)
        {
            // clear appointments if doctor is null
            if (doctor == null)
                return null;

            var result = await Service.GetAvaibleAppointmentsAsync(doctor.Id, _patient.Clinic.Id, _patient.Id, DateTime.Today, DateTime.Today.AddMonths(1).AddDays(-1), Consts.Id, null);

            result.Check();

            return result.ListAppointments.Select(x => new AppointmentViewModel(x.IdAppointment, x.VisitStart, Doctor));
        }

        private async Task<IEnumerable<DoctorViewModel>> GetDoctorsImpl(SpecialtyViewModel specialty)
        {
            if (specialty == null)
                return null;

            var result = await Service.GetDoctorListAsync(specialty.Id, _patient.Clinic.Id, _patient.Id, Consts.Id, null);

            result.Check();

            return result.Docs.Select(x => new DoctorViewModel(x.IdDoc, x.Name, Specialty, x.CountFreeParticipantIE));
        }

        private async Task<IEnumerable<SpecialtyViewModel>> GetSpecialtiesImpl()
        {
            var result = await Service.GetSpesialityListAsync(_patient.Clinic.Id, _patient.Id, Consts.Id, null);

            result.Check();

            return result.ListSpesiality.Select(x => new SpecialtyViewModel(x.IdSpesiality, x.NameSpesiality, x.CountFreeParticipantIE));
        }
    }
}