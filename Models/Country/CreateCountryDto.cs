//You can add validation here.

using System.ComponentModel.DataAnnotations;

namespace HotelListing.Models.Country
{
    public class CreateCountryDto : BaseCountryDto
   // public class CreateCountryDto : IValidatableObject
    {
       
       //I created a BaseCountryDto and moved the old properties in this CreateCountryDto there. 
        //Can the below validation class be moved to the BaseCountryDto.


       /* public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            //code the rules here
            if (ShortName == "US") {
                results.Add(new ValidationResult("US not applicable")); 

            }
            return results;
        }*/
    }
}
