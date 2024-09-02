using HotelListing.Contracts;
using HotelListing.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Repository
{
    //Initialize or generate DB context by adding a constructor. 
    //Why have ICountryRepository here when it contains no methods?

    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelListingDbContext _context;

        //Taking a copy of the HotelListing Db Context and then passing it down to the base
        //Is base a special keyword? or is referrig to a base file?
        public CountriesRepository(HotelListingDbContext context) : base(context)
        {

            //to access the db, need to create private context field
            //now that we have a field inside of the constructor, we have a copy 
            //of the db context, locally?
            this._context = context;
        }




           public async Task<Country> GetDetails(int id)
           {
               return await _context.Countries.Include(q => q.Hotels)
                   .FirstOrDefaultAsync(q => q.Id == id);
           }
    }
}
