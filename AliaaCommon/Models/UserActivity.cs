using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Web;

namespace AliaaCommon.Models
{
    [CollectionOptions(Name = "ActivityLogs", Capped = true, MaxSize = 10000000)]
    [CollectionSave(WriteLog = false, NormalizeStrings = false)]
    [BsonKnownTypes(typeof(DeleteActivity), typeof(InsertActivity), typeof(UpdateActivity))]
    public abstract class UserActivity : MongoEntity
    {
        public string Username { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.String)]
        public ActivityType ActivityType { get; protected set; }

        public string CollectionName { get; set; }

        public ObjectId ObjId { get; set; }

        public UserActivity()
        {
            Username = HttpContext.Current?.User?.Identity?.Name;
        }
    }

    public enum ActivityType
    {
        Delete,
        Insert,
        Update
    }
}
