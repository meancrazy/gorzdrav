using System;
using System.Reactive;
using Gorzdrav.Core.ViewModels;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public static class Interactions
    {
        public static readonly Interaction<Exception, Unit> Exceptions = new Interaction<Exception, Unit>();

        public static readonly Interaction<Unit, PatientViewModel> AddPatient = new Interaction<Unit, PatientViewModel>();

        public static readonly Interaction<PatientViewModel, AppointmentViewModel> AddAppointment = new Interaction<PatientViewModel, AppointmentViewModel>();
    }
}