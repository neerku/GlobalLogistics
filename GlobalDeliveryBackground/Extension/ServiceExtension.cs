using GlobalDeliveryBackground.ChangeStream;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace GlobalDeliveryBackground.Extension
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

            var wa = new WatchStream(client);
            Task.Run(() => wa.StartCollectionWatch()).ConfigureAwait(false);
            Task.Run(() => wa.AssignPlaneToCargo()).ConfigureAwait(false);
        }
    }
}

