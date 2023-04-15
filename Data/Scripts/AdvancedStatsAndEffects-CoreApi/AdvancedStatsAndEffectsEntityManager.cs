using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
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

        protected override void DoInit(MyObjectBuilder_SessionComponent sessionComponent)
        {
            Instance = this;
            if (MyAPIGateway.Session.IsServer)
            {
                if (IsServer)
                {
                    RegisterWatcher();
                }
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                Players?.Clear();
                Players = null;
                MyVisualScriptLogicProvider.PlayerConnected -= Players_PlayerConnected;
                MyVisualScriptLogicProvider.PlayerDisconnected -= Players_PlayerDisconnected;
                MyEntities.OnEntityAdd -= Entities_OnEntityAdd;
                MyEntities.OnEntityRemove -= Entities_OnEntityRemove;
            }
            base.UnloadData();
        }

        public void Players_PlayerConnected(long playerId)
        {
            UpdatePlayerList();
        }

        public void Players_PlayerDisconnected(long playerId)
        {
            UpdatePlayerList();
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

            foreach (var p in tempPlayers)
            {
                if (p?.Character == null || p.Character.IsDead)
                    continue;

                if (p.IsValidPlayer())
                    Players[p.IdentityId] = p;
            }
        }

        public void RegisterWatcher()
        {

            foreach (var entity in MyEntities.GetEntities())
            {
                Entities_OnEntityAdd(entity);
            }
            inicialLoadComplete = true;

            UpdatePlayerList();

            MyVisualScriptLogicProvider.PlayerConnected += Players_PlayerConnected;
            MyVisualScriptLogicProvider.PlayerDisconnected += Players_PlayerDisconnected;

            MyEntities.OnEntityAdd += Entities_OnEntityAdd;
            MyEntities.OnEntityRemove += Entities_OnEntityRemove;

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

        private void Entities_OnEntityAdd(MyEntity entity)
        {
            var character = entity as IMyCharacter;
            if (character != null)
            {
                var playerId = character.GetPlayerId();
                if (character.IsValidPlayer())
                {
                    UpdatePlayerList();
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsEntityManager), $"MyEntities_OnEntityAddWatcher IMyCharacter PlayerId:{playerId} EntityId:{character.EntityId} DisplayName:{character.DisplayName}");
                    if (PlayerCharacters.Any(x => x.Value.PlayerId == playerId))
                    {
                        var playerChar = PlayerCharacters.FirstOrDefault(x => x.Value.PlayerId == playerId).Value;
                        var newChar = playerChar.Entity.EntityId != character.EntityId;
                        playerChar.ConfigureCharacter(character);
                        if (newChar)
                        {
                            foreach (var playerRespawn in AdvancedStatsAndEffectsSession.Static.PlayerRespawn)
                            {
                                if (playerRespawn.Action != null)
                                {
                                    playerRespawn.Action(playerId, character, character.Components.Get<MyCharacterStatComponent>(), false);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool newPod = false;
                        PlayerCharacters[character.EntityId] = new PlayerCharacterBodyController(character);
                        var steamId = PlayerCharacters[character.EntityId].Player?.SteamUserId;
                        if (steamId.HasValue)
                        {
                            PlayerCharacters[character.EntityId].LoadStoreData(AdvancedStatsAndEffectsStorage.Instance.GetPlayerData(steamId.Value));
                        }
                        else
                        {
                            newPod = true;
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
                    if (!BotCharacters.ContainsKey(character.EntityId))
                    {
                        AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsEntityManager), $"MyEntities_OnEntityAddWatcher IMyCharacter BotId:{playerId} EntityId:{character.EntityId} DisplayName:{character.Name}");
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
        }

    }

}
