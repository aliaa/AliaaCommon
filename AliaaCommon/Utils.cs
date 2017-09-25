using FarsiLibrary;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

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

        public static string GetDateString(DateTime date)
        {
            if (Thread.CurrentThread.CurrentCulture.IsFarsiCulture())
                return PersianDateConverter.ToPersianDate(date).ToString();
            return date.ToString();
        }

        public static DataTable CreateDataTable<T>(DataTable table, IEnumerable<T> list, bool convertDateToPersian = true, string[] excludeColumns = null)
        {
            if (list == null)
                return null;
            Type ttype = typeof(T);
            PropertyInfo[] props = ttype.GetProperties();
            Dictionary<PropertyInfo, string> displayNames = new Dictionary<PropertyInfo, string>();
            foreach (PropertyInfo p in props)
            {
                if (excludeColumns != null)
                {
                    bool exclude = false;
                    foreach (string exCol in excludeColumns)
                    {
                        if (p.Name == exCol)
                        {
                            exclude = true;
                            break;
                        }
                    }
                    if (exclude)
                        continue;
                }
                string dispName = GetDisplayNameOfMember(p);
                displayNames.Add(p, dispName);
                Type propType = p.PropertyType;
                if (propType.IsEquivalentTo(typeof(ObjectId)) || propType.IsEnum || p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                    propType = typeof(string);
                Type undelying = Nullable.GetUnderlyingType(propType);
                if (undelying != null)
                    propType = undelying;
                DataColumn col = new DataColumn(dispName, propType);
                table.Columns.Add(col);
            }

            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo p in displayNames.Keys)
                {
                    object value = p.GetValue(item);
                    if (value is ObjectId)
                    {
                        value = value.ToString();
                    }
                    else if (p.PropertyType.IsEnum)
                        value = GetDisplayNameOfMember(p.PropertyType, value.ToString());
                    else if (value is DateTime && convertDateToPersian)
                        value = GetDateString((DateTime)value);
                    else if (value is IEnumerable && !(value is string))
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (object i in (IEnumerable)value)
                        {
                            sb.Append(i).Append(" ; ");
                        }
                        sb.Remove(sb.Length - 3, 3);
                        value = sb.ToString();
                    }
                    row[displayNames[p]] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public static string GetDisplayNameOfMember(MemberInfo member)
        {
            if (member == null)
                return null;
            DisplayNameAttribute attr = (DisplayNameAttribute)member.GetCustomAttribute(typeof(DisplayNameAttribute));
            if (attr == null)
            {
                DisplayNameXAttribute attrx = (DisplayNameXAttribute)member.GetCustomAttribute(typeof(DisplayNameXAttribute));
                if (attrx == null)
                    return member.Name;
                return attrx.DisplayName;
            }
            else
                return attr.DisplayName;
        }

        public static string GetDisplayNameOfMember(Type classType, string memberName)
        {
            MemberInfo[] members = classType.GetMember(memberName);
            if (members == null || members.Length == 0)
                return memberName;
            return GetDisplayNameOfMember(members[0]);
        }

        public static string GetDisplayNameOfMember<TClass>(Expression<Func<TClass, object>> p)
        {
            Type classType = typeof(TClass);
            string memberName;
            if (p.Body is MemberExpression)
                memberName = ((MemberExpression)p.Body).Member.Name;
            else if (p.Body is UnaryExpression)
                memberName = ((p.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            else
                throw new NotImplementedException();
            return GetDisplayNameOfMember(classType, memberName);
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

        public static PersianDate ParseToPersianDate(string date)
        {
            if (date.Contains(' '))
                date = date.Substring(0, date.IndexOf(' '));
            int firstSeparatorIndex = date.IndexOf('/');
            if (firstSeparatorIndex < 0)
                firstSeparatorIndex = date.IndexOf('-');
            int secondSeparatorIndex = date.IndexOf('/', firstSeparatorIndex + 1);
            if (secondSeparatorIndex < 0)
                secondSeparatorIndex = date.IndexOf('-', firstSeparatorIndex + 1);
            int year = int.Parse(date.Substring(0, firstSeparatorIndex));
            int month = int.Parse(date.Substring(firstSeparatorIndex + 1, secondSeparatorIndex - firstSeparatorIndex - 1));
            int day = int.Parse(date.Substring(secondSeparatorIndex + 1, date.Length - secondSeparatorIndex - 1));
            PersianDate pd = new PersianDate(year, month, day);
            return pd;
        }

        public static PersianDate ParseToPersianDate(string date, TimeSpan time)
        {
            PersianDate pd = ParseToPersianDate(date);
            pd.Hour = time.Hours;
            pd.Minute = time.Minutes;
            pd.Second = time.Seconds;
            pd.Millisecond = time.Milliseconds;
            return pd;
        }
        public static PersianDate AddDateToPersianDate(PersianDate original, int years, int months, int days)
        {
            DateTime gDate = PersianDateConverter.ToGregorianDateTime(original).AddYears(years).AddMonths(months).AddDays(days);
            return PersianDateConverter.ToPersianDate(gDate);
        }

        public static PersianDate AddDateToPersianDate(string pDate, int years, int months, int days)
        {
            return AddDateToPersianDate(PersianDate.Parse(pDate), years, months, days);
        }

        public static string GetPersianMonthName(int month)
        {
            switch (month)
            {
                case 1: return "فروردین";
                case 2: return "اردیبهشت";
                case 3: return "خرداد";
                case 4: return "تیر";
                case 5: return "مرداد";
                case 6: return "شهریور";
                case 7: return "مهر";
                case 8: return "آبان";
                case 9: return "آذر";
                case 10: return "دی";
                case 11: return "بهمن";
                case 12: return "اسفند";
                default: return null;
            }
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


        public static string GetDatePersianString(long time, bool isLinuxEpoch, bool alsoTime)
        {
            DateTime dt;
            if (isLinuxEpoch)
            {
                dt = new DateTime(1970, 1, 1, 0, 0, 0);
                dt = dt.AddMilliseconds(time);
            }
            else
                dt = new DateTime(time);

            PersianDate pd = new PersianDate(dt);
            if (alsoTime)
                return pd.ToString();
            return pd.ToString("yyyy/MM/dd");
        }
    }
}
