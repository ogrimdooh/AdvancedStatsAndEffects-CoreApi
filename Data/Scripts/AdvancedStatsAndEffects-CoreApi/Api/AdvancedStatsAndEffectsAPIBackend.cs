using System;
using System.Collections.Generic;
using VRageMath;
using Sandbox.ModAPI;
using VRage.Serialization;
using VRage.Game;
using VRage;
using System.IO;
using ProtoBuf;
using VRage.ModAPI;
using Sandbox.Game;
using VRage.ObjectBuilders;
using System.Linq;
using VRage.Utils;
using VRage.Game.ModAPI;
using VRage.Game.Entity;

namespace AdvancedStatsAndEffects
{
    //Do not include this file in your project modders
    public class AdvancedStatsAndEffectsAPIBackend
    {

        public enum OverTimeEffectType
        {

            Instant = 0,
            OverTime = 1

        }

        public enum FixedEffectInConsumableType
        {

            Add = 0,
            ChanceAdd = 1,
            Remove = 2,
            ChanceRemove = 3

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class FixedEffectInConsumableInfo
        {

            [ProtoMember(1)]
            public string Target { get; set; }

            [ProtoMember(2)]
            public FixedEffectInConsumableType Type { get; set; }

            [ProtoMember(2)]
            public float Chance { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimeConsumableInfo
        {

            [ProtoMember(1)]
            public string Target { get; set; }

            [ProtoMember(2)]
            public float Amount { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimeEffectInfo
        {

            [ProtoMember(1)]
            public string Target { get; set; }

            [ProtoMember(2)]
            public OverTimeEffectType Type { get; set; }

            [ProtoMember(3)]
            public float Amount { get; set; }

            [ProtoMember(4)]
            public float TimeToEffect { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class ConsumableInfo
        {

            [ProtoMember(1)]
            public SerializableDefinitionId DefinitionId { get; set; }

            [ProtoMember(2)]
            public string StatTrigger { get; set; }

            [ProtoMember(3)]
            public float TimeToConsume { get; set; }

            [ProtoMember(4)]
            public List<OverTimeConsumableInfo> OverTimeConsumables { get; set; } = new List<OverTimeConsumableInfo>();

            [ProtoMember(5)]
            public List<OverTimeEffectInfo> OverTimeEffects { get; set; } = new List<OverTimeEffectInfo>();

            [ProtoMember(6)]
            public List<FixedEffectInConsumableInfo> FixedEffects { get; set; } = new List<FixedEffectInConsumableInfo>();

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class FixedStatInfo
        {

            [ProtoMember(1)]
            public string Id { get; set; }

            [ProtoMember(2)]
            public string Name { get; set; }

            [ProtoMember(3)]
            public int Group { get; set; }

            [ProtoMember(4)]
            public int Index { get; set; }

        }

        public const int MinVersion = 1;
        public const ushort ModHandlerID = 35875;

        private static readonly Dictionary<string, Delegate> ModAPIMethods = new Dictionary<string, Delegate>()
        {
            ["VerifyVersion"] = new Func<int, string, bool>(VerifyVersion),
            ["GetGameTime"] = new Func<long>(GetGameTime),
            ["SetStatAsConsumableTrigger"] = new Func<string, bool>(SetStatAsConsumableTrigger),
            ["ConfigureConsumable"] = new Func<string, bool>(ConfigureConsumable),
            ["ConfigureFixedStat"] = new Func<string, bool>(ConfigureFixedStat)
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

        public static long GetGameTime()
        {
            if (AdvancedStatsAndEffectsTimeManager.Instance != null)
                return AdvancedStatsAndEffectsTimeManager.Instance.GameTime;
            return 0;
        }

        public static bool SetStatAsConsumableTrigger(string statToBind)
        {
            if (AdvancedStatsAndEffectsSession.Static.BodyStatsConfigs.Keys.Contains(statToBind))
            {
                if (!AdvancedStatsAndEffectsSession.Static.TriggerStats.Contains(statToBind))
                {
                    AdvancedStatsAndEffectsSession.Static.TriggerStats.Add(statToBind);
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Set stat as Consumable Trigger : {statToBind}");
                    return true;
                }
                else
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Stat already set as Consumable Trigger : {statToBind}");
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Not a valid stat : {statToBind}");
            }
            return false;
        }

        public static bool ConfigureConsumable(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var consumableInfo = MyAPIGateway.Utilities.SerializeFromXML<ConsumableInfo>(value);
                    AdvancedStatsAndEffectsSession.Static.DoConfigureConsumable(consumableInfo);
                }
                catch (Exception e)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsAPIBackend), e);
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(AdvancedStatsAndEffectsAPIBackend), $"ConfigureConsumable : value is null");
            }
            return false;
        }

        public static bool ConfigureFixedStat(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var fixedStatInfo = MyAPIGateway.Utilities.SerializeFromXML<FixedStatInfo>(value);
                    AdvancedStatsAndEffectsSession.Static.DoConfigureFixedStat(fixedStatInfo);
                }
                catch (Exception e)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsAPIBackend), e);
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(AdvancedStatsAndEffectsAPIBackend), $"ConfigureFixedStat : value is null");
            }
            return false;
        }

    }

}