using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;

namespace AdvancedStatsAndEffects
{
    public class OverTimeConsumable
    {

        public float CurrentNotConsumed
        {
            get
            {
                if (CurrentValues.Any())
                    return Math.Max(CurrentValues.Values.Max(x => x.Current), 0);
                return 0;
            }
        }

        public bool FullyConsumed
        {
            get
            {
                return CurrentNotConsumed == 0;
            }
        }

        public UniqueEntityId Id { get; set; }
        public ConcurrentDictionary<string, OverTimeProperty> CurrentValues { get; set; } = new ConcurrentDictionary<string, OverTimeProperty>();

        public PlayerData.OverTimeConsumableData GetSaveData()
        {
            var data = new PlayerData.OverTimeConsumableData()
            {
                Id = Id.DefinitionId,
                CurrentValues = new List<PlayerData.OverTimeNamedPropertyData>()
            };
            foreach (var value in CurrentValues)
            {
                data.CurrentValues.Add(value.Value.GetSaveData(value.Key));
            }
            return data;
        }

        public static OverTimeConsumable FromSaveData(PlayerData.OverTimeConsumableData data)
        {
            var returndata = new OverTimeConsumable()
            {
                Id = new UniqueEntityId(data.Id),
                CurrentValues = new ConcurrentDictionary<string, OverTimeProperty>()
            };
            foreach (var value in data.CurrentValues)
            {
                returndata.CurrentValues[value.Name] = OverTimeProperty.FromSaveData(value);
            }
            return returndata;
        }

    }

}
