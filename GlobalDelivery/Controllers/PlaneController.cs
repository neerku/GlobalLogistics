using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalDelivery.ActionFilters;
using GlobalDelivery.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalDelivery.Controllers
{
    [Route("planes")]
    [ApiController]
    public class PlaneController : ControllerBase
    {
        private readonly PlaneRepository _planeRepository;
        private readonly CityRepository _cityRepository;
        private readonly CargoRepository _cargoRepository;

        public PlaneController(PlaneRepository planeRepository,CityRepository cityRepository,CargoRepository cargoRepository)
        {
            _planeRepository = planeRepository;
            _cityRepository = cityRepository;
            _cargoRepository = cargoRepository;
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
        [ArrayInput("location")]
        [Route("{id}/location/{location?}/{heading}")]
        public async Task<ActionResult> UpdatePlaneLocationAndHeading(string id,[FromRoute]string[] location, int heading)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            if (!Enumerable.Range(0, 360).Contains(heading))
                return BadRequest("Header is out of Range");
            var locationValidation = ValidateLocation(location.ToList());
            if (!locationValidation.Item1)
                return BadRequest("Location is out of Range");

            var updatedPlane = await _planeRepository.UpdateLocationHeadingAndCityAsync(id, locationValidation.Item2, heading);

            return Ok(updatedPlane);
            
        }

        [HttpPut]
        [ArrayInput("location")]
        [Route("{id}/location/{location?}/{heading}/{cityId}")]
        public async Task<ActionResult> UpdatePlaneLocationHeadingAndCity(string id, [FromRoute] string[] location, int heading,string cityId)
        {
            var plane = await _planeRepository.GetPlaneAsync(id);
            if (plane == null)
                return NotFound("Plane Not found");
            if (!Enumerable.Range(0, 360).Contains(heading))
                return BadRequest("Header is out of Range");
            var locationValidation = ValidateLocation(location.ToList());
            if (!locationValidation.Item1)
                return BadRequest("Location is out of Range");
            var city = await _cityRepository.GetCityAsync(cityId);
            if (city == null)
                return BadRequest("City is Invalid");

            var updatedPlane =await _planeRepository.UpdateLocationHeadingAndCityAsync(id, locationValidation.Item2, heading, cityId);

            return Ok(updatedPlane);
        }

        [HttpPut]
        [Route("{id}/route/{cityId}")]
        public async Task<ActionResult> UpdateRouteWIthSingleCity(string id, string cityId)
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
        public async Task<ActionResult> AddCitytoRoute(string id, string cityId)
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
            
              await _planeRepository.DeleteReachedDestinationAsync(plane);
            await _planeRepository.ProcessCargoUnloadingAsync(plane);
            await _planeRepository.ProcessCargoLoadingAsync(plane); 

           return Ok(plane);
        }

      
        private (Boolean,List<double>) ValidateLocation(List<string> location) 
        {
            var newList = new List<double>();
            if (location.Count == 2) 
            {
                try
                {
                    foreach (var loc in location)
                    {
                        Double db = Convert.ToDouble(loc);
                        newList.Add(db);
                    }
                    return (true, newList);
                }
                catch (FormatException)
                {

                    return (false, newList);
                }

            }
            return (false, newList);
        }
    }
}
