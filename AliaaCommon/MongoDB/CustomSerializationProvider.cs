using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.MongoDB
{
    internal class CustomSerializationProvider : IBsonSerializationProvider
    {
        static LocalDateTimeSerializer dateTimeSerializer = new LocalDateTimeSerializer();

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(DateTime))
                return dateTimeSerializer;

            return null; // falls back to Mongo defaults
        }
    }
}
