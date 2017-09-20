using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class ClinicViewModel : BaseViewModel
    {
        [Reactive]
        public int Id { get; set; }

        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public DistrictViewModel District { get; set; }
    }
}