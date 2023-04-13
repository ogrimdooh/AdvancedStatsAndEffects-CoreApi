using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRage.ObjectBuilders;

namespace AdvancedStatsAndEffects
{
    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class PlayerData
    {

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class FixedStatStack
        {

            [XmlElement]
            public string Name { get; set; }
            [XmlElement]
            public byte Value { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class FixedStatTimer
        {

            [XmlElement]
            public string Name { get; set; }
            [XmlElement]
            public long Value { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class StatData
        {

            [XmlElement]
            public string Name { get; set; }
            [XmlElement]
            public float Value { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimeNamedPropertyData : OverTimePropertyData
        {

            [XmlElement]
            public string Name { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimePropertyData
        {

            [XmlElement]
            public float Max { get; set; }
            [XmlElement]
            public float Current { get; set; }
            [XmlElement]
            public float ConsumeRate { get; set; }
            [XmlElement]
            public bool IsPositive { get; set; }

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimeConsumableData
        {

            [XmlElement]
            public SerializableDefinitionId Id { get; set; }
            [XmlArray("Values"), XmlArrayItem("Value", typeof(OverTimeNamedPropertyData))]
            public List<OverTimeNamedPropertyData> CurrentValues { get; set; } = new List<OverTimeNamedPropertyData>();

        }

        [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
        public class OverTimeEffectData
        {

            [XmlElement]
            public SerializableDefinitionId Id { get; set; }
            [XmlElement]
            public string Target { get; set; }
            [XmlElement]
            public OverTimePropertyData CurrentValue { get; set; }

        }

        [XmlElement]
        public long PlayerId { get; set; }
        [XmlElement]
        public ulong SteamPlayerId { get; set; }
        [XmlArray("Stats"), XmlArrayItem("Stat", typeof(StatData))]
        public List<StatData> Stats { get; set; } = new List<StatData>();
        [XmlArray("FixedStatStacks"), XmlArrayItem("Stack", typeof(FixedStatStack))]
        public List<FixedStatStack> FixedStatStacks { get; set; } = new List<FixedStatStack>();
        [XmlArray("FixedStatTimers"), XmlArrayItem("Timer", typeof(FixedStatTimer))]
        public List<FixedStatTimer> FixedStatTimers { get; set; } = new List<FixedStatTimer>();
        [XmlArray("OverTimeConsumables"), XmlArrayItem("Consumable", typeof(OverTimeConsumableData))]
        public List<OverTimeConsumableData> OverTimeConsumables { get; set; } = new List<OverTimeConsumableData>();
        [XmlArray("OverTimeEffects"), XmlArrayItem("Effect", typeof(OverTimeEffectData))]
        public List<OverTimeEffectData> OverTimeEffects { get; set; } = new List<OverTimeEffectData>();

        public void SetStatValue(string name, float value)
        {
            if (Stats.Any(x => x.Name == name))
                Stats.FirstOrDefault(x => x.Name == name).Value = value;
            else
                Stats.Add(new StatData()
                {
                    Name = name,
                    Value = value
                });
        }

        public float GetStatValue(string name)
        {
            if (Stats.Any(x => x.Name == name))
                return Stats.FirstOrDefault(x => x.Name == name).Value;
            return 0;
        }

    }

}
