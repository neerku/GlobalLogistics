using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GlobalDeliveryBackground.Models
{
    public class Cargo
    {
        private string _id;

        [BsonElement("_id")]
       [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string Destination { get; set; }

        public string Location { get; set; }

        public string Courier { get; set; }

        public DateTime Received { get; set; }

        public string Status { get; set; }
    }
}
