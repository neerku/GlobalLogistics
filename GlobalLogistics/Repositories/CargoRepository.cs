using GlobalLogistics.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalLogistics.Repositories
{
    public class CargoRepository
    {
        private readonly IMongoCollection<Models.Cargo> cargoCollection;
        private readonly IMongoClient mongoClient;

        public CargoRepository(IMongoClient client)
        {
            mongoClient = client;
            cargoCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                .GetCollection<Models.Cargo>(APIConstant.CargoCollection);
        }


        public async Task<Cargo> GetCargoAsync(string id)
        {
            var cargo = await cargoCollection
                .Find(Builders<Cargo>.Filter.Eq(x => x.Id, id))
                .FirstOrDefaultAsync();
            return cargo;
        }


        public async Task<IReadOnlyList<Cargo>> GetCargoesAsync(string location)
        {

            var filter = Builders<Cargo>.Filter.Eq(x => x.Location,location);
            filter = filter & (Builders<Cargo>.Filter.Eq(x => x.Status, APIConstant.InProcess));

            //filter = filter & (Builders<Cargo>.Filter.Eq(x => x.Status, APIConstant.InProcess)) & 
            //    (Builders<Cargo>.Filter.Eq(x => x.Destination, location) 
            //    | Builders<Cargo>.Filter.Eq(x => x.Courier, location));

            var cities = await cargoCollection
                .Find(filter)
                .ToListAsync();
            return cities;
        }

        public async Task<Cargo> AddCargoAsync(string location, string destination)
        {

            try
            {
                var newCargo = new Cargo
                {
                    
                    Location= location,
                    Destination=destination,
                    Status=APIConstant.InProcess
                  
                };

               await cargoCollection.InsertOneAsync(newCargo);
                return newCargo;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateStatusDeliveredAsync(string id)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
           try
            {

               var update = Builders<Models.Cargo>.Update
                                .Set(s => s.Status, APIConstant.Delivered)
                                .Set(s => s.Received, DateTime.UtcNow);
               


                UpdateResult actionResult = await cargoCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> LoadCargoAsync(string id,string planeId)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Models.Cargo>.Update
                                 .Set(s => s.Courier, planeId);



                UpdateResult actionResult = await cargoCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UnloadCargoAsync(string id)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Models.Cargo>.Update
                                 .Set(s => s.Courier, "");



                UpdateResult actionResult = await cargoCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> MoveCargoAsync(string id, string location)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Models.Cargo>.Update
                                 .Set(s => s.Location, location);



                UpdateResult actionResult = await cargoCollection.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
