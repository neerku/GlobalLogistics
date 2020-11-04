using GlobalDelivery.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDelivery.Repositories
{
    public class CityRepository
    {
        private readonly IMongoCollection<Models.City> cityCollection;
        private readonly IMongoClient mongoClient;

        public CityRepository(IMongoClient client)
        {
            mongoClient = client;
           cityCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                    .GetCollection<Models.City>(APIConstant.CitiesCollection);
        }

        public async Task<IReadOnlyList<City>> GetCitiesAsync()
        {            
            var cities = await cityCollection
                .Find(Builders<City>.Filter.Empty)
                .ToListAsync();
            return cities;
        }


       public async Task<City> GetCityAsync(string cityId)
        {
            var city = await cityCollection
                .Find(Builders<City>.Filter.Eq(x => x.Name, cityId))
                .FirstOrDefaultAsync();
            return city;
        }
    }
}
