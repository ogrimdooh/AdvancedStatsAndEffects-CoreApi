using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace AdvancedStatsAndEffects
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
    public class PlayerStatValueData
    {

        [ProtoMember(1)]
        public string Target { get; set; }

        [ProtoMember(2)]
        public float Value { get; set; }

    }

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class PlayerStatGenericData
    {

        [ProtoMember(1)]
        public string Target { get; set; }

        [ProtoMember(2)]
        public string Value { get; set; }

    }

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class PlayerClientUpdateData
    {

        [ProtoMember(1)]
        public int HashCode { get; set; }
        [ProtoMember(2)]
        public List<PlayerStatValueData> FixedStatsStacks { get; set; } = new List<PlayerStatValueData>();
        [ProtoMember(3)]
        public List<PlayerStatValueData> FixedStatsTimers { get; set; } = new List<PlayerStatValueData>();
        [ProtoMember(4)]
        public List<PlayerStatGenericData> CustomData { get; set; } = new List<PlayerStatGenericData>();

        public override int GetHashCode()
        {
            return HashCode;
        }

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

        [ProtoMember(3)]
        public bool MaxStacks { get; set; }

        [ProtoMember(4)]
        public byte Stacks { get; set; }

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

        [ProtoMember(5)]
        public bool CanStack { get; set; }

        [ProtoMember(6)]
        public byte MaxStacks { get; set; }

        [ProtoMember(7)]
        public bool CanSelfRemove { get; set; }

        [ProtoMember(8)]
        public int TimeToSelfRemove { get; set; }

        [ProtoMember(9)]
        public bool CompleteRemove { get; set; }

        [ProtoMember(10)]
        public byte StacksWhenRemove { get; set; }

    }

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class VirtualStatInfo
    {

        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string Target { get; set; }

    }

    public class AdvancedStatsAndEffectsClientAPI
    {

        private static AdvancedStatsAndEffectsClientAPI instance;

        public static string ModName = "";
        public const ushort ModHandlerID = 35876;
        public const int ModAPIVersion = 1;

        public static bool Registered { get; private set; } = false;

        private static Dictionary<string, Delegate> ModAPIMethods;

        private static Func<int, string, bool> _VerifyVersion;
        private static Func<string, byte> _GetPlayerFixedStatStack;
        private static Func<string, long> _GetPlayerFixedStatRemainTime;
        private static Func<int> _GetPlayerFixedStatUpdateHash;

        /// <summary>
        /// Returns the hash of a player data to use in HUD updates
        /// </summary>
        public static int GetPlayerFixedStatUpdateHash()
        {
            return _GetPlayerFixedStatUpdateHash?.Invoke() ?? 0;
        }

        /// <summary>
        /// Returns true if the version is compatibile with the API Backend, this is automatically called
        /// </summary>
        public static bool VerifyVersion(int Version, string ModName)
        {
            return _VerifyVersion?.Invoke(Version, ModName) ?? false;
        }

        /// <summary>
        /// Get the stack of a fixed stat in the player
        /// </summary>
        public static byte GetPlayerFixedStatStack(string fixedStat)
        {
            return _GetPlayerFixedStatStack?.Invoke(fixedStat) ?? 0;
        }

        /// <summary>
        /// Get the remain timer of a fixed stat in the player
        /// </summary>
        public static long GetPlayerFixedStatRemainTime(string fixedStat)
        {
            return _GetPlayerFixedStatRemainTime?.Invoke(fixedStat) ?? 0;
        }

        /// <summary>
        /// Unregisters the mod
        /// </summary>
        public void Unregister()
        {
            if (instance != null)
            {
                instance.DoUnregister();
            }
        }

        /// <summary>
        /// Unregisters the mod
        /// </summary>
        public void DoUnregister()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(ModHandlerID, ModHandler);
            Registered = false;
            instance = null;
            m_onRegisteredAction = null;
        }

        private Action m_onRegisteredAction;

        /// <summary>
        /// Create a Advanced Stats And Effects API Instance. Please only create one per mod. 
        /// </summary>
        /// <param name="onRegisteredAction">Callback once the Advanced Stats And Effects API is active. You can Instantiate Advanced Stats And Effects API objects in this Action</param>
        public AdvancedStatsAndEffectsClientAPI(Action onRegisteredAction = null)
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            m_onRegisteredAction = onRegisteredAction;
            MyAPIGateway.Utilities.RegisterMessageHandler(ModHandlerID, ModHandler);
            if (ModName == "")
            {
                if (MyAPIGateway.Utilities.GamePaths.ModScopeName.Contains("_"))
                    ModName = MyAPIGateway.Utilities.GamePaths.ModScopeName.Split('_')[1];
                else
                    ModName = MyAPIGateway.Utilities.GamePaths.ModScopeName;
            }
        }

        private void ModHandler(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj is Dictionary<string, Delegate>)
            {
                ModAPIMethods = (Dictionary<string, Delegate>)obj;
                _VerifyVersion = (Func<int, string, bool>)ModAPIMethods["VerifyVersion"];

                Registered = VerifyVersion(ModAPIVersion, ModName);

                MyLog.Default.WriteLine("Registering Advanced Stats And Effects Client API for Mod '" + ModName + "'");

                if (Registered)
                {
                    try
                    {
                        _GetPlayerFixedStatStack = (Func<string, byte>)ModAPIMethods["GetPlayerFixedStatStack"];
                        _GetPlayerFixedStatRemainTime = (Func<string, long>)ModAPIMethods["GetPlayerFixedStatRemainTime"];
                        _GetPlayerFixedStatUpdateHash = (Func<int>)ModAPIMethods["GetPlayerFixedStatUpdateHash"];

                        if (m_onRegisteredAction != null)
                            m_onRegisteredAction();
                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("Advanced Stats And Effects", "Mod '" + ModName + "' encountered an error when registering the Advanced Stats And Effects Client API, see log for more info.");
                        MyLog.Default.WriteLine("Advanced Stats And Effects: " + e);
                    }
                }
            }
        }

    }

    public class AdvancedStatsAndEffectsAPI
    {

        private static AdvancedStatsAndEffectsAPI instance;

        public static string ModName = "";
        public const ushort ModHandlerID = 35875;
        public const int ModAPIVersion = 1;

        public static bool Registered { get; private set; } = false;

        private static Dictionary<string, Delegate> ModAPIMethods;

        private static Func<int, string, bool> _VerifyVersion;
        private static Func<long> _GetGameTime;
        private static Func<string, bool> _SetStatAsConsumableTrigger;
        private static Func<string, bool> _ConfigureConsumable;
        private static Func<string, bool> _ConfigureFixedStat;
        private static Func<string, bool> _ConfigureVirtualStat;
        private static Func<string, Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool> _AddBeforeCycleStatCallback;
        private static Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool> _AddStartStatCycleCallback;
        private static Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool> _AddEndStatCycleCallback;
        private static Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool> _AddBeforePlayersUpdateCallback;
        private static Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool> _AddAfterPlayersUpdateCallback;
        private static Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool> _AddBeforeCycleCallback;
        private static Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool> _AddAfterCycleCallback;
        private static Func<string, Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent>, int, bool> _AddFixedStatCycleCallback;
        private static Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool> _AddAfterPlayerReset;
        private static Func<Action<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool> _AddAfterPlayerRespawn;
        private static Func<string, Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent>, int, bool> _AddVirtualStatAbsorptionCicle;
        private static Func<Action<long, IMyCharacter>, int, bool> _AddAfterBotAdd;
        private static Func<long, string, byte, bool, bool> _AddFixedEffect;
        private static Func<long, string, byte, bool, bool> _RemoveFixedEffect;
        private static Func<long, bool> _ClearOverTimeConsumable;
        private static Func<long, string, float> _GetRemainOverTimeConsumable;
        private static Func<long, Vector2> _GetLastHealthChange;
        private static Func<long, MyDefinitionId, bool> _DoPlayerConsume;
        private static Func<long, string, byte> _GetPlayerFixedStatStack;
        private static Func<long, string, long> _GetPlayerFixedStatRemainTime;
        private static Func<long, int> _GetPlayerFixedStatUpdateHash;

        /// <summary>
        /// Returns true if the version is compatibile with the API Backend, this is automatically called
        /// </summary>
        public static bool VerifyVersion(int Version, string ModName)
        {
            return _VerifyVersion?.Invoke(Version, ModName) ?? false;
        }

        /// <summary>
        /// Returns the hash of a player data to use in HUD updates
        /// </summary>
        public static int GetPlayerFixedStatUpdateHash(long playerId)
        {
            return _GetPlayerFixedStatUpdateHash?.Invoke(playerId) ?? 0;
        }

        /// <summary>
        /// Get the stack of a fixed stat in the player
        /// </summary>
        public static byte GetPlayerFixedStatStack(long playerId, string fixedStat)
        {
            return _GetPlayerFixedStatStack?.Invoke(playerId, fixedStat) ?? 0;
        }

        /// <summary>
        /// Get the remain timer of a fixed stat in the player
        /// </summary>
        public static long GetPlayerFixedStatRemainTime(long playerId, string fixedStat)
        {
            return _GetPlayerFixedStatRemainTime?.Invoke(playerId, fixedStat) ?? 0;
        }

        /// <summary>
        /// return the game time in MS (only when not paused)
        /// </summary>
        public static long GetGameTime()
        {
            var value = _GetGameTime?.Invoke();
            return value.HasValue ? value.Value : 0;
        }

        /// <summary>
        /// Make player to consume a target consumable
        /// </summary>
        public static bool DoPlayerConsume(long playerId, MyDefinitionId consumableId)
        {
            return _DoPlayerConsume?.Invoke(playerId, consumableId) ?? false;
        }

        /// <summary>
        /// Add a callback after player reset status
        /// </summary>
        public static bool AddAfterPlayerReset(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            return _AddAfterPlayerReset?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback after player respawn
        /// </summary>
        public static bool AddAfterPlayerRespawn(Action<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            return _AddAfterPlayerRespawn?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback after bot added
        /// </summary>
        public static bool AddAfterBotAdd(Action<long, IMyCharacter> callback, int priority)
        {
            return _AddAfterBotAdd?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback to the virtual stat absorption cicle
        /// </summary>
        public static bool AddVirtualStatAbsorptionCicle(string targetStat, Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            return _AddVirtualStatAbsorptionCicle?.Invoke(targetStat, callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback on before player update to a specific stat
        /// </summary>
        public static bool AddBeforeCycleStatCallback(string targetStat, Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            return _AddBeforeCycleStatCallback?.Invoke(targetStat, callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback on before player cycle to a specific stat
        /// </summary>
        public static bool AddStartStatCycleCallback(string targetStat, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            return _AddStartStatCycleCallback?.Invoke(targetStat, callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback on after player cycle to a specific stat
        /// </summary>
        public static bool AddEndStatCycleCallback(string targetStat, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            return _AddEndStatCycleCallback?.Invoke(targetStat, callback, priority) ?? false;
        }

        /// <summary>
        /// Add a fixed effect from a player
        /// </summary>
        public static bool AddFixedEffect(long playerId, string fixedEffectId, byte stacks, bool max)
        {
            return _AddFixedEffect?.Invoke(playerId, fixedEffectId, stacks, max) ?? false;
        }

        /// <summary>
        /// Clear all over time consumable from the player
        /// </summary>
        public static bool ClearOverTimeConsumable(long playerId)
        {
            return _ClearOverTimeConsumable?.Invoke(playerId) ?? false;
        }

        /// <summary>
        /// Get the remain amount of a stat or virtual stat
        /// </summary>
        public static float GetRemainOverTimeConsumable(long playerId, string stat)
        {
            return _GetRemainOverTimeConsumable?.Invoke(playerId, stat) ?? 0;
        }

        /// <summary>
        /// Get the remain amount of a stat or virtual stat
        /// </summary>
        public static Vector2 GetLastHealthChange(long playerId)
        {
            return _GetLastHealthChange?.Invoke(playerId) ?? Vector2.Zero;
        }

        /// <summary>
        /// Remove a fixed effect from a player
        /// </summary>
        public static bool RemoveFixedEffect(long playerId, string fixedEffectId, byte stacks, bool max)
        {
            return _RemoveFixedEffect?.Invoke(playerId, fixedEffectId, stacks, max) ?? false;
        }

        /// <summary>
        /// Add a callback before players update, if the callback return 'false' will stop update
        /// </summary>
        public static bool AddBeforePlayersUpdateCallback(Func<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            return _AddBeforePlayersUpdateCallback?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback after players update
        /// </summary>
        public static bool AddAfterPlayersUpdateCallback(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            return _AddAfterPlayersUpdateCallback?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback before cycle, if the callback return 'false' will stop the cycle
        /// </summary>
        public static bool AddBeforeCycleCallback(Func<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            return _AddBeforeCycleCallback?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback after cycle
        /// </summary>
        public static bool AddAfterCycleCallback(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            return _AddAfterCycleCallback?.Invoke(callback, priority) ?? false;
        }

        /// <summary>
        /// Add a callback after cycle
        /// </summary>
        public static bool AddFixedStatCycleCallback(string fixedStat, Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            return _AddFixedStatCycleCallback?.Invoke(fixedStat, callback, priority) ?? false;
        }

        /// <summary>
        /// Set a stat to be a consumable trigger to the system
        /// </summary>
        public static bool SetStatAsConsumableTrigger(string statToBind)
        {
            return _SetStatAsConsumableTrigger?.Invoke(statToBind) ?? false;
        }

        /// <summary>
        /// Configure a consumable to be used by the framework
        /// </summary>
        public static bool ConfigureConsumable(ConsumableInfo value)
        {
            string messageToSend = MyAPIGateway.Utilities.SerializeToXML<ConsumableInfo>(value);
            return _ConfigureConsumable?.Invoke(messageToSend) ?? false;
        }

        /// <summary>
        /// Configure a fixed stat to be used by the framework
        /// </summary>
        public static bool ConfigureFixedStat(FixedStatInfo value)
        {
            string messageToSend = MyAPIGateway.Utilities.SerializeToXML<FixedStatInfo>(value);
            return _ConfigureFixedStat?.Invoke(messageToSend) ?? false;
        }

        /// <summary>
        /// Configure a fixed stat to be used by the framework
        /// </summary>
        public static bool ConfigureVirtualStat(VirtualStatInfo value)
        {
            string messageToSend = MyAPIGateway.Utilities.SerializeToXML<VirtualStatInfo>(value);
            return _ConfigureVirtualStat?.Invoke(messageToSend) ?? false;
        }

        /// <summary>
        /// Unregisters the mod
        /// </summary>
        public void Unregister()
        {
            if (instance != null)
            {
                instance.DoUnregister();
            }
        }

        /// <summary>
        /// Unregisters the mod
        /// </summary>
        public void DoUnregister()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(ModHandlerID, ModHandler);
            Registered = false;
            instance = null;
            m_onRegisteredAction = null;
        }

        private Action m_onRegisteredAction;

        /// <summary>
        /// Create a Advanced Stats And Effects API Instance. Please only create one per mod. 
        /// </summary>
        /// <param name="onRegisteredAction">Callback once the Advanced Stats And Effects API is active. You can Instantiate Advanced Stats And Effects API objects in this Action</param>
        public AdvancedStatsAndEffectsAPI(Action onRegisteredAction = null)
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            m_onRegisteredAction = onRegisteredAction;
            MyAPIGateway.Utilities.RegisterMessageHandler(ModHandlerID, ModHandler);
            if (ModName == "")
            {
                if (MyAPIGateway.Utilities.GamePaths.ModScopeName.Contains("_"))
                    ModName = MyAPIGateway.Utilities.GamePaths.ModScopeName.Split('_')[1];
                else
                    ModName = MyAPIGateway.Utilities.GamePaths.ModScopeName;
            }
        }

        private void ModHandler(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj is Dictionary<string, Delegate>)
            {
                ModAPIMethods = (Dictionary<string, Delegate>)obj;
                _VerifyVersion = (Func<int, string, bool>)ModAPIMethods["VerifyVersion"];

                Registered = VerifyVersion(ModAPIVersion, ModName);

                MyLog.Default.WriteLine("Registering Advanced Stats And Effects API for Mod '" + ModName + "'");

                if (Registered)
                {
                    try
                    {
                        _GetGameTime = (Func<long>)ModAPIMethods["GetGameTime"];
                        _SetStatAsConsumableTrigger = (Func<string, bool>)ModAPIMethods["SetStatAsConsumableTrigger"];
                        _ConfigureConsumable = (Func<string, bool>)ModAPIMethods["ConfigureConsumable"];
                        _ConfigureFixedStat = (Func<string, bool>)ModAPIMethods["ConfigureFixedStat"];
                        _ConfigureVirtualStat = (Func<string, bool>)ModAPIMethods["ConfigureVirtualStat"];
                        _AddBeforeCycleStatCallback = (Func<string, Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>)ModAPIMethods["AddBeforeCycleStatCallback"];
                        _AddStartStatCycleCallback = (Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>)ModAPIMethods["AddStartStatCycleCallback"];
                        _AddEndStatCycleCallback = (Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>)ModAPIMethods["AddEndStatCycleCallback"];
                        _AddBeforePlayersUpdateCallback = (Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>)ModAPIMethods["AddBeforePlayersUpdateCallback"];
                        _AddAfterPlayersUpdateCallback = (Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>)ModAPIMethods["AddAfterPlayersUpdateCallback"];
                        _AddBeforeCycleCallback = (Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>)ModAPIMethods["AddBeforeCycleCallback"];
                        _AddAfterCycleCallback = (Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>)ModAPIMethods["AddAfterCycleCallback"];
                        _AddFixedStatCycleCallback = (Func<string, Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent>, int, bool>)ModAPIMethods["AddFixedStatCycleCallback"];
                        _AddVirtualStatAbsorptionCicle = (Func<string, Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent>, int, bool>)ModAPIMethods["AddVirtualStatAbsorptionCicle"];
                        _AddAfterPlayerReset = (Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>)ModAPIMethods["AddAfterPlayerReset"];
                        _AddAfterPlayerRespawn = (Func<Action<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>)ModAPIMethods["AddAfterPlayerRespawn"];
                        _AddAfterBotAdd = (Func<Action<long, IMyCharacter>, int, bool>)ModAPIMethods["AddAfterBotAdd"];
                        _AddFixedEffect = (Func<long, string, byte, bool, bool>)ModAPIMethods["AddFixedEffect"];
                        _RemoveFixedEffect = (Func<long, string, byte, bool, bool>)ModAPIMethods["RemoveFixedEffect"];
                        _ClearOverTimeConsumable = (Func<long, bool>)ModAPIMethods["ClearOverTimeConsumable"];
                        _GetRemainOverTimeConsumable = (Func<long, string, float>)ModAPIMethods["GetRemainOverTimeConsumable"];
                        _GetLastHealthChange = (Func<long, Vector2>)ModAPIMethods["GetLastHealthChange"];
                        _DoPlayerConsume = (Func<long, MyDefinitionId, bool>)ModAPIMethods["DoPlayerConsume"];
                        _GetPlayerFixedStatStack = (Func<long, string, byte>)ModAPIMethods["GetPlayerFixedStatStack"];
                        _GetPlayerFixedStatRemainTime = (Func<long, string, long>)ModAPIMethods["GetPlayerFixedStatRemainTime"];
                        _GetPlayerFixedStatUpdateHash = (Func<long, int>)ModAPIMethods["GetPlayerFixedStatUpdateHash"];

                        if (m_onRegisteredAction != null)
                            m_onRegisteredAction();
                    }
                    catch (Exception e)
                    {
                        MyAPIGateway.Utilities.ShowMessage("Advanced Stats And Effects", "Mod '" + ModName + "' encountered an error when registering the Advanced Stats And Effects API, see log for more info.");
                        MyLog.Default.WriteLine("Advanced Stats And Effects: " + e);
                    }
                }
            }
        }

    }

}