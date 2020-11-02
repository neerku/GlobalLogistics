using GlobalLogistics.Models;
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
        private readonly IMongoClient mongoClient;

        public PlaneRepository(IMongoClient client)
        {
            mongoClient = client;
            planeCollection = mongoClient.GetDatabase("logistics").GetCollection<Models.Plane>("planes");
        }


    }
}
