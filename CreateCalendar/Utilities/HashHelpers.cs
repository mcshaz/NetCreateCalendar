using System.IO.Hashing;

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
        public static byte[] CreateXxHash128(params object[] objs)
        {
            return XxHash128.Hash(SequentialHashCodes(objs));
        }
        public static byte[] CreateXxHash64(params object[] objs)
        {
            return XxHash64.Hash(SequentialHashCodes(objs));
        }

        public static byte[] CreateXxHash32(params object[] objs)
        {
            return XxHash32.Hash(SequentialHashCodes(objs));
        }
        public static byte[] SequentialHashCodes(params object[] objs)
        {
            var byteList = new List<byte>(objs.Length * 4);
            foreach(object obj in objs)
            {
                switch (obj)
                {
                    case byte[] b:
                        byteList.AddRange(b);
                        break;
                    case Guid g:
                        byteList.AddRange(g.ToByteArray());
                        break;
                    default:
                        byteList.AddRange(BitConverter.GetBytes(obj.GetHashCode()));
                        break;
                }
            }
            return [.. byteList];
        }
        /*
        public static byte[] SequentialHashCodes(params object[] objs)
        {
            byte[] byteArray = new byte[objs.Length * 4];

            for (int i = 0; i < objs.Length; ++i)
                Array.Copy(BitConverter.GetBytes(objs[i].GetHashCode()), 0, byteArray, i * 4, 4);
            return byteArray;
        }
        */
        }
}
