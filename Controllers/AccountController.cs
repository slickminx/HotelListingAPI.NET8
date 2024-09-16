using HotelListing.Contracts;
using HotelListing.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
        {
            this._authManager = authManager;
            this._logger = logger;
        }

        // POST: api/Account/register
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Register([FromBody] ApiUserDto apiUserDto)
        {
            //FromBody is coming from the body, not the paramaeters, like ID at the end of the url


            _logger.LogInformation($"Registraton Attempt for {apiUserDto.Email}");

           
                var errors = await _authManager.Register(apiUserDto);

                if (errors.Any())
                {     //Model State handles the errors or the state of model
                      //Model being the whatever datatype accepting data for the request
                      //The Model State gets set and the the model state holds the errors,
                      //and displays when a BadRequest comes in.
                      //Example: When the error "name required" that is an example of a model state being returned


                    //Can you further explain what an Model State?

                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);

                    }

                    return BadRequest(ModelState);

                }
                return Ok();
            
        
        }
        // POST: api/Account/register
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        //Questions on debugging. Different results on postman vs swagger.
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation($"Login Attempt for {loginDto.Email}");
            var authResponse = await _authManager.Login(loginDto);

                if (authResponse == null)
                {
                    return Unauthorized(); //401 http error
                                           //forbidden() aka 403 is used when you're authorized, but don't have the role to access the page
                }
                return Ok(authResponse);            

        }

        // POST: api/Account/refreshtoken
        [HttpPost]
        [Route("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDto request)
        {

            var authResponse = await _authManager.VerifyRefreshToken(request);

            if (authResponse == null)
            {
                return Unauthorized(); //401 http error
                //forbidden() aka 403 is used when you're authorized, but don't have the role to access the page
            }
            return Ok(authResponse);

        }
    }
}
