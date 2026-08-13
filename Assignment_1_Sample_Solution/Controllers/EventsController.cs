using Assignment_1_Sample_Solution.Data;
using Assignment_1_Sample_Solution.Hubs;
using Assignment_1_Sample_Solution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment_1_Sample_Solution.Controllers
{
    [Route("events")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public EventsController(
    ApplicationDbContext context,
    IHubContext<EventHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ==========================================
        // EVENT LIST
        // Publicly accessible
        // ==========================================

        // GET: /events
        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Attendees)
                .ToListAsync();

            return View("Index", events);
        }

        // ==========================================
        // EVENT DETAILS
        // Publicly accessible
        // ==========================================

        // GET: /events/1/details
        [AllowAnonymous]
        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // ==========================================
        // CREATE EVENT
        // ORGANIZER ONLY
        // ==========================================

        // GET: /events/create
        [Authorize(Roles = "Organizer")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /events/create
        // POST: /events/create
        [Authorize(Roles = "Organizer")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Title,Description,Date,Location")] Event eventItem,
            IFormFile? bannerFile)
        {
            if (ModelState.IsValid)
            {
                // Get the logged-in Organizer's Identity user ID
                var organizerUserId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(organizerUserId))
                {
                    return Challenge();
                }

                // Store the Organizer who owns this event
                eventItem.OrganizerUserId = organizerUserId;

                // Save banner image locally for now.
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(bannerFile.FileName);

                    var filePath = Path.Combine(
                        uploadsFolder,
                        fileName);

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await bannerFile.CopyToAsync(stream);
                    }

                    eventItem.BannerUrl = "/uploads/" + fileName;
                }

                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(eventItem);
        }

        // ==========================================
        // EDIT EVENT
        // ORGANIZER ONLY
        // ==========================================

        // GET: /events/1/edit
        [Authorize(Roles = "Organizer")]
        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: /events/1/edit
        // POST: /events/1/edit
        [Authorize(Roles = "Organizer")]
        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? id,
            IFormFile? bannerFile)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the existing event from the database.
            // This preserves OrganizerUserId and the existing banner.
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            // Get the submitted form values.
            var title = Request.Form["Title"].ToString();
            var description = Request.Form["Description"].ToString();
            var dateString = Request.Form["Date"].ToString();
            var location = Request.Form["Location"].ToString();

            // Update the editable properties.
            existingEvent.Title = title;
            existingEvent.Description = description;
            existingEvent.Location = location;

            if (DateTime.TryParse(dateString, out var parsedDate))
            {
                existingEvent.Date = parsedDate;
            }

            // If a new banner was selected, save it.
            if (bannerFile != null && bannerFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(bannerFile.FileName);

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await bannerFile.CopyToAsync(stream);
                }

                existingEvent.BannerUrl =
                    "/uploads/" + fileName;
            }

            // IMPORTANT:
            // We intentionally do NOT change OrganizerUserId.
            // The existing Organizer remains the owner.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // DELETE EVENT
        // ORGANIZER ONLY
        // ==========================================

        // GET: /events/1/delete
        [Authorize(Roles = "Organizer")]
        [HttpGet("{id:int}/delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: /events/1/delete
        [Authorize(Roles = "Organizer")]
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VIEW ATTENDEES
        // AUTHENTICATED USERS
        // ==========================================

        // GET: /events/1/attendees
        [Authorize(Roles = "Organizer")]
        [HttpGet("{id:int}/attendees")]
        public async Task<IActionResult> ManageAttendees(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // ==========================================
        // ADD ATTENDEE
        // ORGANIZER ONLY
        // ==========================================

        // POST: /events/1/attendees/create
        [Authorize(Roles = "Organizer")]
        [HttpPost("{eventId:int}/attendees/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(
            int eventId,
            string name,
            string email)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
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
            await _context.SaveChangesAsync();

            TempData["Message"] = "Attendee registered!";

            return RedirectToAction(
                nameof(ManageAttendees),
                new { id = eventId });
        }

        // ==========================================
        // SELF-REGISTER FOR EVENT
        // ANY AUTHENTICATED USER
        // ==========================================

        // POST: /events/1/register
        // ==========================================
        // SELF-REGISTER FOR EVENT
        // ANY AUTHENTICATED USER
        // ==========================================

        // POST: /events/1/register
        [Authorize]
        [HttpPost("{eventId:int}/register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            // ------------------------------------------
            // 1. Find the event
            // ------------------------------------------

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            // ------------------------------------------
            // 2. Get the logged-in user's Identity ID
            // ------------------------------------------

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // ------------------------------------------
            // 3. Prevent duplicate registration
            // ------------------------------------------

            var alreadyRegistered = await _context.Attendees
                .AnyAsync(a =>
                    a.EventId == eventId &&
                    a.UserId == userId);

            if (alreadyRegistered)
            {
                TempData["Message"] =
                    "You are already registered for this event.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = eventId });
            }

            // ------------------------------------------
            // 4. Create the attendee
            // ------------------------------------------

            var attendee = new Attendee
            {
                UserId = userId,

                Name = User.Identity?.Name
                    ?? "Unknown User",

                Email = User.Identity?.Name
                    ?? string.Empty,

                EventId = eventId
            };

            _context.Attendees.Add(attendee);

            // ------------------------------------------
            // 5. Save registration to database
            // ------------------------------------------

            await _context.SaveChangesAsync();

            // ------------------------------------------
            // 6. Get updated attendee count
            // ------------------------------------------

            var attendeeCount = await _context.Attendees
                .CountAsync(a => a.EventId == eventId);

            // ------------------------------------------
            // 7. Broadcast to everyone viewing this event
            // ------------------------------------------

            await _hubContext.Clients
                .Group($"event-{eventId}")
                .SendAsync(
                    "AttendeeRegistered",
                    new
                    {
                        count = attendeeCount,
                        name = attendee.Name,
                        email = attendee.Email
                    });

            // ------------------------------------------
            // 8. Notify the Organizer privately
            // ------------------------------------------

            if (!string.IsNullOrEmpty(eventItem.OrganizerUserId))
            {
                var organizerMessage =
                    $"{attendee.Email} just registered for your {eventItem.Title}.";

                await _hubContext.Clients
                    .User(eventItem.OrganizerUserId)
                    .SendAsync(
                        "OrganizerNotification",
                        organizerMessage);
            }

            // ------------------------------------------
            // 9. Return to event details
            // ------------------------------------------

            TempData["Message"] =
                "You have successfully registered for this event.";

            return RedirectToAction(
                nameof(Details),
                new { id = eventId });
        }

        // ==========================================
        // SELF-UNREGISTER FROM EVENT
        // ANY AUTHENTICATED USER
        // ==========================================

        // POST: /events/1/unregister
        [Authorize]
        [HttpPost("{eventId:int}/unregister")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int eventId)
        {
            // ------------------------------------------
            // 1. Get the logged-in user's Identity ID
            // ------------------------------------------

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // ------------------------------------------
            // 2. Find this user's registration
            // ------------------------------------------

            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a =>
                    a.EventId == eventId &&
                    a.UserId == userId);

            if (attendee == null)
            {
                TempData["Message"] =
                    "You are not registered for this event.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = eventId });
            }

            // Save the information before deleting the record.
            // We need the attendee name/email for SignalR.
            var attendeeName = attendee.Name;
            var attendeeEmail = attendee.Email;

            // ------------------------------------------
            // 3. Remove the attendee from the database
            // ------------------------------------------

            _context.Attendees.Remove(attendee);

            await _context.SaveChangesAsync();

            // ------------------------------------------
            // 4. Get the new attendee count
            // ------------------------------------------

            var attendeeCount = await _context.Attendees
                .CountAsync(a => a.EventId == eventId);

            // ------------------------------------------
            // 5. Tell everyone viewing this event
            //    that the attendee was removed
            // ------------------------------------------

            await _hubContext.Clients
                .Group($"event-{eventId}")
                .SendAsync(
                    "AttendeeUnregistered",
                    new
                    {
                        count = attendeeCount,
                        name = attendeeName,
                        email = attendeeEmail
                    });

            // ------------------------------------------
            // 6. Return to Event Details
            // ------------------------------------------

            TempData["Message"] =
                "You have been unregistered from this event.";

            return RedirectToAction(
                nameof(Details),
                new { id = eventId });
        }

        // ==========================================
        // EDIT ATTENDEE
        // ORGANIZER ONLY
        // ==========================================

        // GET: /events/1/attendees/edit/{attendeeId}
        [Authorize(Roles = "Organizer")]
        [HttpGet("{eventId:int}/attendees/edit/{attendeeId}")]
        public async Task<IActionResult> EditAttendee(
            int eventId,
            string attendeeId)
        {
            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a =>
                    a.Id == attendeeId &&
                    a.EventId == eventId);

            if (attendee == null)
            {
                return NotFound();
            }

            return View(attendee);
        }

        // POST: /events/1/attendees/edit/{attendeeId}
        [Authorize(Roles = "Organizer")]
        [HttpPost("{eventId:int}/attendees/edit/{attendeeId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttendee(
            int eventId,
            string attendeeId,
            string name,
            string email)
        {
            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a =>
                    a.Id == attendeeId &&
                    a.EventId == eventId);

            if (attendee == null)
            {
                return NotFound();
            }

            attendee.Name = name;
            attendee.Email = email;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Attendee updated!";

            return RedirectToAction(
                nameof(ManageAttendees),
                new { id = eventId });
        }

        // ==========================================
        // DELETE ATTENDEE
        // ORGANIZER ONLY
        // ==========================================

        // GET: /events/1/attendees/delete/{attendeeId}
        [Authorize(Roles = "Organizer")]
        [HttpGet("{eventId:int}/attendees/delete/{attendeeId}")]
        public async Task<IActionResult> DeleteAttendee(
            int eventId,
            string attendeeId)
        {
            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a =>
                    a.Id == attendeeId &&
                    a.EventId == eventId);

            if (attendee == null)
            {
                return NotFound();
            }

            return View(attendee);
        }

        // POST: /events/1/attendees/delete/{attendeeId}
        [Authorize(Roles = "Organizer")]
        [HttpPost("{eventId:int}/attendees/delete/{attendeeId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendeeConfirmed(
            int eventId,
            string attendeeId)
        {
            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a =>
                    a.Id == attendeeId &&
                    a.EventId == eventId);

            if (attendee == null)
            {
                return NotFound();
            }

            _context.Attendees.Remove(attendee);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Attendee deleted!";

            return RedirectToAction(
                nameof(ManageAttendees),
                new { id = eventId });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}