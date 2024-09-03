using HotelListing.Contracts;
using HotelListing.Data;

namespace HotelListing.Repository
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRespository
    {
        public HotelsRepository(HotelListingDbContext context) : base(context)
        {
        }
    }
}
