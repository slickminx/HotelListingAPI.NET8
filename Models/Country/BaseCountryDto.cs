using System.ComponentModel.DataAnnotations;

namespace HotelListing.Models.Country
{

    //DRY Principles
    //Abstract class means you can't instiate them or intialize them as standalone objects
    //Use them for inheritance purposes
    public abstract class BaseCountryDto
    {
        [Required]
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
