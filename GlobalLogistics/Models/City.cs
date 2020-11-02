using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalLogistics.Models
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
        public List<string> Location { get; set; }
    }
}
