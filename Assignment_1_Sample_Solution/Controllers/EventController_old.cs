using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment_1_Sample_Solution.Data;
using Assignment_1_Sample_Solution.Models;

namespace Assignment_1_Sample_Solution.Controllers
{
    public class EventController_old : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController_old(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all events
        public IActionResult Index()
        {
            var events = _context.Events
                .Include(e => e.Attendees)
                .ToList();

            return View(events);
        }

        public IActionResult Details(int id)
        {
            var selectedEvent = _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefault(e => e.Id == id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        // Display attendees for a specific event
        public IActionResult ManageAttendees(int id)
        {
            var selectedEvent = _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefault(e => e.Id == id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        // Register a new attendee
        [HttpPost]
        public IActionResult SignUp(int eventId, string name, string email)
        {
            var selectedEvent = _context.Events
                .FirstOrDefault(e => e.Id == eventId);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            var attendee = new Attendee
            {
                Name = name,
                Email = email,
                EventId = eventId
            };

            _context.Attendees.Add(attendee);
            _context.SaveChanges();

            TempData["Message"] = "Attendee registered!";

            return RedirectToAction(nameof(ManageAttendees), new { id = eventId });
        }
    }
}