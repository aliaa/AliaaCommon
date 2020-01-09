using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EasyMongoNet;
using Newtonsoft.Json;

namespace AliaaCommon
{

    public class StringNormalizer : IStringNormalizer
    {
        public class CharsData
        {
            public HashSet<char> IgnoredChars { get; set; }
            public Dictionary<char, char> CharMapping { get; set; }
            public Dictionary<char, char> NumbersMapping { get; set; }
            public Dictionary<char, char> NumbersMappingInverse { get; set; }
        }

        private readonly CharsData charsData;
        public bool AlsoMapNumberDigits { get; set; } = false;

        private static StringNormalizer _instance = null;
        public static StringNormalizer GetInstance(string jsonFilePath)
        {
            if (_instance == null)
                _instance = new StringNormalizer(jsonFilePath);
            return _instance;
        }

        public StringNormalizer(string jsonFilePath)
        {
            charsData = JsonConvert.DeserializeObject<CharsData>(File.ReadAllText(jsonFilePath));
        }

        public StringNormalizer(CharsData charsData)
        {
            this.charsData = charsData;
        }

        private char UnifyCharacter(char ch)
        {
            if (charsData.CharMapping.ContainsKey(ch))
                return charsData.CharMapping[ch];
            if (AlsoMapNumberDigits)
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

        public string NormalizeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                if (!charsData.IgnoredChars.Contains(ch))
                    sb.Append(UnifyCharacter(ch));
            }
            return sb.ToString();
        }

        private char MapDigitChar(char ch)
        {
            if (charsData.NumbersMappingInverse.ContainsKey(ch))
                return charsData.NumbersMappingInverse[ch];
            return ch;
        }

        public string MapDigitString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                sb.Append(MapDigitChar(ch));
            }
            return sb.ToString();
        }

        public void Preprocess(object obj)
        {
            Type type = obj.GetType();
            foreach (PropertyInfo p in type.GetProperties())
            {
                object value;
                try
                {
                    value = p.GetValue(obj);
                }
                catch { continue; }
                if (value == null)
                    continue;
                if (p.PropertyType.IsEquivalentTo(typeof(string)))
                {
                    if (!p.CanRead || !p.CanWrite)
                        continue;
                    string original = value as string;
                    string normalized = NormalizeString(original);
                    if (original != normalized)
                    {
                        try
                        {
                            p.SetValue(obj, normalized);
                        }
                        catch { continue; }
                    }
                }
                else if (p.PropertyType.IsSubclassOf(typeof(IMongoEntity)))
                {
                    Preprocess(p.GetValue(obj));
                }
                else if (value is IEnumerable)
                {
                    IEnumerable array = value as IEnumerable;
                    foreach (var item in array)
                        if (item != null)
                            Preprocess(item);
                }
            }
        }
    }
}