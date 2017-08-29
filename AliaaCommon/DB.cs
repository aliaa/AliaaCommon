using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CollectionNameAttribute : Attribute
    {
        public string CollectionName { get; private set; }

        public CollectionNameAttribute(string collectionName)
        {
            this.CollectionName = collectionName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MongoIndexAttribute : Attribute
    {
        public bool SetIndex { get; set; } = true;

        public MongoIndexAttribute()
        { }

        public MongoIndexAttribute(bool SetIndex)
        {
            this.SetIndex = SetIndex;
        }
    }

    public abstract class MongoEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
    }

    public static class DB<T> where T : MongoEntity
    {
        //static readonly string DB_NAME = ConfigurationManager.AppSettings["DBName"];
        //static readonly string DB_USERNAME = ConfigurationManager.AppSettings["DBUsername"];
        //static readonly string DB_PASSWORD = ConfigurationManager.AppSettings["DBPassword"];
        //static readonly string DB_HOST = ConfigurationManager.AppSettings["DBHost"];
        //static readonly string CONN_STRING = "mongodb://" + DB_USERNAME + ":" + DB_PASSWORD + "@" + DB_HOST + "/" + DB_NAME;

        private static string dbName, connstring;
        private static bool writeLogDefaultValue, unifyCharsDefaultValue, unifyNumsDefaultValue;

        private static IMongoDatabase db = null;
        private static Dictionary<Type, object> Collections = new Dictionary<Type, object>();

        public static void Initialize(string dbName, string username, string password, bool includeDbNameInConnString, string host = "localhost", int port = 27017,
            bool writeLogDefaultValue = true, bool unifyCharsDefaultValue = true, bool unifyNumsDefaultValue = false)
        {
            DB<T>.dbName = dbName;
            StringBuilder sb = new StringBuilder("mongodb://");
            if (username != null && password != null)
                sb.Append(username + ":" + password + "@");
            sb.Append(host);
            if (port != 27017)
                sb.Append(":").Append(port);
            if (includeDbNameInConnString)
                sb.Append("/").Append(dbName);
            connstring = sb.ToString();

            DB<T>.writeLogDefaultValue = writeLogDefaultValue;
            DB<T>.unifyCharsDefaultValue = unifyCharsDefaultValue;
            DB<T>.unifyNumsDefaultValue = unifyNumsDefaultValue;
        }

        private static IMongoDatabase GetDatabase()
        {
            if (db != null)
                return db;
            //MongoClientSettings settings = new MongoClientSettings();
            //settings.Server = new MongoServerAddress("172.26.2.15");
            MongoClient client = new MongoClient(connstring);
            db = client.GetDatabase(dbName);

            ConventionRegistry.Register(
                nameof(DictionaryRepresentationConvention),
                new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfDocuments) }, _ => true);

            return db;
        }

        static readonly Type mongoIndexAttrType = typeof(MongoIndexAttribute);

        public static IMongoCollection<T> Collection
        {
            get
            {
                Type ttype = typeof(T);
                if (Collections.ContainsKey(ttype))
                    return Collections[ttype] as IMongoCollection<T>;

                string collectionName = GetCollectionName(ttype);
                IMongoCollection<T> collection = GetDatabase().GetCollection<T>(collectionName);
                foreach (PropertyInfo prop in ttype.GetProperties())
                {
                    MongoIndexAttribute attr = (MongoIndexAttribute)Attribute.GetCustomAttribute(prop, mongoIndexAttrType);
                    if (attr != null)
                        collection.Indexes.CreateOne(Builders<T>.IndexKeys.Ascending(prop.Name));
                }
                try
                {
                    Collections.Add(ttype, collection);
                }
                catch { }
                return collection;
            }
        }

        private static string GetCollectionName(Type ttype)
        {
            CollectionNameAttribute attr = (CollectionNameAttribute)Attribute.GetCustomAttribute(ttype, typeof(CollectionNameAttribute));
            if (attr != null)
                return attr.CollectionName;
            return ttype.Name;
        }

        public static T FindById(ObjectId id)
        {
            return Collection.Find(Builders<T>.Filter.Eq(t => t.Id, id)).FirstOrDefault();
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
                Collection.ReplaceOne(Builders<T>.Filter.Eq(t => t.Id, entity.Id), entity, new UpdateOptions { IsUpsert = true });
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
                        Type itemType = item.GetType();
                        UnifyCharsInObject(itemType, item);
                    }
                }
            }
        }

        public static void Remove(T obj)
        {
            Collection.DeleteOne(Builders<T>.Filter.Eq(t => t.Id, obj.Id));

            bool writeLog = true;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), collSaveType);
            if (attr != null)
                writeLog = attr.WriteLog;
            if (writeLog)
                DB<UserActivity>.Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), obj));
        }

        public static List<T> GetAll()
        {
            return Collection.Find(FilterDefinition<T>.Empty).ToList();
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
    }
}
