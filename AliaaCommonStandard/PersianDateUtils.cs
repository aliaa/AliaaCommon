using FarsiLibrary.Utils;
using FarsiLibrary.Utils.Internals;
using System;
using System.Reflection;
using System.Threading;

namespace AliaaCommon
{
    public static class PersianDateUtils
    {
        public static string GetDateString(DateTime date, bool includeTime = true)
        {
            if (Thread.CurrentThread.CurrentCulture.IsFarsiCulture())
                return GetPersianDateString(date, includeTime);
            if (includeTime)
                return date.ToString();
            return date.ToShortDateString();
        }

        public static string GetPersianDateString(DateTime date, bool includeTime = true)
        {
            try
            {
                if (includeTime)
                    return PersianDateConverter.ToPersianDate(date).ToString();
                return PersianDateConverter.ToPersianDate(date).ToString("yy/mm/dd");
            }
            catch
            {
                return null;
            }
        }


        public static PersianDate ParseToPersianDate(string date)
        {
            if (date.Contains(" "))
                date = date.Substring(0, date.IndexOf(' '));
            int year, month, day;
            if (date.Contains("/") || date.Contains("-"))
            {
                int firstSeparatorIndex = date.IndexOf('/');
                if (firstSeparatorIndex < 0)
                    firstSeparatorIndex = date.IndexOf('-');
                int secondSeparatorIndex = date.IndexOf('/', firstSeparatorIndex + 1);
                if (secondSeparatorIndex < 0)
                    secondSeparatorIndex = date.IndexOf('-', firstSeparatorIndex + 1);
                year = int.Parse(date.Substring(0, firstSeparatorIndex));
                month = int.Parse(date.Substring(firstSeparatorIndex + 1, secondSeparatorIndex - firstSeparatorIndex - 1));
                day = int.Parse(date.Substring(secondSeparatorIndex + 1, date.Length - secondSeparatorIndex - 1));
            }
            else
            {
                if (date.Length == 6)
                    date = "13" + date;
                year = int.Parse(date.Substring(0, 4));
                month = int.Parse(date.Substring(4, 2));
                day = int.Parse(date.Substring(6, 2));
            }

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

        public static string GetPersianDayOfWeekName(int day)
        {
            switch (day)
            {
                case 0: return "شنبه";
                case 1: return "یکشنبه";
                case 2: return "دوشنبه";
                case 3: return "سه شنبه";
                case 4: return "چهارشنبه";
                case 5: return "پنجشنبه";
                case 6: return "جمعه";
                default: return null;
            }
        }

        public static string GetPersianDateString(long time, bool isLinuxEpoch, bool alsoTime)
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


        public static DateTime PersianDateTimeToGeorgian(string persianDateTime)
        {
            string[] dateTime = persianDateTime.Split(' ');
            string date = dateTime[0];
            string time;
            if (dateTime.Length > 1)
                time = dateTime[1];
            else
                time = "00:00:00";
            int year = int.Parse(date.Split('/')[0]);
            int month = int.Parse(date.Split('/')[1]);
            int day = int.Parse(date.Split('/')[2]); ;
            int hour = int.Parse(time.Split(':')[0]);
            int minute = int.Parse(time.Split(':')[1]);
            int second = int.Parse(time.Split(':')[2]);

            System.Globalization.PersianCalendar p = new System.Globalization.PersianCalendar();
            DateTime dt = p.ToDateTime(year, month, day, hour, minute, second, 0, 0);
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        public static void CorrectPersianDateTimes<T>(T obj)
        {
            foreach (PropertyInfo p in typeof(T).GetProperties())
            {
                if (p.PropertyType.IsEquivalentTo(typeof(DateTime)))
                {
                    DateTime dt = (DateTime)p.GetValue(obj);
                    if (dt.Year < 2000 && dt.Year > 1000)
                    {
                        dt = PersianDateConverter.ToGregorianDateTime(dt.Year + "/" + dt.Month + "/" + dt.Day);
                        dt = dt.AddHours(12);
                        p.SetValue(obj, dt);
                    }
                }
            }
        }
    }
}
