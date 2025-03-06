using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace AdvancedStatsAndEffects
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class AdvancedStatsAndEffectsEntityManager : BaseSessionComponent
    {

        public static AdvancedStatsAndEffectsEntityManager Instance { get; private set; }

        public ConcurrentDictionary<long, PlayerCharacterBodyController> PlayerCharacters { get; private set; } = new ConcurrentDictionary<long, PlayerCharacterBodyController>();
        public ConcurrentDictionary<long, BotCharacterBodyController> BotCharacters { get; private set; } = new ConcurrentDictionary<long, BotCharacterBodyController>();
        public ConcurrentDictionary<long, IMyPlayer> Players { get; private set; } = new ConcurrentDictionary<long, IMyPlayer>();

        private bool inicialLoadComplete = false;
        private bool sessionReady = false;

        protected override void DoInit(MyObjectBuilder_SessionComponent sessionComponent)
        {
            Instance = this;
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            if (IsServer)
            {
                RegisterWatcher();
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                Players?.Clear();
                Players = null;
                PlayerCharacters.Clear();
                PlayerCharacters = null;
                BotCharacters.Clear();
                BotCharacters = null;
                MyVisualScriptLogicProvider.PlayerConnected -= Players_PlayerConnected;
                MyVisualScriptLogicProvider.PlayerDisconnected -= Players_PlayerDisconnected;
                MyEntities.OnEntityAdd -= Entities_OnEntityAdd;
                MyEntities.OnEntityRemove -= Entities_OnEntityRemove;
                MyAPIGateway.Session.OnSessionReady -= Session_OnSessionReady;
            }
            base.UnloadData();
        }

        public void Players_PlayerConnected(long playerId)
        {
            MyAPIGateway.Parallel.Start(() => {
                MyAPIGateway.Parallel.Sleep(1000);

                var tempPlayers = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(tempPlayers, (x) => x.Identity.IdentityId == playerId);
                if (tempPlayers.Count > 0)
                {
                    DoProcessPlayerList(tempPlayers);
                }
            });
        }

        public void Players_PlayerDisconnected(long playerId)
        {
            if (Players.ContainsKey(playerId))
                Players.Remove(playerId);
        }

        public PlayerCharacterBodyController GetPlayerCharacterBySteamId(ulong steamId)
        {
            var query = PlayerCharacters.Where(x => x.Value.Player?.SteamUserId == steamId);
            return query.Any() ? query.FirstOrDefault().Value : null;
        }

        public PlayerCharacterBodyController GetPlayerCharacter(long playerId)
        {
            var query = PlayerCharacters.Where(x => x.Value.PlayerId == playerId);
            return query.Any() ? query.FirstOrDefault().Value : null;
        }

        public CharacterBodyController GetCharacter(long id)
        {
            if (PlayerCharacters.ContainsKey(id))
                return PlayerCharacters[id];
            if (BotCharacters.ContainsKey(id))
                return BotCharacters[id];
            return null;
        }

        public void UpdatePlayerList()
        {
            Players.Clear();
            var tempPlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(tempPlayers);
            DoProcessPlayerList(tempPlayers);
        }

        private void DoProcessPlayerList(List<IMyPlayer> tempPlayers)
        {
            foreach (var p in tempPlayers)
            {
                if (p?.Character == null || p.Character.IsDead)
                    continue;

                if (p.IsValidPlayer())
                {
                    Players[p.IdentityId] = p;
                    DoAddPlayerCharacter(p.IdentityId, p.Character, false, 0);
                }
            }
        }

        public void RegisterWatcher()
        {

            MyAPIGateway.Session.OnSessionReady += Session_OnSessionReady;

            foreach (var entity in MyEntities.GetEntities())
            {
                Entities_OnEntityAdd(entity);
            }
            inicialLoadComplete = true;

            MyEntities.OnEntityAdd += Entities_OnEntityAdd;
            MyEntities.OnEntityRemove += Entities_OnEntityRemove;

            UpdatePlayerList();

            MyVisualScriptLogicProvider.PlayerConnected += Players_PlayerConnected;
            MyVisualScriptLogicProvider.PlayerDisconnected += Players_PlayerDisconnected;

        }

        private void Session_OnSessionReady()
        {
            sessionReady = true;
            MyAPIGateway.Session.OnSessionReady -= Session_OnSessionReady;
        }

        private void Entities_OnEntityRemove(MyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null)
            {
                if (BotCharacters.ContainsKey(character.EntityId))
                    BotCharacters.Remove(character.EntityId);
            }
        }

        private bool DoAddPlayerCharacter(long playerId, IMyCharacter character, bool newPod, long podId)
        {
            if (character != null && character.IsValidPlayer())
            {
                if (AdvancedStatsAndEffectsSettings.Instance.Debug)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsEntityManager), $"MyEntities_OnEntityAddWatcher IMyCharacter PlayerId:{playerId} EntityId:{character.EntityId} DisplayName:{character.DisplayName}");
                }
                if (PlayerCharacters.Any(x => x.Value.PlayerId == playerId))
                {
                    var playerData = PlayerCharacters.FirstOrDefault(x => x.Value.PlayerId == playerId);
                    var playerChar = playerData.Value;
                    var newChar = playerChar.Entity.EntityId != character.EntityId;
                    playerChar.ConfigureCharacter(character);
                    if (newChar)
                    {
                        PlayerCharacters.Remove(playerData.Key);
                        PlayerCharacters[character.EntityId] = playerChar;
                    }
                    if (newChar || newPod)
                    {
                        if (newPod && AdvancedStatsAndEffectsSettings.Instance.CheckOtherGridsOnResetPlayerStats && 
                            AdvancedStatsAndEffectsSession.Static.PlayerRespawn.Any() && playerChar.Player != null)
                        {
                            foreach (var gridId in playerChar.Player.Grids)
                            {
                                var grid = MyAPIGateway.Entities.GetEntityById(gridId) as MyCubeGrid;
                                if (grid != null && grid.BlocksCount > 1 && grid.EntityId != podId)
                                {
                                    newPod = false;
                                    break;
                                }
                            }
                        }
                        foreach (var playerRespawn in AdvancedStatsAndEffectsSession.Static.PlayerRespawn)
                        {
                            if (playerRespawn.Action != null)
                            {
                                playerRespawn.Action(playerId, character, character.Components.Get<MyCharacterStatComponent>(), newPod);
                            }
                        }
                    }
                }
                else
                {
                    PlayerCharacters[character.EntityId] = new PlayerCharacterBodyController(character);
                    var steamId = PlayerCharacters[character.EntityId].Player?.SteamUserId;
                    if (steamId.HasValue)
                    {
                        var data = AdvancedStatsAndEffectsStorage.Instance.GetPlayerData(steamId.Value);
                        if (data != null)
                            PlayerCharacters[character.EntityId].LoadStoreData(data);
                        else
                            PlayerCharacters[character.EntityId].ResetCharacterStats();
                    }
                    else
                    {
                        PlayerCharacters[character.EntityId].ResetCharacterStats();
                    }
                    if (newPod && AdvancedStatsAndEffectsSession.Static.PlayerRespawn.Any() && PlayerCharacters[character.EntityId].Player != null)
                    {
                        foreach (var gridId in PlayerCharacters[character.EntityId].Player.Grids)
                        {
                            var grid = MyAPIGateway.Entities.GetEntityById(gridId) as MyCubeGrid;
                            if (grid != null && grid.BlocksCount > 1 && grid.EntityId != podId)
                            {
                                newPod = false;
                                break;
                            }
                        }
                    }
                    foreach (var playerRespawn in AdvancedStatsAndEffectsSession.Static.PlayerRespawn)
                    {
                        if (playerRespawn.Action != null)
                        {
                            playerRespawn.Action(playerId, character, character.Components.Get<MyCharacterStatComponent>(), newPod);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void Entities_OnEntityAdd(MyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null)
            {
                var playerId = character.GetPlayerId();
                if (!DoAddPlayerCharacter(playerId, character, false, 0))                
                {
                    if (!BotCharacters.ContainsKey(character.EntityId))
                    {
                        if (AdvancedStatsAndEffectsSettings.Instance.Debug)
                        {
                            AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsEntityManager), $"MyEntities_OnEntityAddWatcher IMyCharacter BotId:{playerId} EntityId:{character.EntityId} DisplayName:{character.Name}");
                        }
                        BotCharacters[character.EntityId] = new BotCharacterBodyController(character);
                        foreach (var afterBotAdd in AdvancedStatsAndEffectsSession.Static.AfterBotAdd)
                        {
                            if (afterBotAdd.Action != null)
                            {
                                afterBotAdd.Action(character.EntityId, character);
                            }
                        }
                    }
                }
            }
            else
            {
                var cubeGrid = entity as IMyCubeGrid;
                if (cubeGrid != null)
                {
                    if (!sessionReady)
                    {
                        var result = new List<IMySlimBlock>();
                        cubeGrid.GetBlocks(result, x => x.FatBlock as IMyCockpit != null);
                        foreach (var block in result)
                        {
                            var cryoBlock = block.FatBlock as IMyCockpit;
                            if (cryoBlock != null && cryoBlock.Pilot != null)
                            {
                                var playerId = cryoBlock.Pilot.GetPlayerId();
                                DoAddPlayerCharacter(playerId, cryoBlock.Pilot, false, 0);
                            }
                        }
                    }
                    else
                    {
                        if (cubeGrid.IsRespawnGrid)
                        {
                            var playerId = cubeGrid.BigOwners.FirstOrDefault();
                            MyAPIGateway.Parallel.Start(() => {
                                MyAPIGateway.Parallel.Sleep(1000);
                                if (Players.ContainsKey(playerId))
                                {
                                    DoAddPlayerCharacter(playerId, Players[playerId].Character, true, cubeGrid.EntityId);
                                }
                            });
                        }
                    }
                }
            }
        }

    }

}
