using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // INDEX: Display all bookings + search feature
        // ============================================
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var query = _context.BookingDetails.AsQueryable();

                // Search functionality
                if (!string.IsNullOrEmpty(searchString))
                {
                    // Search by BookingID
                    if (int.TryParse(searchString, out int bookingId))
                    {
                        query = query.Where(b => b.BookingID == bookingId);
                    }
                    else
                    {
                        // Search by Event Name
                        query = query.Where(b =>
                            b.EventName != null &&
                            b.EventName.Contains(searchString));
                    }
                }

                var bookings = await query.ToListAsync();

                ViewBag.SearchString = searchString;

                return View(bookings);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while loading bookings.";

                return View(new List<BookingDetailsView>());
            }
        }

        // ============================================
        // CREATE (GET)
        // ============================================
        public IActionResult Create()
        {
            try
            {
                ViewBag.Events = _context.Event.ToList();
                ViewBag.Venues = _context.Venue.ToList();

                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while loading the page.";

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // CREATE (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                // Get selected event
                var selectedEvent = await _context.Event
                    .FirstOrDefaultAsync(e => e.EventID == booking.EventID);

                if (selectedEvent == null)
                {
                    ModelState.AddModelError("", "Selected event does not exist.");
                }
                else
                {
                    // Check for double booking
                    var conflict = await _context.Booking
                        .Include(b => b.Event)
                        .AnyAsync(b =>
                            b.VenueID == booking.VenueID &&
                            b.Event.EventDate == selectedEvent.EventDate);

                    if (conflict)
                    {
                        ModelState.AddModelError("",
                            "This venue is already booked for the selected date.");
                    }
                }

                // Save booking
                if (ModelState.IsValid)
                {
                    _context.Booking.Add(booking);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        "Booking created successfully.";

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "An unexpected error occurred while creating the booking.");
            }

            // Reload dropdowns
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();

            return View(booking);
        }

        // ============================================
        // EDIT (GET)
        // ============================================
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var booking = await _context.Booking.FindAsync(id);

                if (booking == null)
                {
                    return NotFound();
                }

                ViewBag.Events = _context.Event.ToList();
                ViewBag.Venues = _context.Venue.ToList();

                return View(booking);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while loading the booking.";

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // EDIT (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID)
            {
                return NotFound();
            }

            try
            {
                // Get selected event
                var selectedEvent = await _context.Event
                    .FirstOrDefaultAsync(e => e.EventID == booking.EventID);

                if (selectedEvent == null)
                {
                    ModelState.AddModelError("",
                        "Selected event does not exist.");
                }
                else
                {
                    // Check for double booking excluding current booking
                    var conflict = await _context.Booking
                        .Include(b => b.Event)
                        .AnyAsync(b =>
                            b.BookingID != booking.BookingID &&
                            b.VenueID == booking.VenueID &&
                            b.Event.EventDate == selectedEvent.EventDate);

                    if (conflict)
                    {
                        ModelState.AddModelError("",
                            "This venue is already booked for the selected date.");
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        "Booking updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Booking.Any(e => e.BookingID == booking.BookingID))
                {
                    return NotFound();
                }

                ModelState.AddModelError("",
                    "Another user modified this booking.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "An unexpected error occurred while updating the booking.");
            }

            // Reload dropdowns
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();

            return View(booking);
        }

        // ============================================
        // DELETE (GET)
        // ============================================
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var booking = await _context.Booking
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .FirstOrDefaultAsync(m => m.BookingID == id);

                if (booking == null)
                {
                    return NotFound();
                }

                return View(booking);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while loading the booking.";

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================
        // DELETE (POST)
        // ============================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var booking = await _context.Booking.FindAsync(id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] =
                        "Booking not found.";

                    return RedirectToAction(nameof(Index));
                }

                _context.Booking.Remove(booking);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Booking deleted successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while deleting the booking.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}