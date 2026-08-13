namespace Assignment_1_Sample_Solution.Models
{
    public class Attendee
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Identity user who registered for the event
        public string? UserId { get; set; }

        public int EventId { get; set; }

        public Event? Event { get; set; }
    }
}