using System.Collections.Concurrent;

namespace AdvancedStatsAndEffects
{
    public static class ConsumableInfoExtension
    {

        public static void AddToOverTimeConsumable(this ConsumableInfo consumableInfo, OverTimeConsumable target)
        {
            if (consumableInfo != null && target != null)
            {
                foreach (var item in consumableInfo.OverTimeConsumables)
                {
                    if (target.CurrentValues.ContainsKey(item.Target))
                    {
                        target.CurrentValues[item.Target].AddAmmount(item.Amount);
                    }
                }
            }
        }

        public static OverTimeConsumable GetAsOverTimeConsumable(this ConsumableInfo consumableInfo)
        {
            if (consumableInfo != null)
            {
                var data = new OverTimeConsumable()
                {
                    Id = new UniqueEntityId(consumableInfo.DefinitionId),
                    CurrentValues = new ConcurrentDictionary<string, OverTimeProperty>()
                };
                foreach (var item in consumableInfo.OverTimeConsumables)
                {
                    data.CurrentValues[item.Target] = new OverTimeProperty(item.Amount, item.Amount / consumableInfo.TimeToConsume);
                }
                return data;
            }
            return null;
        }

    }

}
