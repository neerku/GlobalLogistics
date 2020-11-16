﻿

using MongoDB.Driver;
using GlobalDelivery.Models;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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

            Task.Run(() => this.GetInitialData()).ConfigureAwait(false);

        }

        public static List<Cargo> newlyAddedCargoList = new List<Cargo>();

        public static List<City> Cities = new List<City>();
        public static List<City> RegionalCities = new List<City>();

        public static List<Plane> Planes = new List<Plane>();
        public static List<Plane> RegionalPlanes = new List<Plane>();

      
        public async Task GetInitialData() {
            Planes = await planeCollection
                   .Find(Builders<Plane>.Filter.Empty)
                   .ToListAsync();
            RegionalPlanes = Planes.Where(x => x.IsHubCarrier == true).ToList();

            Cities = await cityCollection
                   .Find(Builders<City>.Filter.Empty)
                   .ToListAsync();
            RegionalCities = Cities.Where(x => x.IsHub == true).ToList();

        }
        public void StartCollectionWatch()
        {
            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Cargo>>().Match("{ operationType: { $in: [ 'insert','update'] } }");

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

        //public async Task AssignPlaneToCargo()
        //{
           
        //    while (true)
        //    {
        //        try
        //        {
        //            if (WatchStream.newlyAddedCargoList.Any())
        //            {
        //                var cargo = WatchStream.newlyAddedCargoList.FirstOrDefault();
        //                var city = await cityCollection
        //                         .Find(Builders<City>.Filter.Eq(x => x.Name, cargo.Location))
        //                               .FirstOrDefaultAsync();
                        
        //                    var nearestPlanes = this.GetNearestPlanes(city);
        //                    if (nearestPlanes.Any())
        //                    {
        //                        Plane nearbyPLane= new Plane();
        //                        for (int i = 0; i < nearestPlanes.Count(); i++)
        //                        {
        //                        if(nearestPlanes[i].Route.Count>10)
        //                             continue;
        //                            else
        //                            {
        //                            nearbyPLane = nearestPlanes[i];
        //                            break;
        //                            }
        //                        }
        //                    if (string.IsNullOrWhiteSpace(nearbyPLane.Callsign))
        //                    {
        //                        Thread.Sleep(5000);
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        var isAssigned = await this.LoadCargoAsync(cargo.Id, nearbyPLane.Callsign);
        //                        await this.CallPlane(nearbyPLane.Callsign, city.Name);
        //                        newlyAddedCargoList.RemoveAt(0);
        //                    }

        //                }

                        
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //            continue;
        //        }
        //    }

        //}

        public async Task AssignPlaneToCargo()
        {

            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if (WatchStream.newlyAddedCargoList.Any())
                    {
                        var cargo = WatchStream.newlyAddedCargoList.FirstOrDefault();

                        if (cargo.Courier == null && cargo.Status!=BackgroundConstant.Delivered)
                        {
                            var plane = this.GetPlaneForAssignedDestination(Cities.FirstOrDefault(x => x.Name == cargo.Location),
                                Cities.FirstOrDefault(x => x.Name == cargo.Destination));
                            if (plane != null)
                            {
                                var isAssigned = await this.LoadCargoAsync(cargo.Id, plane.Callsign);
                                newlyAddedCargoList.RemoveAt(0);
                            }


                        }
                        else { newlyAddedCargoList.RemoveAt(0); }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;
                }
            }

        }

        public Plane GetPlaneForAssignedDestination(City source, City destination)
        {
            Plane plane = null;
            if (string.Equals(source.Region, destination.Region))
            {
                //var filter = Builders<Plane>.Filter.ElemMatch(x => x.Route, destination.Name);
                //filter &= Builders<Plane>.Filter.Eq(x => x.IsHubCarrier, false);

                //plane = await planeCollection
                //     .Find(filter)
                //     .FirstOrDefaultAsync();

                plane = Planes.FirstOrDefault(x => x.IsHubCarrier == false && x.Route.Contains(destination.Name));
            }
            else
            {
                //var cityFilter = Builders<City>.Filter.Eq(x => x.Region, destination.Region);
                //cityFilter &= Builders<City>.Filter.Eq(x => x.IsHub, true);

                //var regionalhub = await cityCollection
                // .Find(cityFilter)
                // .FirstOrDefaultAsync();
                var regionalhub = RegionalCities.Where(x => x.Region == destination.Region && x.IsHub == true).FirstOrDefault();

                //var filter = Builders<Plane>.Filter.ElemMatch(x => x.Route, regionalhub.Name);
                //filter &= Builders<Plane>.Filter.Eq(x => x.IsHubCarrier, true);

                //plane = await planeCollection
                //     .Find(filter)
                //     .FirstOrDefaultAsync();
                plane = Planes.Where(x => x.IsHubCarrier == true && x.Route.Contains(regionalhub.Name)).FirstOrDefault();

            }


            return plane;
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

        //public async Task<bool> CallPlane(string id,string cityName)
        //{
        //    var plane = await planeCollection
        //        .Find(Builders<Plane>.Filter.Eq(x => x.Callsign, id))
        //        .FirstOrDefaultAsync();

        //    if (plane.Route.Contains(cityName))
        //        return true;

        //    var filter = Builders<Plane>.Filter.Eq(s => s.Callsign, id);
        //    //filter &= Builders<Plane>.Filter.Eq(s => s.Route, cityName);
        //    UpdateDefinition<Plane> update;

        //    try
        //    {
        //        update = Builders<Plane>.Update.PushEach(s => s.Route, new List<string> { cityName },position:1);

        //        UpdateResult actionResult = await planeCollection.UpdateOneAsync(filter, update);

        //        return actionResult.IsAcknowledged && actionResult.ModifiedCount == 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}
