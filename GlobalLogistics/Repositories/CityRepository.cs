using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalLogistics.Repositories
{
    public class CityRepository
    {
        private readonly IMongoCollection<Models.City> cityCollection;
        private readonly IMongoClient mongoClient;

        public CityRepository(IMongoClient client)
        {
            mongoClient = client;
           cityCollection = mongoClient.GetDatabase("logistics").GetCollection<Models.City>("city");
        }
    }
}
