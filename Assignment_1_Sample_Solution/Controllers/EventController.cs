using Microsoft.AspNetCore.Mvc;
using Assignment_1_Sample_Solution.Models;

namespace Assignment_1_Sample_Solution.Controllers
{
    public class EventController : Controller
    {
        private static List<Event> events = new()
        {
            new Event
            {
                Id = 1,
                Title = "Career Fair",
                Date = new DateTime(2026, 2, 1),
                Location = "Gym"
            },

            new Event
            {
                Id = 2,
                Title = "Tech Talk",
                Date = new DateTime(2026, 2, 8),
                Location = "Auditorium"
            },

            new Event
            {
                Id = 3,
                Title = "Hack Night",
                Date = new DateTime(2026, 2, 15),
                Location = "Library"
            }
        };

        public IActionResult Index()
        {
            return View(events);
        }

        public IActionResult ManageAttendees(int id)
        {
            Event selectedEvent =
                events.FirstOrDefault(e => e.Id == id);

            return View(selectedEvent);
        }

        [HttpPost]
        public IActionResult SignUp(
            int eventId,
            string name,
            string email)
        {
            Event selectedEvent =
                events.FirstOrDefault(e => e.Id == eventId);

            if (selectedEvent != null)
            {
                selectedEvent.Attendees.Add(
                    new Attendee
                    {
                        Name = name,
                        Email = email
                    });
            }

            TempData["Message"] = "Attendee registered!";

            return RedirectToAction(
                "ManageAttendees",
                new { id = eventId });
        }
    }
}