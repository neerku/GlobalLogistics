using GlobalLogistics.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalLogistics.Repositories.ChangeStream
{
    public class WatchStream
    {
        public void StartCollectionWatch(MongoClient mongoClient)
        {
           var cargoCollection = mongoClient.GetDatabase(APIConstant.LogisticsDatabase)
                .GetCollection<Models.Cargo>(APIConstant.CargoCollection);

            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Cargo>>().Match("{ operationType: { $in: [ 'insert'] } }");

            var cursor = cargoCollection.Watch<ChangeStreamDocument<Cargo>>(pipeline, options);

            var enumerator = cursor.ToEnumerable().GetEnumerator();
            while (true)
            {
                enumerator.MoveNext();
                //var ct = cursor.GetResumeToken();
                ChangeStreamDocument<Cargo> doc = enumerator.Current;
                // Do something here with your document
                Console.WriteLine(doc.DocumentKey);
            }
        }
    }
}
