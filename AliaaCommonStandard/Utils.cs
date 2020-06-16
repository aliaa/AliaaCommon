using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AliaaCommon
{
    public static class Utils
    {
        public static readonly string[] ACCEPTABLE_FILE_EXTENTIONS_TO_UPLOAD = new string[]
        {
            "png", "jpg", "jpeg", "gif",
            "doc", "docx", "pdf", "ppt", "pptx", "xls", "xlsx", "txt", "vso", "accdb"
        };

        public static bool IsFileUploadAcceptable(string mimeType, string fileName)
        {
            string fileExtention = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            return ACCEPTABLE_FILE_EXTENTIONS_TO_UPLOAD.Contains(fileExtention);
        }

        public static string DisplayName(MemberInfo member)
        {
            if (member == null)
                return null;
            Attribute attr = member.GetCustomAttribute<DisplayAttribute>();
            if (attr != null)
                return ((DisplayAttribute)attr).Name;

            attr = member.GetCustomAttribute(typeof(DisplayNameAttribute));
            if (attr != null)
                return ((DisplayNameAttribute)attr).DisplayName;

            return member.Name;
        }

        public static string DisplayName(Type classType, string memberName)
        {
            MemberInfo[] members = classType.GetMember(memberName);
            if (members == null || members.Length == 0)
                return memberName;
            return DisplayName(members[0]);
        }

        public static string DisplayName<TClass>(Expression<Func<TClass, object>> p)
        {
            string memberName;
            if (p.Body is MemberExpression)
                memberName = ((MemberExpression)p.Body).Member.Name;
            else if (p.Body is UnaryExpression)
                memberName = ((p.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            else
                throw new NotImplementedException();
            return DisplayName(typeof(TClass), memberName);
        }

        public static PhysicalAddress GetMacAddress()
        {
            const int MAC_ADDR_LENGTH = 12;
            PhysicalAddress goodMac = null;
            long maxSpeed = -1;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                PhysicalAddress mac = nic.GetPhysicalAddress();
                if (mac.ToString().Length == MAC_ADDR_LENGTH &&
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    nic.Speed > maxSpeed)
                {
                    goodMac = mac;
                    maxSpeed = nic.Speed;
                }
            }
            return goodMac;
        }


        public static byte[] XorBytes(byte[] b1, byte[] b2)
        {
            int min = Math.Min(b1.Length, b2.Length);
            byte[] result = new byte[min];
            for (int i = 0; i < min; i++)
                result[i] = (byte)(b1[i] ^ b2[i]);
            return result;
        }

        private static MD5 MD5Computer = MD5.Create();

        public static byte[] GetMd5(byte[] b)
        {
            return MD5Computer.ComputeHash(b);
        }

        public static bool ArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null || b1.Length != b2.Length)
                return false;

            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static float GetSimilarityRateOfStrings(string s1, string s2)
        {
            if (s1 == null || s2 == null)
                return s1 == s2 ? 1 : 0;

            string bigger, smaller;
            if (s1.Length > s2.Length)
            {
                bigger = s1;
                smaller = s2;
            }
            else
            {
                bigger = s2;
                smaller = s1;
            }

            for (int size = smaller.Length; size > 0; size--)
            {
                for (int skip = 0; skip <= smaller.Length - size; skip++)
                {
                    string crop = smaller.Substring(skip, size);
                    if (bigger.Contains(crop))
                        return (float)size / bigger.Length;
                }
            }
            return 0;
        }

        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static Random random = new Random(DateTime.Now.GetHashCode());
        private const string PASSWORD_CHARS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz!?@$%*&";

        public static string GenerateRandomPassword(int size = 8)
        {
            StringBuilder sb = new StringBuilder(size);
            for (int i = 0; i < size; i++)
                sb.Append(PASSWORD_CHARS[random.Next(0, PASSWORD_CHARS.Length)]);
            return sb.ToString();
        }
    }
}
