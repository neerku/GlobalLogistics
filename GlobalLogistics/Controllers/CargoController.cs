using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalLogistics.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GlobalLogistics.Controllers
{
    [Route("cargo")]
    [ApiController]
    public class CargoController : ControllerBase
    {
        private readonly CargoRepository _cargoRepository;
        private readonly CityRepository _cityRepository;
        private readonly PlaneRepository _planeRepository;

        public CargoController(CargoRepository cargoRepository, CityRepository cityRepository, PlaneRepository planeRepository)
        {
            _cargoRepository = cargoRepository;
            _cityRepository = cityRepository;
            _planeRepository = planeRepository;
        }

        [HttpGet]
        [Route("location/{location}")]
        public async Task<ActionResult> GetCargoForLocation(string location)
        {
            var city = await _cargoRepository.GetCargoesAsync(location);
            if (city == null)
                return NotFound();
            return Ok(city);
        }

        [HttpPost]
        [Route("{location}/to/{destination}")]
        public async Task<ActionResult> AddCargo(string location,string destination)
        {
            var locationCity= await _cityRepository.GetCityAsync(location);
            if (locationCity == null)
                return NotFound("Location Not found");
            var destinationCity = await _cityRepository.GetCityAsync(destination);
            if (destination == null)
                return NotFound("Destination Not found");
            var city = await _cargoRepository.AddCargoAsync(location,destination);
            if (city == null)
                return NotFound();
            return Ok(city);
        }

        [HttpPut]
        [Route("{id}/delivered")]
        public async Task<ActionResult> DeliverCargo(string id)
        {
            var cargo = await _cargoRepository.GetCargoAsync(id);
            if (cargo == null)
                return NotFound("Cargo Not found");
           var result =await _cargoRepository.UpdateStatusDeliveredAsync(id);
            
            return Ok(result);
        }

        [HttpPut]
        [Route("{id}/courier/{planeId}")]
        public async Task<ActionResult> LoadCargo(string id,string planeId)
        {
            var cargo = await _cargoRepository.GetCargoAsync(id);
            if (cargo == null)
                return NotFound("Cargo Not found");
            var plane = await _planeRepository.GetPlaneAsync(planeId);
               if (plane == null)
                return NotFound("Plane Not found");
            var result = await _cargoRepository.LoadCargoAsync(id,planeId);

            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}/courier")]
        public async Task<ActionResult> UnloadCargo(string id)
        {
            var cargo = await _cargoRepository.GetCargoAsync(id);
            if (cargo == null)
                return NotFound("Cargo Not found");
            var result = await _cargoRepository.UnloadCargoAsync(id);

            return Ok(result);
        }

        [HttpPut]
        [Route("{id}/location/{location}")]
        public async Task<ActionResult> MoveCargo(string id, string location)
        {
            var cargo = await _cargoRepository.GetCargoAsync(id);
            if (cargo == null)
                return NotFound("Cargo Not found");
            var plane = await _planeRepository.GetPlaneAsync(cargo.Courier);
            if (plane == null)
                return NotFound("Plane Not found");
            if(plane.Landed!=cargo.Destination)
                return NotFound("Plane is not yet landed");
            var result = await _cargoRepository.MoveCargoAsync(id, location);

            return Ok(result);
        }

    }
}
