using BMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> Options) : base(Options) { }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<BookedSeat> BookedSeats { get; set; }
    }
}