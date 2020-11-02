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
            planeCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                .GetCollection<Models.Plane>(APIConstant.PlanesCollection);
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
                .Find(Builders<Models.Plane>.Filter.Eq(x => x.CallSign, planeId))
                .FirstOrDefaultAsync();
            return plane;
        }
    }
}
