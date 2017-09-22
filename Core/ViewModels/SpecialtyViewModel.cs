namespace Gorzdrav.Core.ViewModels
{
    public class SpecialtyViewModel
    {
        public SpecialtyViewModel(string id, string name, int tickets)
        {
            Id = id;
            Name = name;
            Tickets = tickets;
        }

        public string Id { get; }
        
        public string Name { get; }

        public int Tickets { get; }
    }
}