using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalLogistics.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalLogistics.Controllers
{
    [Route("planes")]
    [ApiController]
    public class PlaneController : ControllerBase
    {
        private readonly PlaneRepository _planeRepository;

        public PlaneController(PlaneRepository planeRepository)
        {
            _planeRepository = planeRepository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetPlane(string id)
        {
            var city = await _planeRepository.GetPlaneAsync(id);
            if (city == null)
                return NotFound();
            return Ok(city);
        }

        [HttpGet]
        public async Task<ActionResult> GetPlanes()
        {
            var cities = await _planeRepository.GetPlanesAsync();
            if (cities == null)
                return BadRequest("Not found");
            return Ok(cities);
        }
    }
}
