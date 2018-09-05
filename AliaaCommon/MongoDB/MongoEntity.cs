using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    [Serializable]
    public abstract class MongoEntity
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        [BsonId]
        public ObjectId Id { get; set; }
    }
}
