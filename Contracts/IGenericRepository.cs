namespace HotelListing.Contracts
{
    //Interface is a blueprint, is the contract. enforcing what should happen.
    //Type bracket of generic T represets a class.
    /// <summary>
    ////and specifies that T would represent a class, where it would represent our data objects. the data obbjects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id);
        Task<List<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(T entity);
        Task<bool> Exists(int id);
    }
}
