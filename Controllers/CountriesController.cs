using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Data;
using System.Runtime.Intrinsics.X86;
using HotelListing.Models.Country;
using AutoMapper;
using HotelListing.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class CountriesController : ControllerBase
    {
        //DB (Dependency)? Injection (Video 26 with Williams)
        //(registered DBContext as a service in the program.cs.
        //Now can inject dbcontext in file you want,
        //example: need to interact with db in the controller
        //1) have a constructor
        //2) call in the parameter of the datatype and give it a name
        //3) initialize a private field that should be equal to what is being passed in the constructor
        //  We don't have to declare a new instance of the DB context whenever we have a new class
        //  we can simply inject, the I in SOLID, Inversion of Control
        // we don't have to instiatate a new db context every time, and helps the with the lifetime of the context
        // all being handled by the entire program
        //as soon as a request comes in, and we do whatever do in the db.  When the request is over, it kills the db instance in the background.
        //we don't have to be in charge of it, and it saves memory, time, far more efficient.   


        //Task - A task in C# is used to implement Task-based Asynchronous Programming. The Task object is typically executed asynchronously on a thread pool thread rather than synchronously on the main thread of the application.
        //C# supports parallel execution of code through multithreading. A thread is an independent execution path, able to run simultaneously with other threads.
        //The main thread creates a new thread t on which it runs a method that repeatedly prints the character “y”. Simultaneously, the main thread repeatedly prints the character “x”:
        //https://www.albahari.com/threading/

        //async - Signals to the compiler that this method contains an await statement; it contains asynchronous operations.

        //await - The await keyword provides a non-blocking way to start a task, then continue execution when that task completes.

        //ActionResult - An action is capable of returning a specific data type (see WeatherForecastController action).  When multiple return types are possible, it's common to return ActionResult, IActionResult or ActionResult<T>, where T represents the data type to be returned.

        //Entity representing Table does not get suffix aka DTO.  
       
        //inject automapper
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;

        //constructor
        //Once you inject the repository into the constructor, create the repository field and add underscore, since the field will be private
        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository)
        {
           
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
        }

        // GET: api/Countries
        [HttpGet]

        //IEnumberable - return at least one or more in a collection
        //Returning a Mapped DTO to prevent oversharing. 
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            //return Select * from Countries as a List
            //OK can be used to return 200 explictly
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<GetCountryDto>>(countries);
            return Ok(records);

        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            // var country = await _context.Countries.FindAsync(id);
            //Below is refactored
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                return NotFound();
            }

            var countryDto = _mapper.Map<CountryDto>(country);
            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //We should't be accepting the Country Object's Id from the user.  It would be dangerous.
        //Data Transfer DTO - Abstraction of the data that we want to transfer.  
        [HttpPut("{id}")]
        [Authorize]
        //All or nothing was changed
        //Country is the entire object being sent from the UI/Client

        //Creating a repository is for creating another layer of abstraction between our controller and intelligence.
        //A controller should only recieve the request, route the request and return data.  It shouldn't have the business intelligence.
        //A controller is a manager of a manager.  The controller is going to rely on the repostiory.

        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            //every entity (ex: country) has an Entity State

            ////  _context.Entry(country).State = EntityState.Modified;

            var country = await _countriesRepository.GetAsync(id);
            
            if (country == null)
            {
                return NotFound();
            }
            //takes everything from the left object into the right object
            _mapper.Map(updateCountryDto, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                //If two users try to update the same record
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
       
        //This methods expects to return a datatype of Country
        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {

            //example of a guard clause - guarding against the rest of the code, so it doesn't get executed. 
            if (ModelState.IsValid == false) 
            {
                var errors = ModelState.Select(m => m.Value.Errors);

                return BadRequest(errors);
            }
            //context represents
            //a copy of what is being passwed in controller's constructor, DBContext
            //Make sure AutoMapper is a registered/injected in the Program.cs as a services
            //Without AutoMapper
                  /*  var country = new Country
                    {
                        Name = createCountry.Name,
                        ShortName = createCountry.ShortName,
                    };*/

            //With Automapper, prevents you from setting mutliple fields
            //let auto mapper do the conversion
            //Example: Reflection - Express Mapper 
            //Unless the table changes alot (weekly or daily) - Use Chatgpt to create mapping for any number of columns.  
            var country = _mapper.Map<Country>(createCountryDto);

            await _countriesRepository.AddAsync(country);
            

            // this is what is returned in Swagger from the return in code:
            // access-control-allow-origin: *
            // content-type: application/json;
            // charset=utf-8  date: Sun,04 Aug 2024 22:20:23 GMT  
            //location: http://localhost:5292/api/Countries/4
            //server: Kestrel  transfer-encoding: chunked 

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);

        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            //Entity State is in a delete status, so generate a delete SQL?
            //Entity Framework statement

            await _countriesRepository.DeleteAsync(id);
          

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
