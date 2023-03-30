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
        private long deltaTime = 0;
        private long spendTime = 0;

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
                if (hasDied && storeData != null)
                {


                    hasDied = false;
                    storeData = null;
                }
            }
        }

        protected override void OnCharacterDied()
        {
            base.OnCharacterDied();
            hasDied = true;
            storeData = GetStoreData();
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
                    foreach (var stat in BodyStats)
                    {
                        data.SetStatValue(stat.Key, stat.Value.CurrentValue);
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
                        BodyStats.Clear();
                        foreach (var effect in storeData.Stats)
                        {
                            BodyStats[effect.Name].CurrentValue = effect.Value;
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

        private long GetGameTime()
        {
            return ExtendedSurvivalCoreAPI.Registered ? ExtendedSurvivalCoreAPI.GetGameTime() : AdvancedStatsAndEffectsTimeManager.Instance.GameTime;
        }

        public void DoRefreshDeltaTime()
        {
            deltaTime = GetGameTime();
        }

        public void ProcessStatsCycle()
        {
            if (!MyAPIGateway.Session.CreativeMode && IsValid)
            {
                if (deltaTime == 0)
                    DoRefreshDeltaTime();
                spendTime += GetGameTime() - deltaTime;
                DoRefreshDeltaTime();
                long cicleType = 1000; /* default cycle time */
                if (spendTime >= cicleType)
                {
                    spendTime -= cicleType;

                    DoAbsorptionCicle();
                    DoEffectCicle();

                    foreach (var key in Stats.Keys)
                    {
                        Stats[key].Value = BodyStats[key].CurrentValue;
                    }
                }
            }
        }

        private void DoEffectCicle()
        {
            foreach (var effect in OverTimeEffects)
            {
                if ((effect.CurrentValue.IsPositive && effect.CurrentValue.Current > 0) ||
                    (!effect.CurrentValue.IsPositive && effect.CurrentValue.Current < 0))
                {
                    if (BodyStats.Keys.Contains(effect.Target))
                    {
                        BodyStats[effect.Target].CurrentValue += effect.CurrentValue.ConsumeRate;
                        effect.CurrentValue.Current -= effect.CurrentValue.ConsumeRate;
                    }
                    else
                    {
                        effect.CurrentValue.Current = 0;
                    }
                }
            }
            OverTimeEffects.RemoveAll(x =>
                (x.CurrentValue.IsPositive && x.CurrentValue.Current <= 0) ||
                (!x.CurrentValue.IsPositive && x.CurrentValue.Current >= 0)
            );
        }

        private void DoAbsorptionCicle()
        {
            foreach (var consumable in OverTimeConsumables)
            {
                foreach (var valueKey in consumable.CurrentValues.Keys)
                {
                    if (BodyStats.Keys.Contains(valueKey))
                    {
                        BodyStats[valueKey].CurrentValue += consumable.CurrentValues[valueKey].ConsumeRate;
                        consumable.CurrentValues[valueKey].Current -= consumable.CurrentValues[valueKey].ConsumeRate;
                    }
                    else
                    {
                        consumable.CurrentValues.Remove(valueKey);
                    }
                }
            }
            OverTimeConsumables.RemoveAll(x => x.FullyConsumed);
        }

        public void SetCharacterStatValue(string name, float value)
        {
            if (BodyStats.Keys.Contains(name))
            {
                BodyStats[name].CurrentValue = value;
            }
        }

        public void ResetCharacterStats()
        {
            foreach (var key in Stats.Keys)
            {
                Stats[key].Value = Stats[key].DefaultValue;
                BodyStats[key].CurrentValue = Stats[key].Value;
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
