using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalLogistics.Models
{
    public class Plane
    {
        private string _id;

        [BsonElement("_id")]
        [BsonId]
        public string Callsign
        {
            get { return this._id; }
            set { this._id = value; }
        }

        [BsonElement("heading")]
        public int Heading { get; set; }

        [BsonElement("route")]
        public List<string> Route { get; set; }

        public string Landed { get; set; }

        [BsonElement("currentLocation")]
        public List<double> CurrentLocation { get; set; }

    }
}
