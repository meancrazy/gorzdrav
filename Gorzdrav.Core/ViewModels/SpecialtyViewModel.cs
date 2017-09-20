using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class SpecialtyViewModel
    {
        public SpecialtyViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        
        public string Name { get; }
    }
}