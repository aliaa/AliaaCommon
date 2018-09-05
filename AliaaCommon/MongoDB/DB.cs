using AliaaCommon.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace AliaaCommon
{
    public static class DB<T> where T : MongoEntity
    {
        private static readonly string DEFAULT_CONN_STRING = ConfigurationManager.AppSettings["MongoConnString"];
        private static readonly string DEFAULT_DB_NAME = ConfigurationManager.AppSettings["DBName"];
        private static readonly bool DONT_SET_DICTIONARY_CONVENTION_TO_ARRAY_OF_DOCUMENTS = ConfigurationManager.AppSettings["setDictionaryConventionToArrayOfDocuments"] == "false";

        public static bool writeLogDefaultValue = true, unifyCharsDefaultValue = true, unifyNumsDefaultValue;

        private static IMongoDatabase defaultDB = null;

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
                if (CollectionPool.Exists(collectionName))
                    return CollectionPool.Get<T>(collectionName);

                IMongoCollection<T> collection;
                string customConnection = ConfigurationManager.AppSettings["MongodbCustomConnection_" + collectionName];
                if (customConnection != null)
                {
                    string dbName = customConnection.Substring(0, customConnection.IndexOf(";")).Trim();
                    string connString = customConnection.Substring(customConnection.IndexOf(";") + 1).Trim();
                    collection = GetCollection(GetDatabase(connString, dbName));
                }
                else
                    collection = GetCollection(GetDefaultDatabase());

                SetIndexes(collection);
                try
                {
                    CollectionPool.Add(collectionName, collection);
                }
                catch { }
                return collection;
            }
        }

        private static IMongoCollection<T> GetCollection(IMongoDatabase db)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionOptionsAttribute));
            string collectionName = attr?.Name ?? typeof(T).Name;
            if (attr != null && !CheckCollectionExists(db, collectionName))
            {
                CreateCollectionOptions options = new CreateCollectionOptions();
                if(attr.Capped)
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

        private static bool CheckCollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collectionCursor = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collectionCursor.Any();
        }

        //public static IMongoCollection<T> GetCollection(string collectionName)
        //{
        //    return GetCollection(DEFAULT_CONN_STRING, DEFAULT_DB_NAME, collectionName);
        //}

        //public static IMongoCollection<T> GetCollection(string dbName, string collectionName)
        //{
        //    return GetCollection(DEFAULT_CONN_STRING, dbName, collectionName);
        //}

        //public static IMongoCollection<T> GetCollection(string connString, string dbName, string collectionName)
        //{
        //    IMongoCollection<T> collection = GetDatabase(connString, dbName).GetCollection<T>(collectionName);
        //    SetIndexes(collection);
        //    //try
        //    //{
        //    //    Collections.Add(typeof(T), collection);
        //    //}
        //    //catch { }
        //    return collection;
        //}

        private static string GetCollectionName(Type ttype)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(ttype, typeof(CollectionOptionsAttribute));
            return attr?.Name ?? ttype.Name;
        }

        private static void SetIndexes(IMongoCollection<T> collection)
        {
            foreach (MongoIndexAttribute attr in typeof(T).GetCustomAttributes<MongoIndexAttribute>())
            {
                var options = new CreateIndexOptions { Sparse = attr.Sparse, Unique = attr.Unique };
                if (attr.ExpireAfterSeconds > 0)
                    options.ExpireAfter = new TimeSpan(attr.ExpireAfterSeconds * 10000000);
                collection.Indexes.CreateOne(attr.GetIndexKeysDefinition<T>(), options);
            }
        }

        public static T FindById(ObjectId id)
        {
            return Collection.Find(t => t.Id == id).FirstOrDefault();
        }
        
        public static void Save(T entity)
        {
            bool writeLog = writeLogDefaultValue;
            bool unifyChars = unifyCharsDefaultValue;
            bool unifyNums = unifyNumsDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionSaveAttribute));
            if (attr != null)
            {
                writeLog = attr.WriteLog;
                unifyChars = attr.UnifyChars;
                unifyNums = attr.UnifyNumbers;
            }

            if (unifyChars)
                UnifyCharsInObject(typeof(T), entity, unifyNums);

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
                DB<UserActivity>.Save(new UserActivity(activityType, GetCollectionName(typeof(T)), entity));
        }
        
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
                if (p.PropertyType.IsEquivalentTo(typeof(string)))
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
                else if (p.PropertyType.IsSubclassOf(typeof(MongoEntity)))
                {
                    UnifyCharsInObject(p.PropertyType, p.GetValue(entity));
                }
                else if (value is IEnumerable)
                {
                    IEnumerable array = value as IEnumerable;
                    foreach (var item in array)
                        if (item != null)
                            UnifyCharsInObject(item.GetType(), item);
                }
            }
        }

        public static void Delete(T obj)
        {
            Collection.DeleteOne(t => t.Id == obj.Id);

            bool writeLog = writeLogDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionSaveAttribute));
            if (attr != null)
                writeLog = attr.WriteLog;
            if (writeLog)
                DB<UserActivity>.Save(new UserActivity(ActivityType.Delete, GetCollectionName(typeof(T)), obj));
        }
        
        public static void Delete(ObjectId Id)
        {
            bool writeLog = writeLogDefaultValue;
            CollectionSaveAttribute attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionSaveAttribute));
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

            public IBsonSerializer GetSerializer(Type type)
            {
                if (type == typeof(DateTime))
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
