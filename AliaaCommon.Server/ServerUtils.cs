using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace AliaaCommon.Server
{
    public static class ServerUtils
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
