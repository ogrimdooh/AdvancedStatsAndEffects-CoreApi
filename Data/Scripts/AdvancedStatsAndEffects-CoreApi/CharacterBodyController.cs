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

    }

}
