using AliaaCommon.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AliaaCommon.MongoDB
{
    public class MongoDB
    {
        public class CustomConnection
        {
            public string Type { get; set; }
            public string DBName { get; set; }
            public string ConnectionString { get; set; }
        }

        private readonly PersianCharacters persianCharacters;
        public readonly string dbName;
        private readonly string connectionString;
        private readonly bool setDictionaryConventionToArrayOfDocuments;
        private readonly IMongoDatabase database;
        private readonly List<CustomConnection> customConnections = new List<CustomConnection>();
        public bool DefaultWriteLog = false;
        public bool DefaultUnifyChars = false;
        public bool DefaultUnifyNumbers = false;
        private static readonly Dictionary<string, object> Collections = new Dictionary<string, object>();
        private static readonly Dictionary<Type, CollectionSaveAttribute> SaveAttrsCache = new Dictionary<Type, CollectionSaveAttribute>();

        public MongoDB(PersianCharacters persianCharacters, string dbName, string connectionString, bool setDictionaryConventionToArrayOfDocuments, 
            List<CustomConnection> customConnections)
        {
            this.persianCharacters = persianCharacters;
            this.dbName = dbName;
            this.connectionString = connectionString;
            this.setDictionaryConventionToArrayOfDocuments = setDictionaryConventionToArrayOfDocuments;
            this.database = GetDatabase(connectionString, dbName, setDictionaryConventionToArrayOfDocuments);
            this.customConnections = customConnections;
        }

        private IMongoDatabase GetDatabase(string connectionString, string dbName, bool setDictionaryConventionToArrayOfDocuments)
        {
            MongoClient client = new MongoClient(connectionString);
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

        public IMongoCollection<T> GetCollection<T>()
        {
            string collectionName = GetCollectionName(typeof(T));
            if (Collections.ContainsKey(collectionName))
                return (IMongoCollection<T>)Collections[collectionName];

            IMongoCollection<T> collection;
            CustomConnection customConnection = customConnections.FirstOrDefault(c => c.Type == collectionName);
            if (customConnection != null)
            {
                collection = GetCollection<T>(GetDatabase(customConnection.ConnectionString, customConnection.DBName, setDictionaryConventionToArrayOfDocuments));
            }
            else
                collection = GetCollection<T>(database);

            SetIndexes(collection);
            try
            {
                Collections.Add(collectionName, collection);
            }
            catch { }
            return collection;
        }

        private static IMongoCollection<T> GetCollection<T>(IMongoDatabase db)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionOptionsAttribute));
            string collectionName = attr?.Name ?? typeof(T).Name;
            if (attr != null && !CheckCollectionExists(db, collectionName))
            {
                CreateCollectionOptions options = new CreateCollectionOptions();
                if (attr.Capped)
                {
                    options.Capped = attr.Capped;
                    if (attr.MaxSize > 0)
                        options.MaxSize = attr.MaxSize;
                    if (attr.MaxDocuments > 0)
                        options.MaxDocuments = attr.MaxDocuments;
                }
                db.CreateCollection(collectionName, options);
            }
            return db.GetCollection<T>(collectionName);
        }

        private static string GetCollectionName(Type type)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(type, typeof(CollectionOptionsAttribute));
            return attr?.Name ?? type.Name;
        }

        private static bool CheckCollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collectionCursor = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collectionCursor.Any();
        }

        private static void SetIndexes<T>(IMongoCollection<T> collection)
        {
            foreach (MongoIndexAttribute attr in typeof(T).GetCustomAttributes<MongoIndexAttribute>())
            {
                var options = new CreateIndexOptions { Sparse = attr.Sparse, Unique = attr.Unique };
                if (attr.ExpireAfterSeconds > 0)
                    options.ExpireAfter = new TimeSpan(attr.ExpireAfterSeconds * 10000000);
                CreateIndexModel<T> model = new CreateIndexModel<T>(attr.GetIndexKeysDefinition<T>(), options);
                collection.Indexes.CreateOne(model);
            }
        }

        private static CollectionSaveAttribute GetSaveAttribute(Type type)
        {
            if (SaveAttrsCache.ContainsKey(type))
                return SaveAttrsCache[type];
            var attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(type, typeof(CollectionSaveAttribute));
            if (attr != null)
                SaveAttrsCache[type] = attr;
            return attr;
        }

        public T FindById<T>(ObjectId id) where T : MongoEntity
        {
            return GetCollection<T>().Find(t => t.Id == id).FirstOrDefault();
        }

        public T FindById<T>(string id) where T : MongoEntity
        {
            return FindById<T>(ObjectId.Parse(id));
        }

        public void Save<T>(T item) where T : MongoEntity
        {
            bool writeLog = DefaultWriteLog;
            bool unifyChars = DefaultUnifyChars;
            bool unifyNums = DefaultUnifyNumbers;
            Type type = typeof(T);
            CollectionSaveAttribute attr = GetSaveAttribute(type);
            if (attr != null)
            {
                writeLog = attr.WriteLog;
                unifyChars = attr.UnifyChars;
                unifyNums = attr.UnifyNumbers;
            }

            if (unifyChars)
                persianCharacters.UnifyStringsInObject(type, item, unifyNums);

            ActivityType activityType;
            if (item.Id == ObjectId.Empty)
            {
                GetCollection<T>().InsertOne(item);
                activityType = ActivityType.Insert;
            }
            else
            {
                GetCollection<T>().ReplaceOne(t => t.Id == item.Id, item, new UpdateOptions { IsUpsert = true });
                activityType = ActivityType.Update;
            }
            if (writeLog)
                Save(new UserActivity(activityType, GetCollectionName(type), item));
        }

        public void Delete<T>(T item) where T : MongoEntity
        {
            GetCollection<T>().DeleteOne(t => t.Id == item.Id);
            bool writeLog = DefaultWriteLog;
            var attr = GetSaveAttribute(typeof(T));
            if (attr != null)
                writeLog = attr.WriteLog;
            if(writeLog)
                Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), item));
        }

        public void Delete<T>(ObjectId id) where T : MongoEntity
        {
            bool writeLog = DefaultWriteLog;
            var attr = GetSaveAttribute(typeof(T));
            if (attr != null)
                writeLog = attr.WriteLog;
            if (writeLog)
            {
                T item = FindById<T>(id);
                if (item != null)
                    Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), item));
            }
            GetCollection<T>().DeleteOne(t => t.Id == id);
        }

        public IEnumerable<T> All<T>()
        {
            return GetCollection<T>().Find(FilterDefinition<T>.Empty).ToEnumerable();
        }

        public bool Any<T>(Expression<Func<T, bool>> filter) where T : MongoEntity
        {
            return GetCollection<T>().Find(filter).Project(t => t.Id).FirstOrDefault() != ObjectId.Empty;
        }
    }
}
