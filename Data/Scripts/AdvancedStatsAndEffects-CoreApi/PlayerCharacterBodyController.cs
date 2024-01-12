using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace AdvancedStatsAndEffects
{
    public class PlayerCharacterBodyController : CharacterBodyController
    {

        private PlayerData storeData = null;
        private Dictionary<string, float> storeStats = null;

        public PlayerCharacterBodyController(IMyCharacter entity)
            : base(entity)
        {
            
        }

        protected override void OnBeginConfigureCharacter()
        {
            base.OnBeginConfigureCharacter();
            if (StatComponent != null)
            {
                foreach (var stat in AdvancedStatsAndEffectsSession.Static.GetPlayerStatsList())
                {
                    LoadPlayerStat(stat);
                }
                foreach (var beginConfigureCharacter in AdvancedStatsAndEffectsSession.Static.BeginConfigureCharacter)
                {
                    if (beginConfigureCharacter.Action != null)
                    {
                        beginConfigureCharacter.Action(PlayerId, Entity, StatComponent, hasDied, storeStats);
                    }
                }
                if (hasDied)
                {
                    hasDied = false;
                    storeData = null;
                    storeStats = null;
                }
            }
        }

        protected override void OnCharacterDied()
        {
            base.OnCharacterDied();
            hasDied = true;
            storeData = GetStoreData();
            storeStats = GetStoreStats();
        }

        public Dictionary<string, float> GetStoreStats()
        {
            var stats = new Dictionary<string, float>();
            foreach (var stat in Stats)
            {
                stats.Add(stat.Key, stat.Value.Value);
            }
            return stats;
        }

        public PlayerData GetStoreData()
        {
            try
            {
                if (!IsValid)
                    ConfigureCharacter(Entity);
                if (IsValid)
                {
                    var data = new PlayerData
                    {
                        PlayerId = PlayerId,
                        SteamPlayerId = Player?.SteamUserId ?? 0
                    };
                    data.OverTimeConsumables.Clear();
                    foreach (var consumable in OverTimeConsumables)
                    {
                        data.OverTimeConsumables.Add(consumable.GetSaveData());
                    }
                    data.OverTimeEffects.Clear();
                    foreach (var effect in OverTimeEffects)
                    {
                        data.OverTimeEffects.Add(effect.GetSaveData());
                    }
                    data.Stats.Clear();
                    foreach (var stat in Stats)
                    {
                        data.SetStatValue(stat.Key, stat.Value.Value);
                    }
                    data.FixedStatStacks.Clear();
                    foreach (var stack in FixedStatStack)
                    {
                        data.FixedStatStacks.Add(new PlayerData.FixedStatStack() { Name = stack.Key, Value = stack.Value });
                    }
                    data.FixedStatTimers.Clear();
                    foreach (var timer in FixedStatTimer)
                    {
                        data.FixedStatTimers.Add(new PlayerData.FixedStatTimer() { Name = timer.Key, Value = timer.Value });
                    }
                    return data;
                }
                else
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), "GetStoreData Not Valid Player");
                    return null;
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
                return null;
            }
        }

        public void LoadStoreData(PlayerData storeData)
        {
            try
            {
                if (!IsValid)
                    ConfigureCharacter(Entity);
                if (IsValid)
                {
                    if (storeData != null)
                    {
                        OverTimeConsumables.Clear();
                        foreach (var consumable in storeData.OverTimeConsumables)
                        {
                            OverTimeConsumables.Add(OverTimeConsumable.FromSaveData(consumable));
                        }
                        OverTimeEffects.Clear();
                        foreach (var effect in storeData.OverTimeEffects)
                        {
                            OverTimeEffects.Add(OverTimeEffect.FromSaveData(effect));
                        }
                        foreach (var stat in storeData.Stats)
                        {
                            Stats[stat.Name].Value = stat.Value;
                        }
                        FixedStatStack.Clear();
                        foreach (var stack in storeData.FixedStatStacks)
                        {
                            FixedStatStack[stack.Name] = stack.Value;
                        }
                        FixedStatTimer.Clear();
                        foreach (var timer in storeData.FixedStatTimers)
                        {
                            FixedStatTimer[timer.Name] = timer.Value;
                        }
                    }
                    else
                    {
                        AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), "storeData null");
                    }
                }
                else
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), "LoadStoreData Not Valid Player");
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        private DateTime lastRegenEffect;
        private void CheckConsumableHasSpecialAction()
        {
            if (lastRemovedIten != null)
            {
                var removedId = new MyDefinitionId(lastRemovedIten.Value.Key.Content.TypeId, lastRemovedIten.Value.Key.Content.SubtypeId);
                if (removedId.TypeId.ToString().Contains("Consumable") && AdvancedStatsAndEffectsSession.Static.ConsumablesInfo.ContainsKey(removedId))
                {
                    var itemInfo = AdvancedStatsAndEffectsSession.Static.ConsumablesInfo[removedId];
                    var statToCheck = GetStat(itemInfo.StatTrigger);
                    if (statToCheck != null && statToCheck.HasAnyEffect() && DateTime.Now > lastRegenEffect)
                    {
                        lastRegenEffect = DateTime.Now.AddMilliseconds(statToCheck.GetEffects().Max(x => x.Value.Duration * 1000));
                        DoConsumeItem(itemInfo);
                    }
                    lastRemovedIten = null;
                }
            }
        }

        protected override void OnInventoryContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint ammount)
        {
            base.OnInventoryContentsRemoved(item, ammount);
            CheckConsumableHasSpecialAction();
        }

    }

}
