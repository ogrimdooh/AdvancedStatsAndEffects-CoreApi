using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using System.Xml.Serialization;
using System;

namespace AdvancedStatsAndEffects
{

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class AdvancedStatsAndEffectsStorage : BaseSettings
    {

        private const int CURRENT_VERSION = 1;
        private const string FILE_NAME = "AdvancedStatsAndEffects.CoreApi.Storage.xml";

        private static AdvancedStatsAndEffectsStorage _instance;
        public static AdvancedStatsAndEffectsStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }

        private static bool Validate(AdvancedStatsAndEffectsStorage settings)
        {
            var res = true;
            return res;
        }

        public static AdvancedStatsAndEffectsStorage Load()
        {
            _instance = Load(FILE_NAME, CURRENT_VERSION, Validate, () => { return new AdvancedStatsAndEffectsStorage(); });
            return _instance;
        }

        public static void Save()
        {
            try
            {
                Save(Instance, FILE_NAME, true);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsStorage), ex);
            }
        }

        [XmlArray("Players"), XmlArrayItem("Player", typeof(PlayerData))]
        public List<PlayerData> Players { get; set; } = new List<PlayerData>();

        private void CheckPlayers()
        {
            if (Players == null)
                Players = new List<PlayerData>();
            Players.RemoveAll(x => x == null);
        }

        public void SetPlayerData(PlayerData data)
        {
            if (data != null)
            {
                CheckPlayers();
                if (Players.Any(x => x.SteamPlayerId == data.SteamPlayerId))
                    Players.RemoveAll(x => x.SteamPlayerId == data.SteamPlayerId);
                Players.Add(data);
            }
        }

        public PlayerData GetPlayerData(ulong steamPlayerId)
        {
            CheckPlayers();
            if (Players.Any(x => x.SteamPlayerId == steamPlayerId))
                return Players.FirstOrDefault(x => x.SteamPlayerId == steamPlayerId);
            return null;
        }

        public AdvancedStatsAndEffectsStorage()
        {
            Players = new List<PlayerData>();
        }

    }

}
