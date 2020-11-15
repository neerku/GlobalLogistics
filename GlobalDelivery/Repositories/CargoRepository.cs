using GlobalDelivery.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDelivery.Repositories
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

            var cargoes = await cargoCollection
                .Find(filter)
                .ToListAsync();
            return cargoes;
        }

        public async Task<Cargo> AddCargoAsync(string location, string destination)
        {

            try
            {
                var newCargo = new Cargo
                {
                    Received=DateTime.UtcNow,
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

        public async Task<bool> UpdateStatusDeliveredAsync(List<string> ids)
        {

            var filter = Builders<Models.Cargo>.Filter.In(s => s.Id, ids);
           try
            {

               var update = Builders<Models.Cargo>.Update
                                .Set(s => s.Status, APIConstant.Delivered)
                                .Set(s => s.DeliveryDateTime, DateTime.UtcNow);



                UpdateResult actionResult = await cargoCollection.UpdateManyAsync(filter, update);

                return actionResult.IsAcknowledged;
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

        public async Task<Models.Cargo> UnloadCargoAsync(string id)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Models.Cargo>.Update
                                 .Unset(s => s.Courier);


                var options = new FindOneAndUpdateOptions<Models.Cargo> { ReturnDocument = ReturnDocument.After };
                var result = await cargoCollection.FindOneAndUpdateAsync(filter, update, options);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Cargo> MoveCargoAsync(string id, string location)
        {
            var filter = Builders<Models.Cargo>.Filter.Eq(s => s.Id, id);
            try
            {

                var update = Builders<Models.Cargo>.Update
                                 .Set(s => s.Location, location);
                                  


                var options = new FindOneAndUpdateOptions<Models.Cargo> { ReturnDocument = ReturnDocument.After };
                var result = await cargoCollection.FindOneAndUpdateAsync(filter, update, options);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
