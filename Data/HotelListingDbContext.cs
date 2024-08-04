using Microsoft.EntityFrameworkCore;

namespace HotelListing.Data
{
    public class HotelListingDbContext: DbContext
    {
        public HotelListingDbContext(DbContextOptions options) : base(options)
        { 
        
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().HasData(
                new Country
                {
                    Id = 1, 
                    Name = "United States",
                    ShortName = "US"
                },
                new Country
                {
                    Id = 2,
                    Name = "United Kingdom",
                    ShortName = "UK"

                },
                new Country
                {
                    Id = 3,
                    Name = "Bahamas",
                    ShortName = "BS"
                }
            );
            modelBuilder.Entity<Hotel>().HasData(
                new Hotel {
                    Id = 1,
                    Name = "Sandals Resort and Spa",
                    Address = "Negril",
                    CountryId = 3,
                    Rating = 4.5
                },
                new Hotel
                {
                    Id = 2,
                    Name = "The Comfort Inn",
                    Address = "California",
                    CountryId = 1,
                    Rating = 3.3
                },
                new Hotel
                {
                    Id = 3,
                    Name = "Harrods Hotel",
                    Address = "London",
                    CountryId = 2,
                    Rating = 4.9
                }
            );
        }
    }
}
