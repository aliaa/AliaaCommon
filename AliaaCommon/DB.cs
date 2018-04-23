using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class CollectionNameAttribute : Attribute
    {
        public string CollectionName { get; private set; }

        public CollectionNameAttribute(string collectionName)
        {
            this.CollectionName = collectionName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CollectionSaveAttribute : Attribute
    {
        public bool WriteLog { get; set; }
        public bool UnifyChars { get; set; }
        public bool UnifyNumbers { get; set; }

        public CollectionSaveAttribute(bool WriteLog = true, bool UnifyChars = true, bool UnifyNumbers = false)
        {
            this.WriteLog = WriteLog;
            this.UnifyChars = UnifyChars;
            this.UnifyNumbers = UnifyNumbers;
        }
    }
    public enum MongoIndexType
    {
        Acsending,
        Descending,
        Geo2D,
        Geo2DSphere,
        Text,
        Hashed
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class MongoIndexAttribute : Attribute
    {
        private string[] Fields;
        private MongoIndexType[] Types;

        public bool Unique { get; set; } = false;
        public bool Sparse { get; set; } = false;

        public MongoIndexAttribute(string[] Fields, MongoIndexType[] Types = null)
        {
            if (Fields == null || Fields.Length == 0)
                throw new Exception();
            this.Fields = Fields;
            this.Types = Types;
        }

        public MongoIndexAttribute(string IndexDefinition)
        {
            BsonDocument doc = BsonDocument.Parse(IndexDefinition);
            List<string> fieldsList = new List<string>(doc.ElementCount);
            List<MongoIndexType> typesList = new List<MongoIndexType>(doc.ElementCount);
            foreach (var elem in doc.Elements)
            {
                fieldsList.Add(elem.Name);
                if (elem.Value.IsInt32)
                {
                    if (elem.Value.AsInt32 == -1)
                        typesList.Add(MongoIndexType.Descending);
                    else
                        typesList.Add(MongoIndexType.Acsending);
                }
                else
                {
                    switch (elem.Value.AsString.ToLower())
                    {
                        case "2d":
                            typesList.Add(MongoIndexType.Geo2D);
                            break;
                        case "2dsphere":
                            typesList.Add(MongoIndexType.Geo2DSphere);
                            break;
                        case "text":
                            typesList.Add(MongoIndexType.Text);
                            break;
                        case "hashed":
                            typesList.Add(MongoIndexType.Hashed);
                            break;
                        default:
                            throw new Exception("unkonwn index type!");
                    }
                }
            }
            Fields = fieldsList.ToArray();
            Types = typesList.ToArray();
        }

        public IndexKeysDefinition<T> GetIndexKeysDefinition<T>()
        {
            if (Fields.Length == 1)
                return GetIndexDefForOne<T>(Fields[0], Types != null && Types.Length > 0 ? Types[0] : MongoIndexType.Acsending);

            List<IndexKeysDefinition<T>> list = new List<IndexKeysDefinition<T>>(Fields.Length);
            for (int i = 0; i < Fields.Length; i++)
                list.Add(GetIndexDefForOne<T>(Fields[i], Types != null && Fields.Length > i ? Types[i] : MongoIndexType.Acsending));
            return Builders<T>.IndexKeys.Combine(list);
        }

        private static IndexKeysDefinition<T> GetIndexDefForOne<T>(string field, MongoIndexType type)
        {
            switch (type)
            {
                case MongoIndexType.Acsending:
                    return Builders<T>.IndexKeys.Ascending(field);
                case MongoIndexType.Descending:
                    return Builders<T>.IndexKeys.Descending(field);
                case MongoIndexType.Geo2D:
                    return Builders<T>.IndexKeys.Geo2D(field);
                case MongoIndexType.Geo2DSphere:
                    return Builders<T>.IndexKeys.Geo2DSphere(field);
                case MongoIndexType.Text:
                    return Builders<T>.IndexKeys.Text(field);
                case MongoIndexType.Hashed:
                    return Builders<T>.IndexKeys.Hashed(field);
                default:
                    throw new Exception();
            }
        }
    }

    [Serializable]
    public abstract class MongoEntity
    {
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        [BsonId]
        public ObjectId Id { get; set; }
    }

    public static class DB<T> where T : MongoEntity
    {
        private static readonly string DEFAULT_CONN_STRING = ConfigurationManager.AppSettings["MongoConnString"];
        private static readonly string DEFAULT_DB_NAME = ConfigurationManager.AppSettings["DBName"];
        private static readonly bool DONT_SET_DICTIONARY_CONVENTION_TO_ARRAY_OF_DOCUMENTS = ConfigurationManager.AppSettings["setDictionaryConventionToArrayOfDocuments"] == "false";

        public static bool writeLogDefaultValue = true, unifyCharsDefaultValue = true, unifyNumsDefaultValue;

        private static IMongoDatabase defaultDB = null;
        private static Dictionary<string, object> Collections = new Dictionary<string, object>();

        /// <summary>
        /// Gets DataBase object with values of written in App.config or Web.config for connection string ("MongoConnString") and database name ("DBName")
        /// </summary>
        /// <returns></returns>
        public static IMongoDatabase GetDefaultDatabase()
        {
            if (defaultDB != null)
                return defaultDB;
            defaultDB = GetDatabase(DEFAULT_CONN_STRING, DEFAULT_DB_NAME);
            return defaultDB;
        }

        public static IMongoDatabase GetDatabase(string connString, string dbName)
        {
            return GetDatabase(connString, dbName, !DONT_SET_DICTIONARY_CONVENTION_TO_ARRAY_OF_DOCUMENTS);
        }

        public static IMongoDatabase GetDatabase(string connString, string dbName, bool setDictionaryConventionToArrayOfDocuments)
        {
            MongoClient client = new MongoClient(connString);
            var db = client.GetDatabase(dbName);

            if (setDictionaryConventionToArrayOfDocuments)
            {
                ConventionRegistry.Register(
                    nameof(DictionaryRepresentationConvention),
                    new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfDocuments) }, _ => true);
            }
            BsonSerializer.RegisterSerializationProvider(new CustomSerializationProvider());
            return db;
        }

        /// <summary>
        /// Gets the collection object related to type of T class.
        /// if a custom connection config key exists (MongodbCustomConnection_[collName]) it reads the value (dbName; mongodb://yourConnString) 
        /// and connects to it; else finds in default connection and dbName.
        /// </summary>
        public static IMongoCollection<T> Collection
        {
            get
            {
                string collectionName = GetCollectionName(typeof(T));
                if (Collections.ContainsKey(collectionName))
                    return Collections[collectionName] as IMongoCollection<T>;

                IMongoCollection<T> collection;
                string customConnection = ConfigurationManager.AppSettings["MongodbCustomConnection_" + collectionName];
                if (customConnection != null)
                {
                    string dbName = customConnection.Substring(0, customConnection.IndexOf(";")).Trim();
                    string connString = customConnection.Substring(customConnection.IndexOf(";") + 1).Trim();
                    collection = GetDatabase(connString, dbName).GetCollection<T>(collectionName);
                }
                else
                    collection = GetDefaultDatabase().GetCollection<T>(collectionName);

                SetIndexes(collection);
                try
                {
                    Collections.Add(collectionName, collection);
                }
                catch { }
                return collection;
            }
        }

        public static IMongoCollection<T> GetCollection(string collectionName)
        {
            return GetCollection(DEFAULT_CONN_STRING, DEFAULT_DB_NAME, collectionName);
        }

        public static IMongoCollection<T> GetCollection(string dbName, string collectionName)
        {
            return GetCollection(DEFAULT_CONN_STRING, dbName, collectionName);
        }

        public static IMongoCollection<T> GetCollection(string connString, string dbName, string collectionName)
        {
            IMongoCollection<T> collection = GetDatabase(connString, dbName).GetCollection<T>(collectionName);
            SetIndexes(collection);
            //try
            //{
            //    Collections.Add(typeof(T), collection);
            //}
            //catch { }
            return collection;
        }

        private static string GetCollectionName(Type ttype)
        {
            CollectionNameAttribute attr = (CollectionNameAttribute)Attribute.GetCustomAttribute(ttype, typeof(CollectionNameAttribute));
            if (attr != null)
                return attr.CollectionName;
            return ttype.Name;
        }

        private static void SetIndexes(IMongoCollection<T> collection)
        {
            foreach (MongoIndexAttribute attr in typeof(T).GetCustomAttributes<MongoIndexAttribute>())
            {
                collection.Indexes.CreateOne(attr.GetIndexKeysDefinition<T>(), new CreateIndexOptions { Sparse = attr.Sparse, Unique = attr.Unique });
            }
        }

        public static T FindById(ObjectId id)
        {
            return Collection.Find(t => t.Id == id).FirstOrDefault();
        }

        private static readonly Type collSaveType = typeof(CollectionSaveAttribute);
        public static void Save(T entity)
        {
            Type ttype = typeof(T);
            bool writeLog = writeLogDefaultValue;
            bool unifyChars = unifyCharsDefaultValue;
            bool unifyNums = unifyNumsDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(ttype, collSaveType);
            if (attr != null)
            {
                writeLog = attr.WriteLog;
                unifyChars = attr.UnifyChars;
                unifyNums = attr.UnifyNumbers;
            }

            if (unifyChars)
                UnifyCharsInObject(ttype, entity, unifyNums);

            ActivityType activityType;
            if (entity.Id == ObjectId.Empty)
            {
                Collection.InsertOne(entity);
                activityType = ActivityType.Insert;
            }
            else
            {
                Collection.ReplaceOne(t => t.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
                activityType = ActivityType.Update;
            }
            if (writeLog)
                DB<UserActivity>.Save(new UserActivity(activityType, GetCollectionName(ttype), entity));
        }

        private static readonly Type STRING_TYPE = typeof(string);
        private static readonly Type MONGO_ENTITIY_TYPE = typeof(MongoEntity);
        private static void UnifyCharsInObject(Type entityType, object entity, bool farsiNumbers = false)
        {
            foreach (PropertyInfo p in entityType.GetProperties())
            {
                object value;
                try
                {
                    value = p.GetValue(entity);
                }
                catch { continue; }
                if (value == null)
                    continue;
                if (p.PropertyType.IsEquivalentTo(STRING_TYPE))
                {
                    if (!p.CanRead || !p.CanWrite)
                        continue;
                    string original = value as string;
                    string unified = PersianCharacters.UnifyString(original, farsiNumbers);
                    if (original != unified)
                    {
                        try
                        {
                            p.SetValue(entity, unified);
                        }
                        catch { continue; }
                    }
                }
                else if (p.PropertyType.IsSubclassOf(MONGO_ENTITIY_TYPE))
                {
                    UnifyCharsInObject(p.PropertyType, p.GetValue(entity));
                }
                else if (value is IEnumerable)
                {
                    IEnumerable array = value as IEnumerable;
                    foreach (var item in array)
                    {
                        if (item != null)
                        {
                            Type itemType = item.GetType();
                            UnifyCharsInObject(itemType, item);
                        }
                    }
                }
            }
        }

        public static void Delete(T obj)
        {
            Collection.DeleteOne(t => t.Id == obj.Id);

            bool writeLog = writeLogDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), collSaveType);
            if (attr != null)
                writeLog = attr.WriteLog;
            if (writeLog)
                DB<UserActivity>.Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), obj));
        }
        
        public static void Delete(ObjectId Id)
        {
            bool writeLog = writeLogDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), collSaveType);
            if (attr != null)
                writeLog = attr.WriteLog;
            if(writeLog)
            {
                T obj = FindById(Id);
                DB<UserActivity>.Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), obj));
            }
            Collection.DeleteOne(t => t.Id == Id);
        }

        public static List<T> GetAllAsList()
        {
            return Collection.Find(FilterDefinition<T>.Empty).ToList();
        }

        public static IEnumerable<T> GetAllAsEnumerable()
        {
            return Collection.Find(FilterDefinition<T>.Empty).ToEnumerable();
        }

        public static Dictionary<ObjectId, T> GetAllAsDictionary()
        {
            Dictionary<ObjectId, T> dic = new Dictionary<ObjectId, T>();
            foreach (T item in Collection.Find(Builders<T>.Filter.Empty).ToEnumerable())
                dic.Add(item.Id, item);
            return dic;
        }

        class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
        {
            private readonly DictionaryRepresentation _dictionaryRepresentation;
            public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation)
            {
                _dictionaryRepresentation = dictionaryRepresentation;
            }
            public void Apply(BsonMemberMap memberMap)
            {
                memberMap.SetSerializer(ConfigureSerializer(memberMap.GetSerializer()));
            }
            private IBsonSerializer ConfigureSerializer(IBsonSerializer serializer)
            {
                var dictionaryRepresentationConfigurable = serializer as IDictionaryRepresentationConfigurable;
                if (dictionaryRepresentationConfigurable != null)
                {
                    serializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);
                }

                var childSerializerConfigurable = serializer as IChildSerializerConfigurable;
                return childSerializerConfigurable == null
                    ? serializer
                    : childSerializerConfigurable.WithChildSerializer(ConfigureSerializer(childSerializerConfigurable.ChildSerializer));
            }
        }

        public class CustomSerializationProvider : IBsonSerializationProvider
        {
            static LocalDateTimeSerializer dateTimeSerializer = new LocalDateTimeSerializer();
            static Type dateTimeType = typeof(DateTime);

            public IBsonSerializer GetSerializer(Type type)
            {
                if (type == dateTimeType)
                    return dateTimeSerializer;

                return null; // falls back to Mongo defaults
            }
        }

        public class LocalDateTimeSerializer : DateTimeSerializer
        {
            //  MongoDB returns datetime as DateTimeKind.Utc, which cann't be used in our timezone conversion logic
            //  We overwrite it to be DateTimeKind.Unspecified
            public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var obj = base.Deserialize(context, args);
                return new DateTime(obj.Ticks, DateTimeKind.Unspecified);
            }

            //  MongoDB stores all datetime as Utc, any datetime value DateTimeKind is not DateTimeKind.Utc, will be converted to Utc first
            //  We overwrite it to be DateTimeKind.Utc, becasue we want to preserve the raw value
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
            {
                var utcValue = new DateTime(value.Ticks, DateTimeKind.Utc);
                base.Serialize(context, args, utcValue);
            }
        }
    }
}
