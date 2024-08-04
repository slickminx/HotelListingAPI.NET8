using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Data
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating {  get; set; }

        //Foreign Key....to create a new DB Table
        //Ctrl + . on the name and select generate new type
        //and then slide for more to get new type in a new file


        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; }

        public Country Country { get; set; }
    }
}
