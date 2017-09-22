using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class PatientViewModel : BaseViewModel
    {
        [Reactive]
        public string Id { get; set; }
        
        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public string SecondName { get; set; }

        [Reactive]
        public string Surname { get; set; }
        
        [Reactive]
        public DateTime? Birthday { get; set; }

        [Reactive]
        public ClinicViewModel Clinic { get; set; }

        public extern string FullName { [ObservableAsProperty]get; }

        public PatientViewModel()
        {
            var d0 = this.WhenAnyValue(x => x.Name, x => x.Surname, x => x.SecondName, (name, surname, secondName) => $"{surname} {name} {secondName}")
                         .ToPropertyEx(this, x => x.FullName);

            InitCleanup(d0);
        }
    }
}
