using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Data.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasData(
             new Hotel
             {
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
