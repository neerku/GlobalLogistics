
using GlobalDeliveryBackground.Models;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDeliveryBackground.ChangeStream
{
    public class WatchStream
    {
        private readonly IMongoCollection<Models.Cargo> cargoCollection;
        private readonly IMongoCollection<Models.Plane> planeCollection;
        private readonly IMongoCollection<Models.City> cityCollection;
        
        public WatchStream(MongoClient mongoClient) 
        {
           this.cargoCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                 .GetCollection<Models.Cargo>(BackgroundConstant.CargoCollection);

            this.planeCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                .GetCollection<Models.Plane>(BackgroundConstant.PlanesCollection);

           this.cityCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                .GetCollection<Models.City>(BackgroundConstant.CitiesCollection);

        }

        public static List<Cargo> newlyAddedCargoList = new List<Cargo>();
        public void StartCollectionWatch()
        {
            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Cargo>>().Match("{ operationType: { $in: [ 'insert'] } }");

            var cursor = cargoCollection.Watch<ChangeStreamDocument<Cargo>>(pipeline, options);

            var enumerator = cursor.ToEnumerable().GetEnumerator();
            while (true)
            {
                enumerator.MoveNext();
                //var ct = cursor.GetResumeToken();
                ChangeStreamDocument<Cargo> doc = enumerator.Current;
                newlyAddedCargoList.Add(doc.FullDocument);
                Console.WriteLine(doc.DocumentKey);
            }

        }

        public async Task AssignPlaneToCargo()
        {
            var firstKm = 500000;
            while (true)
            {
                try
                {
                    if (WatchStream.newlyAddedCargoList.Any())
                    {
                        var cargo = WatchStream.newlyAddedCargoList.FirstOrDefault();
                        var city = await cityCollection
                                 .Find(Builders<City>.Filter.Eq(x => x.Name, cargo.Location))
                                       .FirstOrDefaultAsync();
                        for (int i = 1; i < 100; i++)
                        {
                            var nearestPlanes = this.GetNearestPlanes(city, firstKm*i);
                            if (nearestPlanes.Any())
                            {
                                var isAssigned = await this.LoadCargoAsync(cargo.Id, nearestPlanes.First().Callsign);
                                newlyAddedCargoList.RemoveAt(0);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }
            }

        }

        public List<Models.Plane> GetNearestPlanes(City city, double distance)
        {
            var lng = city.Location[0];
            var lat = city.Location[1];
            var point = new GeoJson2DGeographicCoordinates(lng, lat);
            var pnt = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(point);
           var fil = Builders<Models.Plane>.Filter.NearSphere(p => p.CurrentLocation, pnt, distance);
           List<Models.Plane> items = planeCollection.Find(fil).ToListAsync().Result;
            return items;
        }

        public async Task<bool> LoadCargoAsync(string id, string planeId)
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
    }
}
