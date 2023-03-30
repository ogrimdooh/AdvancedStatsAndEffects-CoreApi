using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AdvancedStatsAndEffects
{
    public abstract class CharacterBodyController : EntityBase<IMyCharacter>
    {

        public const string HEALTH_KEY = "Health";

        public MyCharacterStatComponent StatComponent { get; private set; }
        protected ConcurrentDictionary<string, MyEntityStat> Stats { get; private set; } = new ConcurrentDictionary<string, MyEntityStat>();
        protected List<string> IgnoreCheckStats { get; private set; } = new List<string>();

        public ConcurrentDictionary<string, BodyStatData> BodyStats { get; set; } = new ConcurrentDictionary<string, BodyStatData>();
        public List<OverTimeConsumable> OverTimeConsumables { get; set; } = new List<OverTimeConsumable>();
        public List<OverTimeEffect> OverTimeEffects { get; set; } = new List<OverTimeEffect>();

        public MyInventory Inventory { get; private set; }
        public Guid InventoryObserver { get; private set; }

        public MyEntityStat Health { get { return GetStat(HEALTH_KEY); } }

        protected Dictionary<string, MyEntityStat> statCache = new Dictionary<string, MyEntityStat>();
        protected Vector2 lastHealthChanged = Vector2.Zero;
        protected KeyValuePair<MyPhysicalInventoryItem, MyFixedPoint>? lastRemovedIten = null;
        protected bool hasDied = false;

        private long _playerId = -1;
        public long PlayerId
        {
            get
            {
                if (_playerId <= 0)
                    _playerId = Entity.GetPlayerId();
                return _playerId;
            }
        }

        public IMyPlayer Player
        {
            get
            {
                return Entity.GetPlayer();
            }
        }

        public bool IsValid
        {
            get
            {
                return GetIsValid();
            }
        }

        public CharacterBodyController(IMyCharacter entity)
            : base(entity)
        {
            ConfigureCharacter(entity);
        }

        public void ConfigureCharacter(IMyCharacter entity)
        {
            try
            {
                if (Entity != null && (Entity != entity || !IsValid))
                    ResetConfiguration();
                if (Entity == null || !IsValid)
                {
                    Entity = entity;
                    OnBeginConfigureCharacter();
                    OnEndConfigureCharacter();
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), $"ConfigureCharacter [Error]");
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        protected virtual void OnBeginConfigureCharacter()
        {
            Entity.CharacterDied += Character_CharacterDied;
            StatComponent = Entity.Components.Get<MyEntityStatComponent>() as MyCharacterStatComponent;
            ClearStatsCache();
            if (StatComponent != null)
                LoadPlayerStat(HEALTH_KEY);
        }

        private Guid DoCreateNewObserver()
        {
            return ExtendedSurvivalCoreAPI.AddInventoryObserver(Entity, 0);
        }

        protected virtual Guid CreateNewObserver()
        {
            if (ExtendedSurvivalCoreAPI.Registered)
                return DoCreateNewObserver();
            else
                AdvancedStatsAndEffectsSession.AddToInvokeAfterCoreApiLoaded(() => {
                    var id = DoCreateNewObserver();
                    if (id != Guid.Empty)
                        InventoryObserver = id;
                });
            return Guid.Empty;
        }

        protected virtual void OnEndConfigureCharacter()
        {
            if (StatComponent != null)
                Health.OnStatChanged += Health_OnStatChanged;
            Inventory = Entity.GetInventory() as MyInventory;
            if (Inventory != null)
            {
                InventoryObserver = CreateNewObserver();
                Inventory.ContentsRemoved += Inventory_ContentsRemoved;
            }
            hasDied = false;
        }

        protected void ResetConfiguration()
        {
            OnBeginResetConfiguration();
            OnEndResetConfiguration();
        }

        protected virtual void OnBeginResetConfiguration()
        {
            if (Health != null)
                Health.OnStatChanged -= Health_OnStatChanged;
            if (Inventory != null)
                Inventory.ContentsRemoved -= Inventory_ContentsRemoved;
            if (InventoryObserver != Guid.Empty)
            {
                if (ExtendedSurvivalCoreAPI.Registered)
                    ExtendedSurvivalCoreAPI.DisposeInventoryObserver(InventoryObserver);
            }
        }

        protected virtual void OnCharacterDied()
        {
            Health.OnStatChanged -= Health_OnStatChanged;
        }

        protected DateTime lastTimeDead;
        protected void Character_CharacterDied(IMyCharacter obj)
        {
            lastTimeDead = DateTime.Now;
            hasDied = true;
            OnCharacterDied();
        }

        protected virtual void OnHealthChanged(float newValue, float oldValue, object statChangeData)
        {

        }

        protected void Health_OnStatChanged(float newValue, float oldValue, object statChangeData)
        {
            lastHealthChanged = new Vector2(newValue, oldValue);
            OnHealthChanged(newValue, oldValue, statChangeData);
        }

        protected virtual void OnInventoryContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint ammount)
        {

        }

        protected void Inventory_ContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint ammount)
        {
            lastRemovedIten = new KeyValuePair<MyPhysicalInventoryItem, MyFixedPoint>(item, ammount);
            OnInventoryContentsRemoved(item, ammount);
        }

        protected virtual void OnEndResetConfiguration()
        {
            Entity = null;
            StatComponent = null;
            Inventory = null;
            Inventory = null;
            InventoryObserver = Guid.Empty;
            ClearLoadedStats();
        }

        public MyEntityStat GetStat(string stat)
        {
            if (Stats.ContainsKey(stat))
                return Stats[stat];
            return null;
        }

        protected virtual void LoadPlayerStat(string statKey)
        {
            Stats[statKey] = GetPlayerStat(statKey);
            BodyStats[statKey] = new BodyStatData()
            {
                CurrentValue = Stats[statKey].Value
            };
        }

        protected MyEntityStat GetPlayerStat(string statName)
        {
            var statKey = statName.ToLower();
            if (statCache.ContainsKey(statKey))
                return statCache[statKey];
            MyEntityStat stat;
            StatComponent.TryGetStat(MyStringHash.GetOrCompute(statName), out stat);
            if (stat != null)
            {
                statCache.Add(statKey, stat);
            }
            return stat;
        }

        protected virtual void ClearLoadedStats()
        {
            foreach (var key in Stats.Keys)
            {
                Stats[key] = null;
                BodyStats[key] = null;
            }
        }

        protected void ClearStatsCache()
        {
            statCache.Clear();
        }

        protected virtual bool GetIsValid()
        {
            return Entity != null && Stats.Any() && !Stats.Any(x => !IgnoreCheckStats.Contains(x.Key) && x.Value == null);
        }

        public void DoConsumeItem(AdvancedStatsAndEffectsAPIBackend.ConsumableInfo consumableInfo)
        {
            var uniqueId = new UniqueEntityId(consumableInfo.DefinitionId);
            if (OverTimeConsumables.Any(x => x.Id == uniqueId))
            {
                var consumableInDigestion = OverTimeConsumables.FirstOrDefault(x => x.Id == uniqueId);
                consumableInfo.AddToOverTimeConsumable(consumableInDigestion);
            }
            else
            {
                OverTimeConsumables.Add(consumableInfo.GetAsOverTimeConsumable());
            }
            DoProcessOverTimeEffects(uniqueId, consumableInfo.OverTimeEffects);
            DoProcessFixedEffects(consumableInfo.FixedEffects);
        }

        private void RemoveFixedEffect(string id)
        {
            var fixedStat = AdvancedStatsAndEffectsSession.Static.GetFixedStat(id);
            if (fixedStat != null)
            {
                var statName = $"StatsGroup{fixedStat.Group.ToString("00")}";
                if (BodyStats.ContainsKey(statName))
                {
                    var targetValue = FixedStatsConstants.GetGroupValues(fixedStat.Group);
                    if (targetValue != null && targetValue.Length > fixedStat.Index && fixedStat.Index >= 0)
                    {
                        var currentValue = (int)BodyStats[statName].CurrentValue;
                        currentValue &= ~targetValue[fixedStat.Index];
                        BodyStats[statName].CurrentValue = currentValue;
                    }
                }
            }
        }

        private void AddFixedEffect(string id)
        {
            var fixedStat = AdvancedStatsAndEffectsSession.Static.GetFixedStat(id);
            if (fixedStat != null)
            {
                var statName = $"StatsGroup{fixedStat.Group.ToString("00")}";
                if (BodyStats.ContainsKey(statName))
                {
                    var targetValue = FixedStatsConstants.GetGroupValues(fixedStat.Group);
                    if (targetValue != null && targetValue.Length > fixedStat.Index && fixedStat.Index >= 0)
                    {
                        var currentValue = (int)BodyStats[statName].CurrentValue;
                        currentValue |= targetValue[fixedStat.Index];
                        BodyStats[statName].CurrentValue = currentValue;
                    }
                }
            }
        }


        private void DoProcessFixedEffects(List<AdvancedStatsAndEffectsAPIBackend.FixedEffectInConsumableInfo> effects)
        {
            if (effects != null && effects.Any())
            {
                foreach (var effect in effects)
                {
                    var chance = effect.Chance * 100;
                    switch (effect.Type)
                    {
                        case AdvancedStatsAndEffectsAPIBackend.FixedEffectInConsumableType.Add:
                            AddFixedEffect(effect.Target);
                            break;
                        case AdvancedStatsAndEffectsAPIBackend.FixedEffectInConsumableType.ChanceAdd:
                            if (chance >= 100 || AdvancedStatsAndEffectsSession.CheckChance(chance))
                            {
                                AddFixedEffect(effect.Target);
                            }
                            break;
                        case AdvancedStatsAndEffectsAPIBackend.FixedEffectInConsumableType.Remove:
                            RemoveFixedEffect(effect.Target);
                            break;
                        case AdvancedStatsAndEffectsAPIBackend.FixedEffectInConsumableType.ChanceRemove:
                            if (chance >= 100 || AdvancedStatsAndEffectsSession.CheckChance(chance))
                            {
                                RemoveFixedEffect(effect.Target);
                            }
                            break;
                    }
                }
            }
        }

        private void DoInstantEffect(string target, float value)
        {
            if (BodyStats.ContainsKey(target))
                BodyStats[target].CurrentValue += value;
        }

        private void DoProcessOverTimeEffects(UniqueEntityId id, List<AdvancedStatsAndEffectsAPIBackend.OverTimeEffectInfo> effects)
        {
            if (effects != null && effects.Any())
            {
                foreach (var effect in effects)
                {
                    switch (effect.Type)
                    {
                        case AdvancedStatsAndEffectsAPIBackend.OverTimeEffectType.Instant:
                            DoInstantEffect(effect.Target, effect.Amount);
                            break;
                        case AdvancedStatsAndEffectsAPIBackend.OverTimeEffectType.OverTime:
                            if (OverTimeEffects.Any(x => x.Id == id && x.Target == effect.Target))
                            {
                                var effectInDigestion = OverTimeEffects.FirstOrDefault(x => x.Id == id && x.Target == effect.Target);
                                effectInDigestion.CurrentValue.AddAmmount(effect.Amount);
                            }
                            else
                            {
                                OverTimeEffects.Add(new OverTimeEffect()
                                {
                                    Id = id,
                                    Target = effect.Target,
                                    CurrentValue = new OverTimeProperty(effect.Amount, effect.Amount / Math.Max(1, effect.TimeToEffect))
                                });
                            }
                            break;
                    }
                }
            }
        }

    }

    public static class FixedStatsConstants
    {

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

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
            Flag31 = 1 << 31,
            Flag32 = 1 << 32

        }

        public static int[] GetGroupValues(int group)
        {
            var type = GetGroupType(group);
            if (type != null)
            {
                return (int[])Enum.GetValues(type);
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
