using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;

namespace AdvancedStatsAndEffects
{
    //Do not include this file in your project modders
    public class AdvancedStatsAndEffectsClientAPIBackend
    {

        public const int MinVersion = 1;
        public const ushort ModHandlerID = 35876;
        
        private static readonly Dictionary<string, Delegate> ModAPIMethods = new Dictionary<string, Delegate>()
        {
            ["VerifyVersion"] = new Func<int, string, bool>(VerifyVersion),
            ["GetPlayerFixedStatStack"] = new Func<string, byte>(GetPlayerFixedStatStack),
            ["GetPlayerFixedStatRemainTime"] = new Func<string, long>(GetPlayerFixedStatRemainTime),
            ["GetPlayerFixedStatUpdateHash"] = new Func<int>(GetPlayerFixedStatUpdateHash)
        };

        public static void BeforeStart()
        {
            MyAPIGateway.Utilities.SendModMessage(ModHandlerID, ModAPIMethods);
        }

        public static bool VerifyVersion(int ModAPIVersion, string ModName)
        {
            if (ModAPIVersion < MinVersion)
            {
                return false;
            }
            return true;
        }

        public static int GetPlayerFixedStatUpdateHash()
        {
            if (AdvancedStatsAndEffectsSession.Static.LastUpdateData != null)
            {
                return AdvancedStatsAndEffectsSession.Static.LastUpdateData.GetHashCode();
            }
            return 0;
        }

        public static byte GetPlayerFixedStatStack(string fixedStat)
        {
            if (AdvancedStatsAndEffectsSession.Static.LastUpdateData != null)
            {
                if (AdvancedStatsAndEffectsSession.Static.LastUpdateData.FixedStatsStacks.Any(x => x.Target == fixedStat))
                {
                    return (byte)AdvancedStatsAndEffectsSession.Static.LastUpdateData.FixedStatsStacks.FirstOrDefault(x => x.Target == fixedStat).Value;
                }
            }
            return 0;
        }

        public static long GetPlayerFixedStatRemainTime(string fixedStat)
        {
            if (AdvancedStatsAndEffectsSession.Static.LastUpdateData != null)
            {
                if (AdvancedStatsAndEffectsSession.Static.LastUpdateData.FixedStatsTimers.Any(x => x.Target == fixedStat))
                {
                    return (long)AdvancedStatsAndEffectsSession.Static.LastUpdateData.FixedStatsTimers.FirstOrDefault(x => x.Target == fixedStat).Value;
                }
            }
            return 0;
        }

    }

}