using HotelListing.Contracts;
using HotelListing.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Repository
{
    //implementation class of the interface.
    //Type T is generic off of GenericRepostiory, it inherits from the interface contract, that T should be class

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;

        public GenericRepository(HotelListingDbContext context)
        {
            this._context = context;
            //Add a constructor and inject DB Context to interact with the database

        }
        public async Task<T> AddAsync(T entity)
        {
            //Entity Framework. Using this AddAsync method, it will automatically deduce what T is and which table this entity belongs to. 
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            //return the T, aka the entity
            return entity; 
        }

        public async Task DeleteAsync(int id)
        {
            //Why is Delete and Update not Async when executing their respective statements?

            var entity = await GetAsync(id);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
          //await is used when there is an async is being used on the method
          //Go to the db and get the db set associated with T (list of records of the DB set) 
          //Parse them out to a list.  ToListAsync is the executing statement that does nothig,
          //until gives me the list, runs the query, gets all the records and returns the list. 
           return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await _context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
