using System;
using System.Security.Cryptography;

namespace AliaaCommonStandard
{
    public static class BinaryUtils
    {
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
    }
}
