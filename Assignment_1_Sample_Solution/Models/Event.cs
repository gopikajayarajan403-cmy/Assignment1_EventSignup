namespace Assignment_1_Sample_Solution.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public string Location { get; set; }

        public List<Attendee> Attendees { get; set; } = new();
    }
}