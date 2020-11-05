using GlobalDelivery.Models;
using GlobalDelivery.Repositories.ChangeStream;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalDelivery.Repositories.Extension
{
    public static class ServiceExtension
    {
        public static MongoClient client;
        public static void AddDataAccessServices(this IServiceCollection services, string mongoUri)
        {
            client = new MongoClient(mongoUri);
            services.AddSingleton<IMongoClient, MongoClient>(s =>
            {
                return client;
            });

            services.AddSingleton<PlaneRepository>();
            services.AddSingleton<CargoRepository>();
            services.AddSingleton<CityRepository>();

            var wa = new WatchStream(client);
            Task.Run(() => wa.StartCollectionWatch()).ConfigureAwait(false);
            Task.Run(() => wa.AssignPlaneToCargo()).ConfigureAwait(false);
        }
    }
}

