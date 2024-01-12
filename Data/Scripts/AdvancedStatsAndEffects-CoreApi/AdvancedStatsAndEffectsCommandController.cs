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
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class AdvancedStatsAndEffectsCommandController : BaseSessionComponent
    {

        public class ValidCommand
        {

            public string Command { get; set; }
            public int MinOptions { get; set; }
            public bool NotFixedOptions { get; set; }
            //public HelpController.CommandEntryHelpInfo HelpInfo { get; set; }

            public ValidCommand(string command, int minOptions, bool notFixedOptions)
            {
                Command = command;
                MinOptions = minOptions;
                NotFixedOptions = notFixedOptions;
            }

        }

        private const string SETTINGS_COMMAND = "settings";
        private const string SETTINGS_COMMAND_PLAYER_STATUS = "player.setstatus";
        private const string SETTINGS_COMMAND_PLAYER_RESETSTATUS = "player.resetstatus";
        private const string SETTINGS_COMMAND_PLAYER_ALL_RESETSTATUS = "player.all.resetstatus";

        public static AdvancedStatsAndEffectsCommandController Static { get; private set; }

        protected override void DoInit(MyObjectBuilder_SessionComponent sessionComponent)
        {

            Static = this;

            if (!IsDedicated)
            {
                MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            }

            if (IsServer)
            {

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS, ServerCommandsMsgHandler);

            }
            else
            {

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS, ClientCommandsMsgHandler);

            }

        }

        public override void LoadData()
        {
            base.LoadData();
            VALID_COMMANDS[SETTINGS_COMMAND] = new ValidCommand(SETTINGS_COMMAND, 3, false);
            VALID_COMMANDS[SETTINGS_COMMAND_PLAYER_STATUS] = new ValidCommand(SETTINGS_COMMAND_PLAYER_STATUS, 3, true);
            VALID_COMMANDS[SETTINGS_COMMAND_PLAYER_RESETSTATUS] = new ValidCommand(SETTINGS_COMMAND_PLAYER_RESETSTATUS, 1, true);
            VALID_COMMANDS[SETTINGS_COMMAND_PLAYER_ALL_RESETSTATUS] = new ValidCommand(SETTINGS_COMMAND_PLAYER_ALL_RESETSTATUS, 1, false);            
        }

        protected override void UnloadData()
        {
            try
            {
                if (!IsDedicated)
                {
                    MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
                }
                if (IsServer)
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS, ServerCommandsMsgHandler);
                }
                else
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS, ClientCommandsMsgHandler);
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
            base.UnloadData();
        }

        public const string BASE_TOPIC_TYPE = "ExtendedSurvival.Stats.Command";
        public ConcurrentDictionary<string, ValidCommand> VALID_COMMANDS = new ConcurrentDictionary<string, ValidCommand>();

        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            sendToOthers = true;
            if (!messageText.StartsWith("/")) return;
            var words = messageText.Trim().ToLower().Replace("/", "").Split(' ');
            if (words.Length > 0)
            {
                if (VALID_COMMANDS.ContainsKey(words[0]))
                {
                    if ((!VALID_COMMANDS[words[0]].NotFixedOptions && words.Length == VALID_COMMANDS[words[0]].MinOptions) ||
                        (VALID_COMMANDS[words[0]].NotFixedOptions && words.Length >= VALID_COMMANDS[words[0]].MinOptions))
                    {
                        sendToOthers = false;
                        Command cmd = new Command(MyAPIGateway.Multiplayer.MyId, words);
                        if (IsServer)
                        {
                            if (MyAPIGateway.Session.IsUserAdmin(MyAPIGateway.Multiplayer.MyId))
                            {
                                HandlerMsgCommand(MyAPIGateway.Multiplayer.MyId, cmd);
                            }
                        }
                        else
                        {
                            string message = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
                            MyAPIGateway.Multiplayer.SendMessageToServer(
                                AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS,
                                Encoding.Unicode.GetBytes(message)
                            );
                        }
                    }
                }
            }
        }

        private void HandlerMsgCommand(ulong steamId, Command mCommandData)
        {
            if (MyAPIGateway.Session.IsUserAdmin(steamId))
            {
                if (VALID_COMMANDS.ContainsKey(mCommandData.content[0]))
                {
                    if ((!VALID_COMMANDS[mCommandData.content[0]].NotFixedOptions && mCommandData.content.Length == VALID_COMMANDS[mCommandData.content[0]].MinOptions) ||
                        (VALID_COMMANDS[mCommandData.content[0]].NotFixedOptions && mCommandData.content.Length >= VALID_COMMANDS[mCommandData.content[0]].MinOptions))
                    {
                        switch (mCommandData.content[0])
                        {
                            case SETTINGS_COMMAND:
                                if (DoCommand_Settings(
                                    mCommandData.content[1],
                                    mCommandData.content[2]
                                ))
                                {
                                    SendMessage(steamId, $"[AdvancedStatsAndEffects] Config {mCommandData.content[1]} set to {mCommandData.content[2]}.", MyFontEnum.White);
                                }
                                break;
                            case SETTINGS_COMMAND_PLAYER_STATUS:
                                if (DoCommand_PlayerStat(
                                    mCommandData.content[1],
                                    mCommandData.content[2],
                                    mCommandData.content.Length >= 4 ? mCommandData.content[3] : null,
                                    mCommandData.sender
                                ))
                                {
                                    SendMessage(steamId, $"[AdvancedStatsAndEffects] Stat {mCommandData.content[1]} set to {mCommandData.content[2]} (Player : {(mCommandData.content.Length >= 4 ? mCommandData.content[3] : "Self")}).", MyFontEnum.White);
                                }
                                break;
                            case SETTINGS_COMMAND_PLAYER_ALL_RESETSTATUS:
                                if (DoCommand_PlayerAllReset())
                                {
                                    SendMessage(steamId, $"[AdvancedStatsAndEffects] All players status reset.", MyFontEnum.White);
                                }
                                break;
                            case SETTINGS_COMMAND_PLAYER_RESETSTATUS:
                                if (DoCommand_PlayerReset(
                                    mCommandData.content.Length >= 2 ? mCommandData.content[1] : null,
                                    mCommandData.sender
                                ))
                                {
                                    SendMessage(steamId, $"[AdvancedStatsAndEffects] Reset stats for {(mCommandData.content.Length >= 2 ? mCommandData.content[1] : "Self")}.", MyFontEnum.White);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private bool DoCommand_Settings(string name, string value)
        {
            return AdvancedStatsAndEffectsSettings.Instance.SetConfigValue(name, value);
        }

        private PlayerCharacterBodyController GetPlayer(string player, ulong steamId)
        {
            PlayerCharacterBodyController playerChar = null;
            if (!string.IsNullOrWhiteSpace(player))
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Values.FirstOrDefault(x => x.Player?.DisplayName.CompareTo(player) == 0);
                ulong playerSteamId = 0;
                if (playerChar == null && ulong.TryParse(player, out playerSteamId))
                {
                    playerChar = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacterBySteamId(playerSteamId);
                }
            }
            else
            {
                playerChar = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacterBySteamId(steamId);
            }
            return playerChar;
        }

        private bool DoCommand_PlayerAllReset()
        {
            if (AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Any())
            {
                var listaResetados = new List<ulong>();
                foreach (var item in AdvancedStatsAndEffectsEntityManager.Instance.PlayerCharacters.Values)
                {
                    item.ResetCharacterStats();
                    listaResetados.Add(item.SteamUserId);
                }
                var idsToRemove = AdvancedStatsAndEffectsStorage.Instance.GetSavedPlayers().Where(x => !listaResetados.Contains(x)).ToArray();
                AdvancedStatsAndEffectsStorage.Instance.RemovePlayerData(idsToRemove);
                return true;
            }
            return false;
        }

        private bool DoCommand_PlayerStat(string name, string value, string player, ulong steamId)
        {
            var playerChar = GetPlayer(player, steamId);
            if (playerChar != null)
            {
                float targetValue;
                if (float.TryParse(value, out targetValue))
                {
                    playerChar.SetCharacterStatValue(name, targetValue);
                    return true;
                }
            }
            return false;
        }

        private bool DoCommand_PlayerReset(string player, ulong steamId)
        {
            var playerChar = GetPlayer(player, steamId);
            if (playerChar != null)
            {
                playerChar.ResetCharacterStats();
                return true;
            }
            return false;
        }

        private void ClientCommandsMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (netId != AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS)
                    return;

                if (IsClient)
                {
                    var message = Encoding.Unicode.GetString(data);
                    var mCommandData = MyAPIGateway.Utilities.SerializeFromXML<Command>(message);

                    int timeToLive = 0;
                    if (mCommandData.content.Length == 3 &&
                        int.TryParse(mCommandData.content[2], out timeToLive))
                    {
                        ShowMessage(mCommandData.content[0], mCommandData.content[1], timeToLive);
                    }
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        private void ServerCommandsMsgHandler(ushort netId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (netId != AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS)
                    return;

                if (IsServer)
                {
                    var message = Encoding.Unicode.GetString(data);
                    var mCommandData = MyAPIGateway.Utilities.SerializeFromXML<Command>(message);

                    HandlerMsgCommand(steamId, mCommandData);
                }
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

        public void SendMessage(ulong target, string text, string font = MyFontEnum.Red, int timeToLive = 5000)
        {
            if (IsDedicated || (IsServer && MyAPIGateway.Multiplayer.MyId != target))
            {
                string[] values = new string[]
                {
                    text,
                    font,
                    timeToLive.ToString()
                };
                Command cmd = new Command(IsDedicated ? 0 : MyAPIGateway.Multiplayer.MyId, values);
                string message = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
                MyAPIGateway.Multiplayer.SendMessageTo(
                    AdvancedStatsAndEffectsSession.NETWORK_ID_COMMANDS,
                    Encoding.Unicode.GetBytes(message),
                    target
                );
            }
            else
            {
                ShowMessage(text, font, timeToLive);
            }
        }

        public void ShowMessage(string text, string font = MyFontEnum.Red, int timeToLive = 5000)
        {
            var hudMsg = MyAPIGateway.Utilities.CreateNotification(string.Empty);
            hudMsg.Hide();
            hudMsg.Font = font;
            hudMsg.AliveTime = timeToLive;
            hudMsg.Text = text;
            hudMsg.Show();
        }

    }

}
