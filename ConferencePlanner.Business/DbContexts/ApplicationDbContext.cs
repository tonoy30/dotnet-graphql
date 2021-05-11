using ConferencePlanner.Contacts.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.Business.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> class using the specified options.
        ///         The <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" /> method will still be called to allow further
        ///         configuration of the options.
        ///     </para>
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Attendee>()
                .HasIndex(a => a.UserName)
                .IsUnique();

            // Many-to-many: Session <-> Attendee
            modelBuilder
                .Entity<SessionAttendee>()
                .HasKey(ca => new {ca.SessionId, ca.AttendeeId});

            // Many-to-many: Speaker <-> Session
            modelBuilder
                .Entity<SessionSpeaker>()
                .HasKey(ss => new {ss.SessionId, ss.SpeakerId});
        }

        public DbSet<Speaker> Speakers { get; set; } = default!;

        public DbSet<Session> Sessions { get; set; } = default!;

        public DbSet<Track> Tracks { get; set; } = default!;

        public DbSet<Attendee> Attendees { get; set; } = default!;
    }
}