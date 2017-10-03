using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AliaaCommon.Models
{
    [MongoIndex("{" + nameof(Username) + ":1}")]
    public class AuthUser : MongoEntity
    {
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public bool IsAdmin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName
        {
            get { return FirstName + " " + LastName; }
        }

        [BsonIgnore]
        public string Password
        {
            set
            {
                HashedPassword = AuthUserCollectionExtention.GetHash(value);
            }
        }
    }

    public static class AuthUserCollectionExtention
    {
        private static readonly SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        public static string GetHash(string pass)
        {
            byte[] passBytes = Encoding.UTF8.GetBytes(pass);
            byte[] toHash = new byte[passBytes.Length + 1];
            Array.Copy(passBytes, toHash, passBytes.Length);
            toHash[passBytes.Length] = (byte)(passBytes.Length % 255);
            byte[] hash = sha1.ComputeHash(toHash);
            return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        public static bool CheckAuthentication(this IMongoCollection<AuthUser> collection, string username, string password)
        {
            string hash = GetHash(password);
            return collection.Count(u => u.Username == username && u.HashedPassword == hash) > 0;
        }

        public static AuthUser GetByUsername(this IMongoCollection<AuthUser> collection, string username)
        {
            return collection.Find(u => u.Username == username).FirstOrDefault();
        }

        public static AuthUser GetCurrentUser(this IMongoCollection<AuthUser> collection)
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null || HttpContext.Current.User.Identity == null || string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                return null;
            return collection.GetByUsername(HttpContext.Current.User.Identity.Name);
        }
    }
}
