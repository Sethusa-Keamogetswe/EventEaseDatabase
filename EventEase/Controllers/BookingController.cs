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
            // Start with all records from the SQL view
            var query = _context.BookingDetails.AsQueryable();

            // Check if user entered a search value
            if (!string.IsNullOrEmpty(searchString))
            {
                // Try to convert the search text into an integer
                // If successful, search BookingID exactly
                if (int.TryParse(searchString, out int bookingId))
                {
                    query = query.Where(b => b.BookingID == bookingId);
                }
                else
                {
                    // If search text is not a number,
                    // search by Event Name using Contains
                    query = query.Where(b =>
                        b.EventName != null && b.EventName.Contains(searchString));
                }
            }

            // Execute query and send result to the view
            var bookings = await query.ToListAsync();

            // Keep search text in the textbox after search
            ViewBag.SearchString = searchString;

            return View(bookings);
        }

        
        public IActionResult Create()
        {
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }
        // ============================================
        // CREATE (POST): Save new booking
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Find the date of the selected event
            var eventDate = _context.Event
                .FirstOrDefault(e => e.EventID == booking.EventID)?.EventDate;

            // ==========================================================
            // DOUBLE-BOOKING VALIDATION
            // Check whether the selected venue already has another booking
            // for an event happening on the same date
            // ==========================================================
            var conflict = await _context.Booking
                .AnyAsync(b => b.VenueID == booking.VenueID &&
                               _context.Event.Any(e =>
                                   e.EventID == b.EventID &&
                                   e.EventDate == eventDate));

            // If conflict exists, show validation error
            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
            }

            // If all validation rules pass, save booking to database
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                // Store success message for display after redirect
                TempData["SuccessMessage"] = "Booking created successfully.";

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown lists if validation fails
            ViewBag["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID); 
            ViewBag["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
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

            ViewData["Events"] = _context.Event.ToList();
            ViewData["Venues"] = _context.Venue.ToList();

            return View(booking);
        }

        // ============================================
        // EDIT (POST): Update existing booking
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            // Check if route ID matches booking ID
            if (id != booking.BookingID) return NotFound();

            // Find the date of the selected event
            var eventDate = _context.Event
                .FirstOrDefault(e => e.EventID == booking.EventID)?.EventDate;

            // ==========================================================
            // DOUBLE-BOOKING VALIDATION
            // Check whether another booking (excluding current one)
            // already exists for the same venue on the same event date
            // ==========================================================
            var conflict = await _context.Booking
                .AnyAsync(b => b.BookingID != booking.BookingID &&
                               b.VenueID == booking.VenueID &&
                               _context.Event.Any(e =>
                                   e.EventID == b.EventID &&
                                   e.EventDate == eventDate));

            // If conflict exists, show validation error
            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
            }

            // If model is valid, update booking
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Booking updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If record no longer exists, return 404
                    if (!_context.Booking.Any(e => e.BookingID == booking.BookingID))
                        return NotFound();

                    throw;
                }
            }

            // Reload dropdown lists if validation fails
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
