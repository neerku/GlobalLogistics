using GlobalLogistics.Models;
using GlobalLogistics.Repositories.ChangeStream;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalLogistics.Repositories.Extension
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

            var wa = new WatchStream();
            Task.Run(() => wa.StartCollectionWatch(client)).ConfigureAwait(false);
        }
    }
}

