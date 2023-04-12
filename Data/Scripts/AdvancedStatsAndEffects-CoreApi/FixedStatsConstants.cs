using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;

namespace AdvancedStatsAndEffects
{
    public static class FixedStatsConstants
    {

        public enum ValidStats
        {

            StatsGroup01,
            StatsGroup02,
            StatsGroup03,
            StatsGroup04,
            StatsGroup05,
            StatsGroup06,
            StatsGroup07,
            StatsGroup08,
            StatsGroup09,
            StatsGroup10

        }

        [Flags]
        public enum StatsGroup01
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup02
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup03
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup04
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup05
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup06
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup07
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup08
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup09
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        [Flags]
        public enum StatsGroup10
        {

            None = 0,
            Flag01 = 1 << 1,
            Flag02 = 1 << 2,
            Flag03 = 1 << 3,
            Flag04 = 1 << 4,
            Flag05 = 1 << 5,
            Flag06 = 1 << 6,
            Flag07 = 1 << 7,
            Flag08 = 1 << 8,
            Flag09 = 1 << 9,
            Flag10 = 1 << 10,
            Flag11 = 1 << 11,
            Flag12 = 1 << 12,
            Flag13 = 1 << 13,
            Flag14 = 1 << 14,
            Flag15 = 1 << 15,
            Flag16 = 1 << 16,
            Flag17 = 1 << 17,
            Flag18 = 1 << 18,
            Flag19 = 1 << 19,
            Flag20 = 1 << 20,
            Flag21 = 1 << 21,
            Flag22 = 1 << 22,
            Flag23 = 1 << 23,
            Flag24 = 1 << 24,
            Flag25 = 1 << 25,
            Flag26 = 1 << 26,
            Flag27 = 1 << 27,
            Flag28 = 1 << 28,
            Flag29 = 1 << 29,
            Flag30 = 1 << 30,
            Flag31 = 1 << 31

        }

        public static int[] GetGroupValues(int group)
        {
            var type = GetGroupType(group);
            if (type != null)
            {
                return Enum.GetValues(type).Cast<int>().ToArray();
            }
            return null;
        }

        public static Type GetGroupType(int group)
        {
            switch (group)
            {
                case 1:
                    return typeof(StatsGroup01);
                case 2:
                    return typeof(StatsGroup02);
                case 3:
                    return typeof(StatsGroup03);
                case 4:
                    return typeof(StatsGroup04);
                case 5:
                    return typeof(StatsGroup05);
                case 6:
                    return typeof(StatsGroup06);
                case 7:
                    return typeof(StatsGroup07);
                case 8:
                    return typeof(StatsGroup08);
                case 9:
                    return typeof(StatsGroup09);
                case 10:
                    return typeof(StatsGroup10);
            }
            return null;
        }

        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct
        {
            foreach (T flag in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (value.IsFlagSet(flag))
                    yield return flag;
            }
        }

        public static bool IsFlagSet<T>(this T value, T flag) where T : struct
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static int GetMaxSetFlagValue<T>(T flags) where T : struct
        {
            int value = (int)Convert.ChangeType(flags, typeof(int));
            IEnumerable<int> setValues = Enum.GetValues(flags.GetType()).Cast<int>().Where(f => (f & value) == f);
            return setValues.Any() ? setValues.Max() : 0;
        }

    }

}
