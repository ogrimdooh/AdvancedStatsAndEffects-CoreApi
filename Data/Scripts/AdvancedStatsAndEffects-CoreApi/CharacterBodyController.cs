using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private long deltaTime = 0;
        private long spendTime = 0;
        private int updateHash = 0;

        public MyCharacterStatComponent StatComponent { get; private set; }
        protected ConcurrentDictionary<string, MyEntityStat> Stats { get; private set; } = new ConcurrentDictionary<string, MyEntityStat>();
        protected List<string> IgnoreCheckStats { get; private set; } = new List<string>();

        public List<OverTimeConsumable> OverTimeConsumables { get; set; } = new List<OverTimeConsumable>();
        public List<OverTimeEffect> OverTimeEffects { get; set; } = new List<OverTimeEffect>();
        public ConcurrentDictionary<string, long> FixedStatTimer { get; set; } = new ConcurrentDictionary<string, long>();
        public ConcurrentDictionary<string, byte> FixedStatStack { get; set; } = new ConcurrentDictionary<string, byte>();

        public MyInventory Inventory { get; private set; }
        public Guid InventoryObserver { get; private set; }

        public MyEntityStat Health { get { return GetStat(HEALTH_KEY); } }

        protected Dictionary<string, MyEntityStat> statCache = new Dictionary<string, MyEntityStat>();
        public Vector2 lastHealthChanged { get; protected set; } = Vector2.Zero;
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

        public bool IsDead
        {
            get
            {
                return Entity == null || Entity.IsDead || Health == null || Health.Value == 0;
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
            Entity.MovementStateChanged += Character_MovementStateChanged;
            StatComponent = Entity.Components.Get<MyEntityStatComponent>() as MyCharacterStatComponent;
            ClearStatsCache();
            if (StatComponent != null)
                LoadPlayerStat(HEALTH_KEY);
        }

        private void Character_MovementStateChanged(IMyCharacter character, VRage.Game.MyCharacterMovementEnum oldState, VRage.Game.MyCharacterMovementEnum newState)
        {
            if (AdvancedStatsAndEffectsSession.Static.PlayerMovementChange.Any())
                foreach (var playerMovementChange in AdvancedStatsAndEffectsSession.Static.PlayerMovementChange)
                {
                    if (playerMovementChange.Action != null)
                    {
                        playerMovementChange.Action(PlayerId, Entity, StatComponent, oldState, newState);
                    }
                }
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
            {
                Health.OnStatChanged -= Health_OnStatChanged; /* if try to add duplicated, remove first */
                Health.OnStatChanged += Health_OnStatChanged;
            }
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
            if (AdvancedStatsAndEffectsSession.Static.PlayerHealthChanged.Any())
                foreach (var playerHealthChanged in AdvancedStatsAndEffectsSession.Static.PlayerHealthChanged)
                {
                    if (playerHealthChanged.Action != null)
                    {
                        playerHealthChanged.Action(PlayerId, Entity, StatComponent, newValue, oldValue, statChangeData);
                    }
                }
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

        public void DoConsumeItem(ConsumableInfo consumableInfo)
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
            foreach (var afterPlayerConsume in AdvancedStatsAndEffectsSession.Static.AfterPlayerConsume)
            {
                if (afterPlayerConsume.Action != null)
                {
                    afterPlayerConsume.Action(PlayerId, Entity, StatComponent, consumableInfo.DefinitionId);
                }
            }
        }

        public bool HasFixedEffect(string id)
        {
            var fixedStat = AdvancedStatsAndEffectsSession.Static.GetFixedStat(id);
            if (fixedStat != null)
            {
                var statName = $"StatsGroup{fixedStat.Group.ToString("00")}";
                if (Stats.ContainsKey(statName))
                {
                    var targetValue = FixedStatsConstants.GetGroupValues(fixedStat.Group);
                    if (targetValue != null && targetValue.Length > fixedStat.Index && fixedStat.Index >= 0)
                    {
                        var currentValue = (int)Stats[statName].Value;
                        return (currentValue & targetValue[fixedStat.Index]) != 0;
                    }
                }
            }
            return false;
        }

        public void RemoveFixedEffect(string id, byte stacks, bool max)
        {
            var fixedStat = AdvancedStatsAndEffectsSession.Static.GetFixedStat(id);
            if (fixedStat != null)
            {
                var statName = $"StatsGroup{fixedStat.Group.ToString("00")}";
                if (Stats.ContainsKey(statName))
                {
                    var targetValue = FixedStatsConstants.GetGroupValues(fixedStat.Group);
                    if (targetValue != null && targetValue.Length > fixedStat.Index && fixedStat.Index >= 0)
                    {
                        var currentValue = (int)Stats[statName].Value;
                        if ((currentValue & targetValue[fixedStat.Index]) != 0)
                        {
                            var doRemove = true;
                            if (fixedStat.CanStack)
                            {
                                if (!max && FixedStatStack.ContainsKey(id))
                                {
                                    FixedStatStack[id] -= stacks;
                                    doRemove = FixedStatStack[id] <= 0;
                                    if (!doRemove)
                                    {
                                        if (fixedStat.CanSelfRemove)
                                        {
                                            FixedStatTimer[id] = fixedStat.TimeToSelfRemove;
                                        }
                                        else if (fixedStat.IsInverseTime)
                                        {
                                            FixedStatTimer[id] = 0;
                                        }
                                    }
                                }
                                if (doRemove)
                                    FixedStatStack.Remove(id);
                            }
                            if (doRemove)
                            {
                                currentValue &= ~targetValue[fixedStat.Index];
                                Stats[statName].Value = currentValue;
                                if (fixedStat.CanSelfRemove || fixedStat.IsInverseTime)
                                {
                                    if (FixedStatTimer.ContainsKey(id))
                                        FixedStatTimer.Remove(id);
                                }
                            }
                            RefreshUpdateHash();
                        }
                    }
                }
            }
        }

        public void AddFixedEffect(string id, byte stacks, bool max)
        {
            var fixedStat = AdvancedStatsAndEffectsSession.Static.GetFixedStat(id);
            if (fixedStat != null)
            {
                var statName = $"StatsGroup{fixedStat.Group.ToString("00")}";
                if (Stats.ContainsKey(statName))
                {
                    var targetValue = FixedStatsConstants.GetGroupValues(fixedStat.Group);
                    if (targetValue != null && targetValue.Length > fixedStat.Index && fixedStat.Index >= 0)
                    {
                        var currentValue = (int)Stats[statName].Value;
                        if ((currentValue & targetValue[fixedStat.Index]) == 0)
                        {
                            currentValue |= targetValue[fixedStat.Index];
                            Stats[statName].Value = currentValue;
                        }
                        if (fixedStat.CanStack)
                        {
                            if (FixedStatStack.ContainsKey(id))
                                FixedStatStack[id] += max ? fixedStat.MaxStacks : stacks;
                            else
                                FixedStatStack[id] = max ? fixedStat.MaxStacks : stacks;
                            FixedStatStack[id] = Math.Min(FixedStatStack[id], fixedStat.MaxStacks);
                        }
                        if (fixedStat.CanSelfRemove)
                        {
                            FixedStatTimer[id] = fixedStat.TimeToSelfRemove;
                        }
                        else if (fixedStat.IsInverseTime && !FixedStatTimer.ContainsKey(id))
                        {
                            FixedStatTimer[id] = 0;
                        }
                        RefreshUpdateHash();
                    }
                }
            }
        }


        private void DoProcessFixedEffects(List<FixedEffectInConsumableInfo> effects)
        {
            if (effects != null && effects.Any())
            {
                foreach (var effect in effects)
                {
                    var chance = effect.Chance * 100;
                    switch (effect.Type)
                    {
                        case FixedEffectInConsumableType.Add:
                            AddFixedEffect(effect.Target, effect.Stacks, effect.MaxStacks);
                            break;
                        case FixedEffectInConsumableType.ChanceAdd:
                            if (chance >= 100 || AdvancedStatsAndEffectsSession.CheckChance(chance))
                            {
                                AddFixedEffect(effect.Target, effect.Stacks, effect.MaxStacks);
                            }
                            break;
                        case FixedEffectInConsumableType.Remove:
                            RemoveFixedEffect(effect.Target, effect.Stacks, effect.MaxStacks);
                            break;
                        case FixedEffectInConsumableType.ChanceRemove:
                            if (chance >= 100 || AdvancedStatsAndEffectsSession.CheckChance(chance))
                            {
                                RemoveFixedEffect(effect.Target, effect.Stacks, effect.MaxStacks);
                            }
                            break;
                    }
                }
            }
        }

        private void DoInstantEffect(string target, float value)
        {
            if (Stats.ContainsKey(target))
                Stats[target].Value += value;
        }

        private void DoProcessOverTimeEffects(UniqueEntityId id, List<OverTimeEffectInfo> effects)
        {
            if (effects != null && effects.Any())
            {
                foreach (var effect in effects)
                {
                    switch (effect.Type)
                    {
                        case OverTimeEffectType.Instant:
                            DoInstantEffect(effect.Target, effect.Amount);
                            break;
                        case OverTimeEffectType.OverTime:
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

        private long GetGameTime()
        {
            return ExtendedSurvivalCoreAPI.Registered ? ExtendedSurvivalCoreAPI.GetGameTime() : AdvancedStatsAndEffectsTimeManager.Instance.GameTime;
        }

        public void DoRefreshDeltaTime()
        {
            deltaTime = GetGameTime();
        }

        private readonly long cicleType = 1000; /* default cycle time */
        public void ProcessStatsCycle()
        {
            if (!MyAPIGateway.Session.CreativeMode && IsValid)
            {
                if (deltaTime == 0)
                    DoRefreshDeltaTime();
                var updateTime = GetGameTime() - deltaTime;
                spendTime += updateTime;

                /* Before Cycle */
                foreach (var stat in AdvancedStatsAndEffectsSession.Static.StatBeforeCycle.Keys)
                {
                    var targetStat = GetStat(stat);
                    if (targetStat != null)
                    {
                        foreach (var statCycle in AdvancedStatsAndEffectsSession.Static.StatBeforeCycle[stat])
                        {
                            if (statCycle.Action != null)
                                statCycle.Action(PlayerId, spendTime, updateTime, Entity, StatComponent, targetStat);
                        }
                    }
                }

                DoRefreshDeltaTime();
                if (spendTime >= cicleType)
                {
                    spendTime -= cicleType;

                    /* Start Cycle */
                    bool canExecute = true;
                    if (AdvancedStatsAndEffectsSession.Static.BeforeCycle.Any())
                        foreach (var beforeUpdate in AdvancedStatsAndEffectsSession.Static.BeforeCycle)
                        {
                            if (beforeUpdate.Action != null && !beforeUpdate.Action(PlayerId, Entity, StatComponent))
                            {
                                canExecute = false;
                                break;
                            }
                        }

                    if (canExecute)
                    {

                        foreach (var stat in AdvancedStatsAndEffectsSession.Static.StartStatCycle.Keys)
                        {
                            var targetStat = GetStat(stat);
                            if (targetStat != null)
                            {
                                foreach (var statCycle in AdvancedStatsAndEffectsSession.Static.StartStatCycle[stat])
                                {
                                    if (statCycle.Action != null)
                                        statCycle.Action(PlayerId, Entity, StatComponent, targetStat);
                                }
                            }
                        }

                        DoAbsorptionCicle();
                        DoEffectCicle();
                        DoFixedStatCycle();

                        /* End Cycle */
                        if (AdvancedStatsAndEffectsSession.Static.AfterCycle.Any())
                            foreach (var afterUpdate in AdvancedStatsAndEffectsSession.Static.AfterCycle)
                            {
                                if (afterUpdate.Action != null)
                                    afterUpdate.Action(PlayerId, Entity, StatComponent);
                            }
                        foreach (var stat in AdvancedStatsAndEffectsSession.Static.EndStatCycle.Keys)
                        {
                            var targetStat = GetStat(stat);
                            if (targetStat != null)
                            {
                                foreach (var statCycle in AdvancedStatsAndEffectsSession.Static.EndStatCycle[stat])
                                {
                                    if (statCycle.Action != null)
                                        statCycle.Action(PlayerId, Entity, StatComponent, targetStat);
                                }
                            }
                        }

                        DoSendDataToClient();

                    }

                }
            }
        }

        private PlayerClientUpdateData lastSendData = null;
        private void DoSendDataToClient()
        {
            try
            {
                if (MyAPIGateway.Utilities.IsDedicated || MyAPIGateway.Session.Player.IdentityId != PlayerId)
                {
                    if (lastSendData == null || lastSendData.HashCode != updateHash)
                    {
                        lastSendData = GetPlayerSendData();
                        if (lastSendData != null && Player != null)
                        {
                            string message = MyAPIGateway.Utilities.SerializeToXML<PlayerClientUpdateData>(lastSendData);
                            MyAPIGateway.Multiplayer.SendMessageTo(
                                AdvancedStatsAndEffectsSession.NETWORK_ID_STATSSYSTEM,
                                Encoding.Unicode.GetBytes(message),
                                Player.SteamUserId
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        public int GetPlayerFixedStatUpdateHash()
        {
            return updateHash;
        }

        private void RefreshUpdateHash()
        {
            updateHash = new Random().Next(0, int.MaxValue);
        }

        private PlayerClientUpdateData GetPlayerSendData()
        {
            return new PlayerClientUpdateData()
            {
                HashCode = updateHash,
                FixedStatsStacks = FixedStatStack.Select(x => new PlayerStatValueData() { Target = x.Key, Value = x.Value }).ToList(),
                FixedStatsTimers = FixedStatTimer.Select(x => new PlayerStatValueData() { Target = x.Key, Value = x.Value }).ToList()
            };
        }

        private void DoFixedStatCycle()
        {
            var keys = FixedStatTimer.Keys.ToArray();
            foreach (var fixedStat in keys)
            {
                var fixedStatData = AdvancedStatsAndEffectsSession.Static.GetFixedStat(fixedStat);
                if (fixedStatData != null)
                {
                    if (FixedStatTimer.ContainsKey(fixedStat))
                    {
                        if (fixedStatData.CanSelfRemove)
                        {
                            FixedStatTimer[fixedStat] -= cicleType;
                            if (FixedStatTimer[fixedStat] <= 0)
                            {
                                RemoveFixedEffect(fixedStat, Math.Max(fixedStatData.StacksWhenRemove, (byte)1), fixedStatData.CompleteRemove);
                            }
                            else
                            {
                                RefreshUpdateHash();
                            }
                        }
                        else if (fixedStatData.IsInverseTime)
                        {
                            FixedStatTimer[fixedStat] += cicleType;
                            if (FixedStatTimer[fixedStat] > fixedStatData.MaxInverseTime)
                                FixedStatTimer[fixedStat] = fixedStatData.MaxInverseTime;
                            if (FixedStatTimer[fixedStat] == fixedStatData.MaxInverseTime && fixedStatData.SelfRemoveWhenMaxInverse)
                            {
                                RemoveFixedEffect(fixedStat, Math.Max(fixedStatData.StacksWhenRemove, (byte)1), fixedStatData.CompleteRemove);
                            }
                            else
                            {
                                RefreshUpdateHash();
                            }
                        }
                    }
                }
                else
                {
                    FixedStatTimer.Remove(fixedStat);
                    if (FixedStatStack.ContainsKey(fixedStat))
                    {
                        FixedStatStack.Remove(fixedStat);
                    }
                }
            }
            foreach (var fixedStat in AdvancedStatsAndEffectsSession.Static.FixedStatCycle.Keys)
            {
                if (HasFixedEffect(fixedStat))
                {
                    foreach (var fixedStatAction in AdvancedStatsAndEffectsSession.Static.FixedStatCycle[fixedStat])
                    {
                        if (fixedStatAction.Action != null)
                        {
                            var statck = FixedStatStack.ContainsKey(fixedStat) ? FixedStatStack[fixedStat] : (byte)0;
                            var timer = FixedStatTimer.ContainsKey(fixedStat) ? FixedStatTimer[fixedStat] : 0;
                            fixedStatAction.Action(fixedStat, statck, timer, PlayerId, Entity, StatComponent);
                        }
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
                    if (Stats.Keys.Contains(effect.Target))
                    {
                        Stats[effect.Target].Value += effect.CurrentValue.ConsumeRate;
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
                    if (Stats.Keys.Contains(valueKey))
                    {
                        Stats[valueKey].Value += consumable.CurrentValues[valueKey].ConsumeRate;
                        consumable.CurrentValues[valueKey].Current -= consumable.CurrentValues[valueKey].ConsumeRate;
                    }
                    else
                    {
                        if (AdvancedStatsAndEffectsSession.Static.IsVirtualStat(valueKey))
                        {
                            var virtualStat = AdvancedStatsAndEffectsSession.Static.GetVirtualStat(valueKey);
                            consumable.CurrentValues[valueKey].Current -= consumable.CurrentValues[valueKey].ConsumeRate;
                            var consumeRate = consumable.CurrentValues[valueKey].ConsumeRate;
                            if (Stats.Keys.Contains(virtualStat.Target))
                            {
                                var maxRate = Stats[virtualStat.Target].MaxValue - Stats[virtualStat.Target].Value;
                                Stats[virtualStat.Target].Value += consumable.CurrentValues[valueKey].ConsumeRate;
                                if (maxRate < consumeRate)
                                    consumeRate -= maxRate;
                                else
                                    consumeRate = 0;
                            }
                            if (AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle.ContainsKey(valueKey))
                                foreach (var virtualStatAbsorptionCicle in AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle[valueKey])
                                {
                                    if (virtualStatAbsorptionCicle.Action != null)
                                        virtualStatAbsorptionCicle.Action(
                                            valueKey,
                                            consumeRate,
                                            consumable.Id.DefinitionId,
                                            PlayerId,
                                            Entity,
                                            StatComponent
                                        );
                                }
                        }
                        else
                        {
                            consumable.CurrentValues.Remove(valueKey);
                        }
                    }
                }
            }
            OverTimeConsumables.RemoveAll(x => x.FullyConsumed);
        }

        public float GetRemainOverTimeConsumable(string stat)
        {
            return OverTimeConsumables.Where(x => x.CurrentValues.ContainsKey(stat)).Sum(x => x.CurrentValues[stat].Current);
        }

        public void DoEmptyConsumables()
        {
            OverTimeConsumables.Clear();
            OverTimeEffects.Clear();
        }

        public void SetCharacterStatValue(string name, float value)
        {
            if (Stats.Keys.Contains(name))
            {
                Stats[name].Value = value;
            }
        }

        public void ResetCharacterStats()
        {
            foreach (var key in Stats.Keys)
            {
                Stats[key].Value = Stats[key].DefaultValue;
            }
            DoEmptyConsumables();
            FixedStatStack.Clear();
            FixedStatTimer.Clear();
            foreach (var playerReset in AdvancedStatsAndEffectsSession.Static.PlayerReset)
            {
                if (playerReset.Action != null)
                {
                    playerReset.Action(PlayerId, Entity, StatComponent);
                }
            }
        }

    }

}
