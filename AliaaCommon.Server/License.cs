using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AliaaCommon
{
    public class License<T> where T : LicenseContent
    {
        

        public T Content { get; set; }
        public string Signature { get; set; }

        //public static void Main(string[] args)
        //{
        //    License lic = new License();
        //    lic.ExpDate = new DateTime(2020, 1, 1);
        //    lic.MaxProcessCount = 999;
        //    lic.Mac = "5CE0C5E5A4BB";
        //    string json = JsonConvert.SerializeObject(lic);
        //    Console.WriteLine(json);
        //}

        public bool CheckLicense(string publicKey)
        {
            string contentString = JsonConvert.SerializeObject(Content);
            return VerifySignature(publicKey, contentString, Signature);
        }

        private static bool VerifySignature(string publicKey, string data, string signature)
        {
            RSACryptoServiceProvider rsaPublic = new RSACryptoServiceProvider(2048);
            rsaPublic.ImportCspBlob(Convert.FromBase64String(publicKey));
            SHA1CryptoServiceProvider hashFunc = new SHA1CryptoServiceProvider();
            return rsaPublic.VerifyData(Encoding.UTF8.GetBytes(data), hashFunc, Convert.FromBase64String(signature));
        }
    }
    public class LicenseContent
    {
        public string User { get; set; }
        public string LicenseBy { get; set; } = "Ali Aboutalebi 2019 (abootalebi@gmail.com)";
        public string Mac { get; set; }
        public DateTime ExpDate { get; set; }
    }
}
