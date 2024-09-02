// S in SOLID - You only want one class to be responsbile for one thing.

//This is data operation, not for API front end operation. 

namespace HotelListing.Data
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ShortName { get; set; }

        public virtual IList<Hotel> Hotels { get; set; }
    }
}