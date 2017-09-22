using System;

namespace Gorzdrav.Core.ViewModels
{
    public class AppointmentViewModel : BaseViewModel
    {
        public AppointmentViewModel(string id, DateTime dateTime, DoctorViewModel doctor)
        {
            Id = id;
            DateTime = dateTime;
            Doctor = doctor;
        }

        public string Id { get; }

        public DateTime DateTime { get; }

        public DoctorViewModel Doctor { get; }
    }
}