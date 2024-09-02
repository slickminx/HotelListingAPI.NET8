using HotelListing.Models.Hotel;

namespace HotelListing.Models.Country
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        //A DTO can only play with other DTO, not data models fields
        //Unless the DTO is being mapped to a data model
        public List<HotelDto> Hotels { get; set; }
    }

}
