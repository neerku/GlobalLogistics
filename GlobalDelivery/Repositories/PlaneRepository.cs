using GlobalDelivery.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GlobalDelivery.Repositories
{
    public class PlaneRepository
    {
        private readonly IMongoCollection<Models.Plane> planeCollection;
        private readonly IMongoCollection<Models.City> cityCollection;
        private readonly IMongoCollection<Models.Cargo> cargoCollection;
        private readonly IMongoClient mongoClient;

        public PlaneRepository(IMongoClient client)
        {
            mongoClient = client;
            planeCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                .GetCollection<Models.Plane>(APIConstant.PlanesCollection);
            cityCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                    .GetCollection<Models.City>(APIConstant.CitiesCollection);
            cargoCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                   .GetCollection<Cargo>(APIConstant.CargoCollection);
        }

        public async Task<IReadOnlyList<Models.Plane>> GetPlanesAsync()
        {
            var planes = await planeCollection
                .Find(Builders<Models.Plane>.Filter.Empty)
                .ToListAsync();
            return planes;
        }


        public async Task<Models.Plane> GetPlaneAsync(string planeId)
        {
            var plane = await planeCollection
                .Find(Builders<Models.Plane>.Filter.Eq(x => x.Callsign, planeId))
                .FirstOrDefaultAsync();
            return plane;
        }
               
        public async Task<Models.Plane> UpdateLocationHeadingAndCityAsync(string id, List<double> location, int heading,string cityId = "")
        {
            var filter = Builders<Models.Plane>.Filter.Eq(s => s.Callsign, id);
            UpdateDefinition<Models.Plane> update;

            try
            {
                if (!string.IsNullOrWhiteSpace(cityId))
                {
                        update = Builders<Models.Plane>.Update
                                        .Set(s => s.Heading, heading)
                                        .Set(s => s.Landed, cityId)
                                        .Set(s => s.CurrentLocation, location);
                }
                else
                {

                    update = Builders<Models.Plane>.Update
                    .Set(s => s.Heading, heading)
                    .Set(s => s.CurrentLocation, location);

                }

                var options = new FindOneAndUpdateOptions<Models.Plane> { ReturnDocument = ReturnDocument.After };
                var result = await planeCollection.FindOneAndUpdateAsync(filter, update, options);

            return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ReplaceRouteAsync(string id, string cityId)
        {
            var filter = Builders<Models.Plane>.Filter.Eq(s => s.Callsign, id);
            UpdateDefinition<Models.Plane> update;

            try
            {
                    update = Builders<Models.Plane>.Update
                                    .Set(s => s.Route, new List<string> { cityId });
               
                UpdateResult actionResult = await planeCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> AddCityToRouteAsync(string id, string cityId)
        {
            var filter = Builders<Models.Plane>.Filter.Eq(s => s.Callsign, id);
            UpdateDefinition<Models.Plane> update;

            try
            {
                update = Builders<Models.Plane>.Update.AddToSet(s => s.Route, cityId);

                UpdateResult actionResult = await planeCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ProcessCargoUnloadingAsync(Models.Plane plane)
        {
            var cargoes = new List<Cargo>();
            UpdateDefinition<Cargo> updateDefinition;
            if (plane.IsHubCarrier == true)
            {
                var landingCity = await cityCollection
                  .Find(Builders<City>.Filter.Eq(x => x.Name, plane.Route.FirstOrDefault()))
                  .FirstOrDefaultAsync();

                var cities = await cityCollection
                  .Find(Builders<City>.Filter.Eq(x => x.Region, landingCity.Region))
                  .ToListAsync();


                var filter = Builders<Cargo>.Filter.In(x => x.Destination, cities.Select(x => x.Name).ToList());
               filter &= Builders<Cargo>.Filter.Eq(x => x.Courier, plane.Callsign);
                cargoes = await cargoCollection
                    .Find(filter)
                    .ToListAsync();

                updateDefinition = Builders<Models.Cargo>.Update
                                     .Set(s => s.Status, APIConstant.InTransit)
                                     .Unset(s => s.Courier)
                                     .Set(s => s.DeliveryDateTime, DateTime.UtcNow);

            }
            else
            {

                var filter = (Builders<Cargo>.Filter.Eq(x => x.Destination, plane.Route.FirstOrDefault()));
                filter &= (Builders<Cargo>.Filter.Eq(x => x.Courier, plane.Callsign));
                cargoes = await cargoCollection
                    .Find(filter)
                    .ToListAsync();

                updateDefinition = Builders<Models.Cargo>.Update
                                 .Set(s => s.Status, APIConstant.Delivered)
                                 .Unset(s => s.Courier)
                                 .Set(s => s.DeliveryDateTime, DateTime.UtcNow);
            }
                           
                var updatingFilter = Builders<Models.Cargo>.Filter.In(s => s.Id, cargoes.Select(x => x.Id).ToList());
                UpdateResult actionResult = await cargoCollection.UpdateManyAsync(updatingFilter, updateDefinition);
            return actionResult.IsAcknowledged;

        }

        public async Task<bool> ProcessCargoLoadingAsync(Models.Plane plane)
        {
            var cargoes = new List<Cargo>();
            var filter = Builders<Cargo>.Filter.Eq(x => x.Courier, plane.Callsign);
            filter &= Builders<Cargo>.Filter.In(x => x.Status,new List<string> { APIConstant.InProcess,APIConstant.InTransit });
            cargoes = await cargoCollection
                    .Find(filter)
                    .ToListAsync();

              var  updateDefinition = Builders<Models.Cargo>.Update
                                     .Set(s => s.Status, APIConstant.InTransit)
                                     .Set(s => s.DeliveryDateTime, DateTime.UtcNow);

            var updatingFilter = Builders<Models.Cargo>.Filter.In(s => s.Id, cargoes.Select(x => x.Id).ToList());
            UpdateResult actionResult = await cargoCollection.UpdateManyAsync(updatingFilter, updateDefinition);
            return actionResult.IsAcknowledged;

        }

        

        public async Task<bool> LoadCargoAsync(string id, string planeId)
        {
            var filter = Builders<Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Cargo>.Update
                                 .Set(s => s.Courier, planeId);



                UpdateResult actionResult = await cargoCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteReachedDestinationAsync(Models.Plane plane)
        {
            var filter = Builders<Models.Plane>.Filter.Eq(s => s.Callsign, plane.Callsign);
            UpdateDefinition<Models.Plane> update;

            try
            {
                update = Builders<Models.Plane>.Update.PopFirst(s => s.Route);

                UpdateResult actionResult = await planeCollection.UpdateOneAsync(filter, update);

                update = Builders<Models.Plane>.Update.PushEach(s => s.Route, new List<string> { plane.Route.First() }, position: plane.Route.Count());

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
