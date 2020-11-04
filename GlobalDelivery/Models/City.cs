using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GlobalDelivery.Models
{
    public class City
    {
        private string _id;

        [BsonElement("_id")]
        [BsonId]
        public string Name
        {
            get { return this._id; }
            set { this._id = value; }
        }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("position")]
        public List<double> Location { get; set; }
    }
}
