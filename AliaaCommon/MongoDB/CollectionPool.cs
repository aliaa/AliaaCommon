using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    internal static class CollectionPool
    {
        private static Dictionary<string, object> collections = new Dictionary<string, object>();

        public static bool Exists(string colName)
        {
            return collections.ContainsKey(colName);
        }

        public static IMongoCollection<T> Get<T>(string colName)
        {
            return (IMongoCollection<T>)collections[colName];
        }

        public static void Add<T>(string colName, IMongoCollection<T> col)
        {
            collections.Add(colName, col);
        }
    }
}
