using Sandbox.Definitions;
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

        public List<OnPlayerCanUpdate> BeforePlayersUpdate { get; set; } = new List<OnPlayerCanUpdate>();
        public List<OnPlayersUpdate> AfterPlayersUpdate { get; set; } = new List<OnPlayersUpdate>();
        public List<OnCanCycle> BeforeCycle { get; set; } = new List<OnCanCycle>();
        public List<OnCycle> AfterCycle { get; set; } = new List<OnCycle>();
        public List<OnPlayerRespawn> PlayerRespawn { get; set; } = new List<OnPlayerRespawn>();
        public List<OnPlayerReset> PlayerReset { get; set; } = new List<OnPlayerReset>();
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
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            }

            if (IsServer)
            {

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NETWORK_ID_COMMANDS, CommandsMsgHandler);
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

        private const string SETTINGS_COMMAND = "settings";
        private const string SETTINGS_COMMAND_PLAYER_STATUS = "player.setstatus";
        private const string SETTINGS_COMMAND_PLAYER_RESETSTATUS = "player.resetstatus";

        private static readonly Dictionary<string, KeyValuePair<int, bool>> VALID_COMMANDS = new Dictionary<string, KeyValuePair<int, bool>>()
        {
            { SETTINGS_COMMAND, new KeyValuePair<int, bool>(3, false) },
            { SETTINGS_COMMAND_PLAYER_STATUS, new KeyValuePair<int, bool>(3, true) },
            { SETTINGS_COMMAND_PLAYER_RESETSTATUS, new KeyValuePair<int, bool>(1, true) }
        };

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            sendToOthers = true;
            if (!messageText.StartsWith("/")) return;
            var words = messageText.Trim().ToLower().Replace("/", "").Split(' ');
            if (words.Length > 0)
            {
                if (VALID_COMMANDS.ContainsKey(words[0]))
                {
                    if ((!VALID_COMMANDS[words[0]].Value && words.Length == VALID_COMMANDS[words[0]].Key) ||
                        (VALID_COMMANDS[words[0]].Value && words.Length >= VALID_COMMANDS[words[0]].Key))
                    {
                        sendToOthers = false;
                        Command cmd = new Command(MyAPIGateway.Multiplayer.MyId, words);
                        string message = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
                        MyAPIGateway.Multiplayer.SendMessageToServer(
                            NETWORK_ID_COMMANDS,
                            Encoding.Unicode.GetBytes(message)
                        );
                    }
                }
            }
        }

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

        private void DoCommand_Settings(string name, string value)
        {
            AdvancedStatsAndEffectsSettings.Instance.SetConfigValue(name, value);
        }

        private void DoCommand_PlayerStat(string name, string value, string player, ulong steamId)
        {
            PlayerCharacterBodyController playerChar = null;
            if (!string.IsNullOrWhiteSpace(player))
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Values.FirstOrDefault(x => x.Player?.DisplayName.CompareTo(player) == 0);
            }
            else
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacterBySteamId(steamId);
            }
            if (playerChar != null)
            {
                float targetValue;
                if (float.TryParse(value, out targetValue))
                {
                    playerChar.SetCharacterStatValue(name, targetValue);
                }
            }
        }

        private void DoCommand_PlayerReset(string player, ulong steamId)
        {
            PlayerCharacterBodyController playerChar = null;
            if (!string.IsNullOrWhiteSpace(player))
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Values.FirstOrDefault(x => x.Player?.DisplayName.CompareTo(player) == 0);
            }
            else
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacterBySteamId(steamId);
            }
            if (playerChar != null)
            {
                playerChar.ResetCharacterStats();
            }
        }

        private void CommandsMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                var message = Encoding.Unicode.GetString(data);
                var mCommandData = MyAPIGateway.Utilities.SerializeFromXML<Command>(message);
                if (MyAPIGateway.Session.IsUserAdmin(steamId))
                {
                    if (VALID_COMMANDS.ContainsKey(mCommandData.content[0]))
                    {
                        if ((!VALID_COMMANDS[mCommandData.content[0]].Value && mCommandData.content.Length == VALID_COMMANDS[mCommandData.content[0]].Key) ||
                            (VALID_COMMANDS[mCommandData.content[0]].Value && mCommandData.content.Length >= VALID_COMMANDS[mCommandData.content[0]].Key))
                        {
                            switch (mCommandData.content[0])
                            {
                                case SETTINGS_COMMAND:
                                    DoCommand_Settings(
                                        mCommandData.content[1],
                                        mCommandData.content[2]
                                    );
                                    break;
                                case SETTINGS_COMMAND_PLAYER_STATUS:
                                    DoCommand_PlayerStat(
                                        mCommandData.content[1],
                                        mCommandData.content[2],
                                        mCommandData.content.Length >= 4 ? mCommandData.content[3] : null,
                                        mCommandData.sender
                                    );
                                    break;
                                case SETTINGS_COMMAND_PLAYER_RESETSTATUS:
                                    DoCommand_PlayerReset(
                                        mCommandData.content.Length >= 2 ? mCommandData.content[1] : null,
                                        mCommandData.sender
                                    );
                                    break;
                            }
                        }
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
            AdvancedStatsAndEffectsAPIBackend.BeforeStart();
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
