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
            cargoCollection = mongoClient.GetDatabase("logistics").GetCollection<Models.Cargo>("cargo");
        }
    }
}
