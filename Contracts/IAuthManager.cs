using HotelListing.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.Contracts
{
    public interface IAuthManager
    {
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);

        //Point of a Refresh Token: If a request comes in and the token might have expired, based on the lifetime of the token, it will present a poor user experience.
        //Example, having to login every 10 minutes after the token has expired.

        Task<string> CreateRefreshToken();
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}
