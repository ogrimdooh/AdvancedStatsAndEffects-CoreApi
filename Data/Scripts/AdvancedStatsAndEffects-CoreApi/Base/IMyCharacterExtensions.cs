using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;

namespace AdvancedStatsAndEffects
{

    public static class IMyCharacterExtensions
    {

        public static readonly string[] VALID_PLAYERS_SUBTYPES = new string[] { "Default_Astronaut", "Default_Astronaut_Female" };
         
        public static bool IsValidPlayer(this IMyCharacter character)
        {
            var player = character.GetPlayer();
            return player != null && !player.IsBot;
        }

        public static bool IsValidBot(this IMyCharacter character)
        {
            var player = character.GetPlayer();
            return player != null && player.IsBot;
        }

        public static IMyPlayer GetPlayer(this IMyCharacter character)
        {
            var playerId = character.GetPlayerId();
            var players = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(players, (player) => { return player.IdentityId == playerId; });
            if (players.Any())
                return players[0];
            return null;
        }

        public static bool TryGetObjectBuilder(this IMyCharacter character, out MyObjectBuilder_Character builder)
        {
            builder = null;
            if (character != null)
            {
                try
                {
                    builder = (character.GetObjectBuilder() as MyObjectBuilder_Character);
                    return true;
                }
                catch (Exception)
                {
                    builder = null;
                }
            }
            return false;
        }

        public static long GetPlayerId(this IMyCharacter character)
        {
            if (character != null)
            {
                MyObjectBuilder_Character builder;
                if (character.TryGetObjectBuilder(out builder))
                {
                    var playerId = builder?.OwningPlayerIdentityId;
                    return playerId.HasValue ? playerId.Value : -1;
                }
            }
            return -1;
        }

    }

}
