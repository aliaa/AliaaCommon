using System.IO;
using System.Text;

namespace AliaaCommon
{
    public static class StringUtils
    {
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
    }
}
