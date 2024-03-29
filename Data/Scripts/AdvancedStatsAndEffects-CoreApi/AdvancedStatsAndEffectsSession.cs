﻿using Sandbox.Definitions;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace AdvancedStatsAndEffects
{

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class AdvancedStatsAndEffectsSession : BaseSessionComponent
    {

        public class OnPlayerCanUpdate
        {

            public Func<long, IMyCharacter, MyCharacterStatComponent, bool> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnPlayersUpdate
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnCanCycle
        {

            public Func<long, IMyCharacter, MyCharacterStatComponent, bool> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnCycle
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnFixedStatCycle
        {

            public Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnStatCycle
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnStatBeforeCycle
        {

            public Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnVirtualStatAbsorptionCicle
        {

            public Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnBotAdd
        {

            public Action<long, IMyCharacter> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnAfterAddFixedEffect
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, string, byte, bool> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnAfterCharacterDied
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnAfterRemoveFixedEffect
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, string, byte, bool> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnPlayerConsume
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, MyDefinitionId> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnBeginConfigureCharacter
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, bool, Dictionary<string, float>> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnPlayerRespawn
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, bool> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnPlayerReset
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnPlayerMovementChange
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, MyCharacterMovementEnum, MyCharacterMovementEnum> Action { get; set; }
            public int Priority { get; set; }

        }

        public class OnHealthChanged
        {

            public Action<long, IMyCharacter, MyCharacterStatComponent, float, float, object> Action { get; set; }
            public int Priority { get; set; }

        }

        public List<OnPlayerCanUpdate> BeforePlayersUpdate { get; set; } = new List<OnPlayerCanUpdate>();
        public List<OnPlayersUpdate> AfterPlayersUpdate { get; set; } = new List<OnPlayersUpdate>();
        public List<OnCanCycle> BeforeCycle { get; set; } = new List<OnCanCycle>();        
        public ConcurrentDictionary<string, List<OnFixedStatCycle>> FixedStatCycle { get; set; } = new ConcurrentDictionary<string, List<OnFixedStatCycle>>();
        public List<OnCycle> AfterCycle { get; set; } = new List<OnCycle>();
        public List<OnBotAdd> AfterBotAdd { get; set; } = new List<OnBotAdd>();
        public List<OnPlayerConsume> AfterPlayerConsume { get; set; } = new List<OnPlayerConsume>();
        public List<OnAfterAddFixedEffect> AfterAddFixedEffect { get; set; } = new List<OnAfterAddFixedEffect>();
        public List<OnAfterRemoveFixedEffect> AfterRemoveFixedEffect { get; set; } = new List<OnAfterRemoveFixedEffect>();
        public List<OnBeginConfigureCharacter> BeginConfigureCharacter { get; set; } = new List<OnBeginConfigureCharacter>();
        public List<OnAfterCharacterDied> AfterCharacterDied { get; set; } = new List<OnAfterCharacterDied>();        
        public List<OnPlayerRespawn> PlayerRespawn { get; set; } = new List<OnPlayerRespawn>();
        public List<OnPlayerReset> PlayerReset { get; set; } = new List<OnPlayerReset>();
        public List<OnPlayerMovementChange> PlayerMovementChange { get; set; } = new List<OnPlayerMovementChange>();
        public List<OnHealthChanged> PlayerHealthChanged { get; set; } = new List<OnHealthChanged>();
        public ConcurrentDictionary<string, List<OnStatCycle>> StartStatCycle { get; set; } = new ConcurrentDictionary<string, List<OnStatCycle>>();
        public ConcurrentDictionary<string, List<OnStatCycle>> EndStatCycle { get; set; } = new ConcurrentDictionary<string, List<OnStatCycle>>();
        public ConcurrentDictionary<string, List<OnStatBeforeCycle>> StatBeforeCycle { get; set; } = new ConcurrentDictionary<string, List<OnStatBeforeCycle>>();
        public ConcurrentDictionary<string, List<OnVirtualStatAbsorptionCicle>> VirtualStatAbsorptionCicle { get; set; } = new ConcurrentDictionary<string, List<OnVirtualStatAbsorptionCicle>>();

        public static bool CheckChance(float chance)
        {
            return new Random().Next(1, 101) <= chance;
        }

        public const ushort NETWORK_ID_COMMANDS = 41823;
        public const ushort NETWORK_ID_DEFINITIONS = 41824;
        public const ushort NETWORK_ID_STATSSYSTEM = 41825;
        public const string CALL_FOR_DEFS = "NEEDDEFS";

        public static AdvancedStatsAndEffectsSession Static { get; private set; }

        private static List<Action> InvokeAfterCoreApiLoaded = new List<Action>();
        public static void AddToInvokeAfterCoreApiLoaded(Action action)
        {
            if (!ExtendedSurvivalCoreAPI.Registered)
                InvokeAfterCoreApiLoaded.Add(action);
        }

        public ConcurrentDictionary<string, MyEntityStatDefinition> BodyStatsConfigs { get; set; } = new ConcurrentDictionary<string, MyEntityStatDefinition>();
        public List<string> TriggerStats { get; set; } = new List<string>();
        public ConcurrentDictionary<MyDefinitionId, ConsumableInfo> ConsumablesInfo { get; set; } = new ConcurrentDictionary<MyDefinitionId, ConsumableInfo>();
        public ConcurrentDictionary<string, FixedStatInfo> FixedStatsInfo { get; set; } = new ConcurrentDictionary<string, FixedStatInfo>();
        public ConcurrentDictionary<string, VirtualStatInfo> VirtualStatInfo { get; set; } = new ConcurrentDictionary<string, VirtualStatInfo>();
        public PlayerClientUpdateData LastUpdateData { get; private set; }

        public ExtendedSurvivalCoreAPI ESCoreAPI;

        public bool IsVirtualStat(string id)
        {
            return VirtualStatInfo.ContainsKey(id);
        }

        public bool IsFixedStat(string id)
        {
            return FixedStatsInfo.ContainsKey(id);
        }

        public VirtualStatInfo GetVirtualStat(string id)
        {
            if (IsVirtualStat(id))
                return VirtualStatInfo[id];
            return null;
        }

        public FixedStatInfo GetFixedStat(string id)
        {
            if (IsFixedStat(id))
                return FixedStatsInfo[id];
            return null;
        }

        public void DoConfigureVirtualStat(VirtualStatInfo virtualStatInfo)
        {
            if (!VirtualStatInfo.Keys.Contains(virtualStatInfo.Name))
            {
                VirtualStatInfo[virtualStatInfo.Name] = virtualStatInfo;
                AdvancedStatsAndEffectsLogging.Instance.LogInfo(GetType(), $"Registred Virtual Stat : {virtualStatInfo.Name}");
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), $"DoConfigureVirtualStat : Virtual Stat is already registred");
            }
        }

        public void DoConfigureFixedStat(FixedStatInfo fixedStatInfo)
        {
            if (!FixedStatsInfo.Keys.Contains(fixedStatInfo.Id))
            {
                if (!FixedStatsInfo.Values.Any(x => x.Group == fixedStatInfo.Group && x.Index == fixedStatInfo.Index))
                {
                    FixedStatsInfo[fixedStatInfo.Id] = fixedStatInfo;
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(GetType(), $"Registred Fixed Stat : {fixedStatInfo.Id}");
                }
                else
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), $"DoConfigureFixedStat : Group and Index already registred");
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), $"DoConfigureFixedStat : Fixed Stat is already registred");
            }
        }

        public void DoConfigureConsumable(ConsumableInfo consumableInfo)
        {
            if (!ConsumablesInfo.Keys.Contains(consumableInfo.DefinitionId))
            {
                ConsumablesInfo[consumableInfo.DefinitionId] = consumableInfo;
                AdvancedStatsAndEffectsLogging.Instance.LogInfo(GetType(), $"Registred Consumable : {consumableInfo.DefinitionId}");                
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(GetType(), $"DoConfigureConsumable : Consumable is already registred");
            }
        }

        protected override void DoInit(MyObjectBuilder_SessionComponent sessionComponent)
        {

            Static = this;

            if (!IsDedicated)
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NETWORK_ID_STATSSYSTEM, ClientUpdateMsgHandler);
            }

            if (IsServer)
            {

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NETWORK_ID_DEFINITIONS, ClientDefinitionsUpdateServerMsgHandler);

                var stats = MyDefinitionManager.Static.GetDefinitionsOfType<MyEntityStatDefinition>();
                var playerCharacters = new string[] { "Default_Astronaut", "Default_Astronaut_Female" };
                foreach (var stat in stats)
                {
                    if (stat.DescriptionString == "#ASE#")
                    {
                        BodyStatsConfigs[stat.Name] = stat;
                        foreach (var character in playerCharacters)
                        {
                            DefinitionUtils.AddStatToCharacter(stat.Name, character);
                        }
                    }
                }

            }
            else
            {

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NETWORK_ID_DEFINITIONS, ClientDefinitionsUpdateMsgHandler);
                Command cmd = new Command(MyAPIGateway.Multiplayer.MyId, CALL_FOR_DEFS);
                string message = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
                MyAPIGateway.Multiplayer.SendMessageToServer(
                    NETWORK_ID_DEFINITIONS,
                    Encoding.Unicode.GetBytes(message)
                );

            }

        }

        protected override void UnloadData()
        {
            ESCoreAPI.Unregister();

            if (!IsDedicated)
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(NETWORK_ID_STATSSYSTEM, ClientUpdateMsgHandler);

            base.UnloadData();
        }

        private void ClientUpdateMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (netId != NETWORK_ID_STATSSYSTEM)
                    return;

                var message = Encoding.Unicode.GetString(data);
                LastUpdateData = MyAPIGateway.Utilities.SerializeFromXML<PlayerClientUpdateData>(message);

            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }

        }

        private const string SETTINGS_COMMAND = "settings";
        private const string SETTINGS_COMMAND_PLAYER_STATUS = "player.setstatus";
        private const string SETTINGS_COMMAND_PLAYER_RESETSTATUS = "player.resetstatus";

        private static readonly Dictionary<string, KeyValuePair<int, bool>> VALID_COMMANDS = new Dictionary<string, KeyValuePair<int, bool>>()
        {
            { SETTINGS_COMMAND, new KeyValuePair<int, bool>(3, false) },
            { SETTINGS_COMMAND_PLAYER_STATUS, new KeyValuePair<int, bool>(3, true) },
            { SETTINGS_COMMAND_PLAYER_RESETSTATUS, new KeyValuePair<int, bool>(1, true) }
        };

        private void ClientDefinitionsUpdateServerMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (netId != NETWORK_ID_DEFINITIONS)
                    return;

                var message = Encoding.Unicode.GetString(data);
                var mCommandData = MyAPIGateway.Utilities.SerializeFromXML<Command>(message);
                if (IsServer)
                {

                    switch (mCommandData.content[0])
                    {
                        default:
                            Command cmd = new Command(0, CALL_FOR_DEFS);
                            cmd.data = Encoding.Unicode.GetBytes(AdvancedStatsAndEffectsSettings.Instance.GetDataToClient());
                            string messageToSend = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
                            MyAPIGateway.Multiplayer.SendMessageTo(
                                NETWORK_ID_DEFINITIONS,
                                Encoding.Unicode.GetBytes(messageToSend),
                                mCommandData.sender
                            );
                            break;
                    }

                }

            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        public override void BeforeStart()
        {
            if (IsServer)
            {
                AdvancedStatsAndEffectsAPIBackend.BeforeStart();
            }
            else
            {
                AdvancedStatsAndEffectsClientAPIBackend.BeforeStart();
            }
        }

        public override void SaveData()
        {
            base.SaveData();

            if (IsServer)
            {
                try
                {
                    foreach (var key in AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Keys)
                    {
                        var player = AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters[key];
                        AdvancedStatsAndEffectsStorage.Instance.SetPlayerData(player.GetStoreData());
                    }
                }
                catch (Exception ex)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
                }

                AdvancedStatsAndEffectsSettings.Save();
                AdvancedStatsAndEffectsStorage.Save();
            }
        }

        public override void LoadData()
        {
            
            if (IsServer)
            {
                AdvancedStatsAndEffectsSettings.Load();
                AdvancedStatsAndEffectsStorage.Load();
                CheckDefinitions();
            }
            
            ESCoreAPI = new ExtendedSurvivalCoreAPI(() => {
                if (IsServer)
                {
                    if (ExtendedSurvivalCoreAPI.Registered)
                    {

                        if (InvokeAfterCoreApiLoaded.Any())
                            foreach (var action in InvokeAfterCoreApiLoaded)
                            {
                                action.Invoke();
                            }

                    }
                }
            });

            base.LoadData();
        }
        
        private bool definitionsChecked = false;
        private void CheckDefinitions()
        {
            AdvancedStatsAndEffectsLogging.Instance.LogInfo(GetType(), $"CheckDefinitions Called");
            if (!definitionsChecked)
            {
                definitionsChecked = true;

                var playerCharacters = new string[] { "Default_Astronaut", "Default_Astronaut_Female" };
                foreach (var character in playerCharacters)
                {
                    foreach (FixedStatsConstants.ValidStats stat in Enum.GetValues(typeof(FixedStatsConstants.ValidStats)).Cast<FixedStatsConstants.ValidStats>())
                    {
                        DefinitionUtils.AddStatToCharacter(stat.ToString(), character);
                    }
                }

            }
        }

        private void ClientDefinitionsUpdateMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (netId != NETWORK_ID_DEFINITIONS)
                    return;

                var message = Encoding.Unicode.GetString(data);
                var mCommandData = MyAPIGateway.Utilities.SerializeFromXML<Command>(message);
                if (IsClient)
                {
                    var settingsData = Encoding.Unicode.GetString(mCommandData.data);
                    AdvancedStatsAndEffectsSettings.ClientLoad(settingsData);
                    CheckDefinitions();
                }

            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        public IEnumerable<string> GetPlayerStatsList()
        {
            return BodyStatsConfigs.Keys;
        }

        protected override void DoUpdate60()
        {
            base.DoUpdate60();

            if (MyAPIGateway.Session.IsServer)
            {

                PlayersUpdate();
                CreaturesUpdate();

            }
        }

        private void CreaturesUpdate()
        {
            try
            {
                foreach (var key in AdvancedStatsAndEffectsEntityManager.Instance.BotCharacters.Keys)
                {
                    var bot = AdvancedStatsAndEffectsEntityManager.Instance.BotCharacters[key];
                    if (!bot.IsValid || bot.IsDead)
                        continue;
                    bot.ProcessStatsCycle();
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        private void PlayersUpdate()
        {
            try
            {
                foreach (var key in AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Keys)
                {
                    var player = AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters[key];
                    if (!player.IsValid || player.IsDead)
                        continue;
                    bool canExecute = true;
                    if (BeforePlayersUpdate.Any())
                        foreach (var beforeUpdate in BeforePlayersUpdate)
                        {
                            if (beforeUpdate.Action != null && !beforeUpdate.Action(key, player.Entity, player.StatComponent))
                            {
                                canExecute = false;
                                break;
                            }
                        }
                    if (canExecute)
                    {
                        player.ProcessStatsCycle();
                        if (AfterPlayersUpdate.Any())
                            foreach (var afterUpdate in AfterPlayersUpdate)
                            {
                                if (afterUpdate.Action != null)
                                    afterUpdate.Action(key, player.Entity, player.StatComponent);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

    }

}
