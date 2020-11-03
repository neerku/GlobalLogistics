using GlobalLogistics.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GlobalLogistics.Repositories
{
    public class PlaneRepository
    {
        private readonly IMongoCollection<Models.Plane> planeCollection;
        private readonly IMongoCollection<Models.City> cityCollection;
        private readonly IMongoClient mongoClient;

        public PlaneRepository(IMongoClient client)
        {
            mongoClient = client;
            planeCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                .GetCollection<Models.Plane>(APIConstant.PlanesCollection);
            cityCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                    .GetCollection<Models.City>(APIConstant.CitiesCollection);
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
                if (string.IsNullOrWhiteSpace(cityId))
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

        public async Task<bool> DeleteReachedDestinationAsync(string id)
        {
            var filter = Builders<Models.Plane>.Filter.Eq(s => s.Callsign, id);
            UpdateDefinition<Models.Plane> update;

            try
            {
                update = Builders<Models.Plane>.Update.PopFirst(s => s.Route);

                UpdateResult actionResult = await planeCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
