using AutoMapper;
using HotelListing.Data;
using HotelListing.Data.HotelListing.Data;
using HotelListing.Models.Country;
using HotelListing.Models.Hotel;
using HotelListing.Models.Users;

//Automapper allows us to create maps between our datatypes.

namespace HotelListing.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Country, CreateCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();
            
            CreateMap<Hotel, HotelDto>().ReverseMap();
            CreateMap<Hotel, CreateHotelDto>().ReverseMap();


            CreateMap<ApiUser, ApiUserDto>().ReverseMap();

        }
    }
}
