using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class DoctorViewModel
    {
        public DoctorViewModel(string id, string name, SpecialtyViewModel specialty, int availableAppointmentsCount)
        {
            Id = id;
            Name = name;
            Specialty = specialty;
            AvailableAppointmentsCount = availableAppointmentsCount;
        }
        
        public string Id { get;  }
        
        public string Name { get;  }
        
        public SpecialtyViewModel Specialty { get;  }
        
        public int AvailableAppointmentsCount { get;  }
    }
}