using Assignment_1_Sample_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Assignment_1_Sample_Solution.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var context = serviceProvider
                .GetRequiredService<ApplicationDbContext>();

            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            // -------------------------------------------------
            // Make sure all database migrations are applied
            // -------------------------------------------------

            await context.Database.MigrateAsync();

            // -------------------------------------------------
            // Seed Roles
            // -------------------------------------------------

            string[] roles =
            {
                "Organizer",
                "Attendee"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            "Could not create role " + role + ": " +
                            string.Join(", ",
                                roleResult.Errors.Select(
                                    e => e.Description)));
                    }
                }
            }

            // -------------------------------------------------
            // Seed Organizer User
            // -------------------------------------------------

            var organizerEmail = "organizer@example.com";

            var organizer = await userManager.FindByEmailAsync(
                organizerEmail);

            if (organizer == null)
            {
                organizer = new IdentityUser
                {
                    UserName = organizerEmail,
                    Email = organizerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    organizer,
                    "Organizer123!");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Could not create Organizer user: " +
                        string.Join(", ",
                            result.Errors.Select(
                                e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                    organizer,
                    "Organizer"))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    organizer,
                    "Organizer");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Could not assign Organizer role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(
                                e => e.Description)));
                }
            }

            // -------------------------------------------------
            // Seed Attendee User
            // -------------------------------------------------

            var attendeeEmail = "attendee@example.com";

            var attendeeUser = await userManager.FindByEmailAsync(
                attendeeEmail);

            if (attendeeUser == null)
            {
                attendeeUser = new IdentityUser
                {
                    UserName = attendeeEmail,
                    Email = attendeeEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    attendeeUser,
                    "Attendee123!");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Could not create Attendee user: " +
                        string.Join(", ",
                            result.Errors.Select(
                                e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                    attendeeUser,
                    "Attendee"))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    attendeeUser,
                    "Attendee");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Could not assign Attendee role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(
                                e => e.Description)));
                }
            }

            // -------------------------------------------------
            // Seed Events
            // -------------------------------------------------

            if (!await context.Events.AnyAsync())
            {
                var events = new List<Event>
                {
                    new Event
                    {
                        Title = "Routing Workshop",
                        Description =
                            "Learn ASP.NET Core routing",
                        Date = new DateTime(
                            2026, 3, 17, 17, 56, 0),
                        Location =
                            "Algonquin College - T Building",
                        BannerUrl = "",
                        OrganizerUserId = organizer.Id
                    },

                    new Event
                    {
                        Title = "Tech Conference 2026",
                        Description =
                            "Technology Conference",
                        Date = new DateTime(
                            2026, 3, 27, 17, 56, 0),
                        Location =
                            "Ottawa Convention Centre",
                        BannerUrl = "",
                        OrganizerUserId = organizer.Id
                    },

                    new Event
                    {
                        Title = "EF Core Bootcamp",
                        Description =
                            "Entity Framework Core Training",
                        Date = new DateTime(
                            2026, 4, 6, 17, 56, 0),
                        Location = "Online",
                        BannerUrl = "",
                        OrganizerUserId = organizer.Id
                    }
                };

                context.Events.AddRange(events);

                await context.SaveChangesAsync();

                // -------------------------------------------------
                // Seed Sample Attendees
                // -------------------------------------------------

                var attendees = new List<Attendee>
                {
                    new Attendee
                    {
                        Name = "Alice Smith",
                        Email = "alice@example.com",
                        EventId = events[0].Id
                    },

                    new Attendee
                    {
                        Name = "Bob Jones",
                        Email = "bob@example.com",
                        EventId = events[0].Id
                    },

                    new Attendee
                    {
                        Name = "John Doe",
                        Email = "john@example.com",
                        EventId = events[1].Id
                    },

                    new Attendee
                    {
                        Name = "Jane Doe",
                        Email = "jane@example.com",
                        EventId = events[1].Id
                    },

                    new Attendee
                    {
                        Name = "Mike Brown",
                        Email = "mike@example.com",
                        EventId = events[2].Id
                    },

                    new Attendee
                    {
                        Name = "Emma Wilson",
                        Email = "emma@example.com",
                        EventId = events[2].Id
                    }
                };

                context.Attendees.AddRange(attendees);

                await context.SaveChangesAsync();
            }

            // -------------------------------------------------
            // Assign Organizer to existing events
            // -------------------------------------------------
            //
            // This handles events created before OrganizerUserId
            // was added to the Event model.
            //

            var eventsWithoutOrganizer =
                await context.Events
                    .Where(e =>
                        e.OrganizerUserId == null)
                    .ToListAsync();

            if (eventsWithoutOrganizer.Count > 0)
            {
                foreach (var eventItem in eventsWithoutOrganizer)
                {
                    eventItem.OrganizerUserId =
                        organizer.Id;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}