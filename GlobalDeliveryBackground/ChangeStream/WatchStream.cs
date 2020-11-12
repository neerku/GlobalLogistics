

using MongoDB.Driver;
using GlobalDelivery.Models;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDeliveryBackground.ChangeStream
{
    public class WatchStream
    {
        private readonly IMongoCollection<Cargo> cargoCollection;
        private readonly IMongoCollection<Plane> planeCollection;
        private readonly IMongoCollection<City> cityCollection;
        
        public WatchStream(MongoClient mongoClient) 
        {
           this.cargoCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                 .GetCollection<Cargo>(BackgroundConstant.CargoCollection);

            this.planeCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                .GetCollection<Plane>(BackgroundConstant.PlanesCollection);

           this.cityCollection = mongoClient.GetDatabase(BackgroundConstant.LogisticsDatabase)
                .GetCollection<City>(BackgroundConstant.CitiesCollection);

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
                        
                            var nearestPlanes = this.GetNearestPlanes(city);
                            if (nearestPlanes.Any())
                            {
                                var isAssigned = await this.LoadCargoAsync(cargo.Id, nearestPlanes.First().Callsign);
                                newlyAddedCargoList.RemoveAt(0);
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

        public List<Plane> GetNearestPlanes(City city)
        {
            var lng = city.Location[0];
            var lat = city.Location[1];
            var point = new GeoJson2DGeographicCoordinates(lng, lat);
            var pnt = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(point);
           var fil = Builders<Plane>.Filter.NearSphere(p => p.CurrentLocation, pnt);
           List<Plane> items = planeCollection.Find(fil).ToListAsync().Result;
            return items;
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
    }
}
