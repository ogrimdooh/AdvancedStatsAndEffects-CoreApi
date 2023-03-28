using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using VRage.Utils;
using VRageMath;

namespace AdvancedStatsAndEffects
{

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class AdvancedStatsAndEffectsSettings : BaseSettings
    {
        
        private const int CURRENT_VERSION = 1;
        private const string FILE_NAME = "AdvancedStatsAndEffects.CoreApi.Settings.xml";

        private static AdvancedStatsAndEffectsSettings _instance;
        public static AdvancedStatsAndEffectsSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
            }
        }



        private static bool Validate(AdvancedStatsAndEffectsSettings settings)
        {
            var res = true;
            return res;
        }

        public static AdvancedStatsAndEffectsSettings Load()
        {
            _instance = Load(FILE_NAME, CURRENT_VERSION, Validate, () => { return new AdvancedStatsAndEffectsSettings(); });
            return _instance;
        }

        public static void ClientLoad(string data)
        {
            _instance = GetData<AdvancedStatsAndEffectsSettings>(data);
        }

        public string GetDataToClient()
        {
            return GetData(this);
        }

        public static void Save()
        {
            try
            {
                Save(Instance, FILE_NAME, true);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsSettings), ex);
            }
        }

        public AdvancedStatsAndEffectsSettings()
        {

        }

        protected override void OnAfterLoad()
        {
            base.OnAfterLoad();

        }

        public bool SetConfigValue(string name, string value)
        {
            switch (name)
            {
                
            }
            return false;
        }

    }

}
