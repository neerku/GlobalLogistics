using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalLogistics.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalLogistics.Controllers
{
    [Route("cities")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly CityRepository _cityRepository;

        public CityController(CityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetCity(string id)
        {
            var city = await _cityRepository.GetCityAsync(id);
            if (city == null) 
                return NotFound();
            return Ok(city);
        }

        [HttpGet]
        public async Task<ActionResult> GetCities()
        {
            var cities = await _cityRepository.GetCitiesAsync();
            if (cities == null)
                return BadRequest("Not found");
            return Ok(cities);
        }
    }
}
