using MongoDB.Bson;
using System;

namespace AliaaCommon.Models
{
    class InsertActivity : UserActivity
    {
        public InsertActivity()
        {
            ActivityType = ActivityType.Insert;
        }
    }
}
