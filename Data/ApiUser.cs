using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace HotelListing.Data
{
    namespace HotelListing.Data
    {
        public class ApiUser : IdentityUser
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

        

        }
    }
}
