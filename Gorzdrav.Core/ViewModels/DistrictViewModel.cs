using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class DistrictViewModel : BaseViewModel
    {
        [Reactive]
        public int Id { get; set; }

        [Reactive]
        public string Name { get; set; }
    }
}