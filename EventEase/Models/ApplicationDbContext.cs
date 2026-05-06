using EventEase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace EventEase.Models
{
    public class ApplicationDbContext : DbContext
    {
        internal readonly object BookingDetailsView;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Venue> Venue { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Booking> Booking { get; set; }

        public DbSet<BookingDetailsView> BookingDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookingDetailsView>()
                .ToView("vw_BookingDetails")
                .HasNoKey();
        }

    }
}

