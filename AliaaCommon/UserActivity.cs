using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AliaaCommon
{
    [CollectionName("ActivityLogs")]
    [CollectionSave(WriteLog = false, UnifyChars = false, UnifyNumbers = false)]
    public class UserActivity : MongoEntity
    {
        public string Username { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.String)]

        public ActivityType ActivityType { get; set; }

        public MongoEntity Obj { get; set; }

        public string CollectionName { get; set; }

        public UserActivity()
        {
            if (HttpContext.Current != null)
                Username = HttpContext.Current.User.Identity.Name;
        }

        public UserActivity(ActivityType activityType, string collectionName, MongoEntity obj)
            : this()
        {
            this.ActivityType = activityType;
            this.CollectionName = collectionName;
            this.Obj = obj;
        }
    }

    public enum ActivityType
    {
        Delete,
        Insert,
        Update
    }
}
