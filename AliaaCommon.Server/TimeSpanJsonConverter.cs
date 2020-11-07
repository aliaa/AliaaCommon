using Newtonsoft.Json;
using System;

namespace AliaaCommon
{
    public class TimeSpanJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(long).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return TimeSpan.FromMilliseconds((long)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((TimeSpan)value).TotalMilliseconds);
        }
    }
}
