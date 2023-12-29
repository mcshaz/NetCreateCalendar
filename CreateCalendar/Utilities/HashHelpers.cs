using System;
using System.ComponentModel;
using System.IO.Hashing;
using System.Net.Http;

namespace CreateCalendar.Utilities
{
    internal static class HashHelpers
    {
        public static int SkeetHash(params object[] objs) 
        {
            int hash = 17;
            unchecked // Overflow is fine, just wrap
            {
                for (int i = 0; i < objs.Length; ++i)
                    hash = hash * 23 + objs[i].GetHashCode();
            }
            return hash;
        }
        public static byte[] CreateXxHash64(params object[] objs)
        {
            byte[] hash = new byte[objs.Length * 4];

            for (int i = 0; i < objs.Length; ++i)
                Array.Copy(BitConverter.GetBytes(objs[i].GetHashCode()), 0, hash, i * 4, 4);
            return XxHash64.Hash(hash);
        }

        public static byte[] CreateXxHash32(params object[] objs)
        {
            byte[] hash = new byte[objs.Length * 4];

            for (int i = 0; i < objs.Length; ++i)
                Array.Copy(BitConverter.GetBytes(objs[i].GetHashCode()), 0, hash, i * 4, 4);
            return XxHash32.Hash(hash);
        }
    }
}
