using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AliaaCommon
{
    public static class HttpContextUtils
    {
        public static string GetIPAddress()
        {
            HttpContext context = HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static byte[] GetIPAddressBytes()
        {
            string ip = GetIPAddress();
            string[] bytesStr = ip.Split('.');
            if (bytesStr.Length != 4)
                return null;
            byte[] res = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                byte b;
                if (!byte.TryParse(bytesStr[i], out b))
                    return null;
                res[i] = b;
            }
            return res;
        }
    }
}
