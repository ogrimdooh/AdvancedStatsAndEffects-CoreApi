using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace AdvancedStatsAndEffects
{

    public static class IMyPlayerExtensions
    {
        public static bool IsValidPlayer(this IMyPlayer player)
        {
            return player != null && !player.IsBot && !string.IsNullOrEmpty(player.DisplayName) && MyAPIGateway.Players.TryGetSteamId(player.IdentityId) != 0;
        }
    }

}