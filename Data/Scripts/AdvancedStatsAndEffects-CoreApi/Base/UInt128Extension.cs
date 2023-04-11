using System;
using System.Collections.Generic;

namespace AdvancedStatsAndEffects
{
    public static class UInt128Extension
    {

        public static UInt128 ToUInt128(this Guid g)
        {
            var array = g.ToByteArray();
            var p1 = BitConverter.ToInt64(array, 8);
            var p2 = BitConverter.ToInt64(array, 0);
            return new UInt128(p1, p2);
        }

        public static Guid ToGuid(this UInt128 i)
        {
            byte[] data = new byte[16];
            Array.Copy(BitConverter.GetBytes(i.p1), 0, data, 8, 8);
            Array.Copy(BitConverter.GetBytes(i.p2), 0, data, 0, 8);
            return new Guid(data);
        }

        public static byte[] ToByteArray(this UInt128 i)
        {
            return i.ToGuid().ToByteArray();
        }

    }

}
