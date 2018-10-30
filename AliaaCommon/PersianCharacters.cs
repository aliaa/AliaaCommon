using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace AliaaCommon
{

    public class PersianCharacters
    {
        private class CharsData
        {
            public List<char> IgnoredChars { get; set; }
            public Dictionary<char, char> CharMapping { get; set; }
            public Dictionary<char, char> NumbersMapping { get; set; }
            public Dictionary<char, char> NumbersMappingInverse { get; set; }
        }

        private CharsData charsData;

        private static PersianCharacters _instance = null;
        public static PersianCharacters GetInstance(string path)
        {
            if (_instance == null)
                _instance = new PersianCharacters(path);
            return _instance;
        }

        public PersianCharacters(string path)
        {
            string filePath = Path.Combine(path, "PersianChars.json");
            charsData = JsonConvert.DeserializeObject<CharsData>(File.ReadAllText(filePath));
        }

        public char UnifyCharacter(char ch, bool farsiNumbers)
        {
            if (charsData.CharMapping.ContainsKey(ch))
                return charsData.CharMapping[ch];
            if (farsiNumbers)
            {
                if(charsData.NumbersMapping.ContainsKey(ch))
                    return charsData.NumbersMapping[ch];
            }
            else
            {
                if(charsData.NumbersMappingInverse.ContainsKey(ch))
                    return charsData.NumbersMappingInverse[ch];
            }
            return ch;
        }

        public string UnifyString(string str, bool farsiNumbers)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                if (!charsData.IgnoredChars.Contains(ch))
                    sb.Append(UnifyCharacter(ch, farsiNumbers));
            }
            return sb.ToString();
        }

        public char ConvertPersianDigitToEnglish(char ch)
        {
            if (charsData.NumbersMappingInverse.ContainsKey(ch))
                return charsData.NumbersMappingInverse[ch];
            return ch;
        }

        public string ConvertPersianDigitToEnglish(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                sb.Append(ConvertPersianDigitToEnglish(ch));
            }
            return sb.ToString();
        }

        public void UnifyStringsInObject(Type entityType, object entity, bool farsiNumbers = false)
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
                    string unified = UnifyString(original, farsiNumbers);
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
                    UnifyStringsInObject(p.PropertyType, p.GetValue(entity), farsiNumbers);
                }
                else if (value is IEnumerable)
                {
                    IEnumerable array = value as IEnumerable;
                    foreach (var item in array)
                        if (item != null)
                            UnifyStringsInObject(item.GetType(), item, farsiNumbers);
                }
            }
        }
    }
}