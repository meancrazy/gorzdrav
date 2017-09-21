namespace Gorzdrav.Core.ViewModels
{
    public class DoctorViewModel
    {
        public DoctorViewModel(string id, string name, SpecialtyViewModel specialty, int tickets)
        {
            Id = id;
            Name = name;
            Specialty = specialty;
            Tickets = tickets;
        }
        
        public string Id { get;  }
        
        public string Name { get;  }
        
        public SpecialtyViewModel Specialty { get;  }
        
        public int Tickets { get;  }
    }
}