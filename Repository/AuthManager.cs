using AutoMapper;
using HotelListing.Contracts;
using HotelListing.Data.HotelListing.Data;
using HotelListing.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
        }

        public async Task<string> CreateRefreshToken()
        {
            //Removes the token from the database for the user
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);

            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshToken);

            //newRefreshtoken is stored in db
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);

            return newRefreshToken;

        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            //API are stateless, they don't know who accessing it or when.
       
          
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);

            if (_user == null || !isValidUser)
            {
                //what is default in this context?
                return default;
            }


            var token = await GenerateToken();

            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()

            };


        }
           
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
           var user = _mapper.Map<ApiUser>(userDto);

           user.UserName = userDto.Email;
           
           var result = await _userManager.CreateAsync(user, userDto.Password);

           if (result.Succeeded) {

                await _userManager.AddToRoleAsync(user, "User");
           }

            return result.Errors;
            
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            //Read Token Content.  This will be strongly typed.
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);

            if(_user == null || _user.Id != request.UserId) { return null; }

            var isValidRefeshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

            if (isValidRefeshToken) 
            {
                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);
            return null;

        }

        private async Task<string> GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            //credentials 
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(_user);

            //Inside the new Claim, the x represents the string/value from the database

            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
             
            //Two ways of storing roles, in the Database or via Token
            var userClaims = await _userManager.GetClaimsAsync(_user);
            
            //Generate information about the user
            var claims = new List<Claim> 
            { 
                //This claim "Sub" represets the person 
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                //If you want to do a custom claim, create a static class, hard code them as constants,
                //so that if change is needed, you can do it in one place
                new Claim("uid", _user.Id),

            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }

}
