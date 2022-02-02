using Microsoft.EntityFrameworkCore;

namespace CinemasOfSity.Models
{
    public class DataContext : DbContext
    {
        public DbSet<Cinemas.Cinema> Cinemas { get; set; }
        public DbSet<Movies.Movie> Movies { get; set; }
        public DbSet<Movies.MovieGenre> MovieGenres { get; set; }
        public DbSet<Movies.MovieCountry> MovieCountries { get; set; }
        public DbSet<CinemaSessions.CinemaSession> CinemaSessions { get; set; }
        public DbSet<Account.User> Users { get; set; }
        public DbSet<Account.Role> Roles { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
