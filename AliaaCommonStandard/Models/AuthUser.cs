using EasyMongoNet;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace AliaaCommon.Models
{
    [CollectionIndex(Fields: new string[] { nameof(Username) }, Unique = true)]
    [BsonIgnoreExtraElements]
    [CollectionSave(WriteLog = true)]
    public class AuthUser : MongoEntity
    {
        [Required]
        [DisplayName("نام کاربری")]
        public string Username { get; set; }

        public string HashedPassword { get; set; }

        public bool IsAdmin { get; set; }
        
        [Required]
        [DisplayName("نام")]
        public string FirstName { get; set; }
        
        [Required]
        [DisplayName("نام خانوادگی")]
        public string LastName { get; set; }

        [DisplayName("غیر فعال شده")]
        public bool Disabled { get; set; }

        public List<string> Applications { get; set; } = new List<string>();

        public string DisplayName
        {
            get { return FirstName + " " + LastName; }
        }

        [BsonIgnore]
        public string Password
        {
            set
            {
                HashedPassword = AuthUserDBExtention.GetHash(value);
            }
        }
    }

    public static class AuthUserDBExtention
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

        public static AuthUser CheckAuthentication(this IReadOnlyDbContext db, string username, string password, bool passwordIsHashed = false)
        {
            string hash;
            if (passwordIsHashed)
                hash = password;
            else
                hash = GetHash(password);
            return db.FindFirst<AuthUser>(u => u.Username == username && u.HashedPassword == hash && u.Disabled != true);
        }

        public static AuthUser GetUserByUsername(this IReadOnlyDbContext db, string username)
        {
            return db.FindFirst<AuthUser>(u => u.Username == username);
        }
    }
}
