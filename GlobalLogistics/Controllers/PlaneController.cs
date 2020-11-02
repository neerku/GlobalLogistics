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
        private readonly CityRepository _cityRepository;
               
                
        public PlaneController(PlaneRepository planeRepository,CityRepository cityRepository)
        {
            _planeRepository = planeRepository;
            _cityRepository = cityRepository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetPlane(string id)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound();
            return Ok(plane);
        }

        [HttpGet]
        public async Task<ActionResult> GetPlanes()
        {
            var planes = await _planeRepository.GetPlanesAsync();
            if (planes == null)
                return NotFound("Not found");
            return Ok(planes);
        }

        [HttpPut]
        [Route("{id}/location/{location}/{heading}")]
        public async Task<ActionResult> UpdatePlaneLocationAndHeading(string id,List<string> location, int heading)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            if (!Enumerable.Range(0, 360).Contains(heading))
                return BadRequest("Header is out of Range");
            if (ValidateLocation(location))
                return BadRequest("Location is out of Range");

            await _planeRepository.UpdateLocationHeadingAndCityAsync(id,location,heading);

            return Ok(plane);
        }

        [HttpPut]
        [Route("{id}/location/{location}/{heading}/{cityId}")]
        public async Task<ActionResult> UpdatePlaneLocationHeadingAndCity(string id, List<string> location, int heading,string cityId)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            if (!Enumerable.Range(0, 360).Contains(heading))
                return BadRequest("Header is out of Range");
            if (ValidateLocation(location))
                return BadRequest("Location is out of Range");
            var city = await _cityRepository.GetCityAsync(cityId);
            if (city == null)
                return BadRequest("City is Invalid");

            await _planeRepository.UpdateLocationHeadingAndCityAsync(id, location, heading, cityId);

            return Ok(plane);
        }

        [HttpPut]
        [Route("{id}/route/{cityId}")]
        public async Task<ActionResult> UpdateRouteWIthSingleCity(string id, List<string> location, int heading, string cityId)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            var city = await _cityRepository.GetCityAsync(cityId);
            if (city == null)
                return BadRequest("City is Invalid");

            await _planeRepository.ReplaceRouteAsync(id, cityId);

            return Ok(plane);
        }

        [HttpPost]
        [Route("{id}/route/{cityId}")]
        public async Task<ActionResult> AddCitytoRoute(string id, List<string> location, int heading, string cityId)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            var city = await _cityRepository.GetCityAsync(cityId);
            if (city == null)
                return BadRequest("City is Invalid");

            await _planeRepository.AddCityToRouteAsync(id, cityId);

            return Ok(plane);
        }

        [HttpDelete]
        [Route("{id}/route/destination")]
        public async Task<ActionResult> DeleteReachedDestination(string id)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            if(plane.Route.Count>0 && string.Compare(plane.Route.FirstOrDefault(), plane.Landed)==0)
              await _planeRepository.DeleteReachedDestinationAsync(id);
           return Ok(plane);
        }

        private Boolean ValidateLocation(List<string> location) 
        {
            if (location.Count == 2) 
            {
                try
                {
                    foreach (var loc in location)
                    {
                        Double db = Convert.ToDouble(loc);
                    }
                    return true;
                }
                catch (FormatException)
                {

                    return false;
                }

            }
            return false;
        }
    }
}
