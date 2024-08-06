using Sandbox.ModAPI;
using System;
using VRage.Utils;
using System.Xml.Serialization;

namespace AdvancedStatsAndEffects
{

    public abstract class BaseSettings
    {

        protected delegate bool BaseSettings_Validade<T>(T settings) where T : BaseSettings;
        protected delegate T BaseSettings_Create<T>() where T : BaseSettings;

        [XmlElement]
        public int Version { get; set; }

        [XmlElement]
        public bool Debug { get; set; } = false;

        protected static T Load<T>(string fileName, int currentVersion, BaseSettings_Validade<T> validade, BaseSettings_Create<T> create) where T : BaseSettings
        {
            var world = true;
            T settings = null;
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
                {
                    var needToRecreate = true;
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(T)))
                    {
                        var fileData = reader.ReadToEnd();
                        if (fileData.Length > 0)
                        {
                            try
                            {
                                settings = GetData<T>(fileData);
                                MyLog.Default.WriteLineAndConsole(fileName + ": Loaded from world file.");
                                needToRecreate = false;
                            }
                            catch (Exception)
                            {
                                MyLog.Default.WriteLineAndConsole(fileName + ": Loaded from world file error.");
                            }
                        }
                    }
                    if (needToRecreate)
                    {
                        settings = create();
                        settings.Version = currentVersion;
                        validade(settings);
                        Save<T>(settings, fileName, world);
                        MyLog.Default.WriteLineAndConsole(fileName + ": World file recreated.");
                    }
                }
                else if (MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, typeof(T)))
                {
                    world = false;
                    var needToRecreate = true;
                    using (var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(fileName, typeof(T)))
                    {
                        var fileData = reader.ReadToEnd();
                        if (fileData.Length > 0)
                        {
                            try
                            {
                                settings = GetData<T>(fileData);
                                MyLog.Default.WriteLineAndConsole(fileName + ": Loaded from local file.");
                                needToRecreate = false;
                            }
                            catch (Exception)
                            {
                                MyLog.Default.WriteLineAndConsole(fileName + ": Loaded from local file error.");
                            }
                        }
                    }
                    if (needToRecreate)
                    {
                        settings = create();
                        settings.Version = currentVersion;
                        validade(settings);
                        Save<T>(settings, fileName, world);
                        MyLog.Default.WriteLineAndConsole(fileName + ": Local file recreated.");
                    }
                }

                if (settings != null)
                {
                    var adjusted = false;
                    if (settings.Version < currentVersion)
                    {
                        MyLog.Default.WriteLineAndConsole(string.Format(fileName + ": Settings have old version: {0} update to {1}", settings.Version, currentVersion));

                        adjusted = true;
                        settings.Version = currentVersion;
                    }
                    adjusted = adjusted || !validade(settings);
                    if (adjusted) Save<T>(settings, fileName, world);
                }
                else
                {
                    settings = create();
                    settings.Version = currentVersion;
                    validade(settings);
                    Save<T>(settings, fileName, world);
                }
                settings.OnAfterLoad();
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole(string.Format(fileName + ": Exception while loading: {0}", ex.Message));
            }

            return settings;
        }

        protected virtual void OnAfterLoad()
        {

        }

        protected static T GetData<T>(string data) where T : BaseSettings
        {
            return MyAPIGateway.Utilities.SerializeFromXML<T>(data);
        }

        protected static string GetData<T>(T settings) where T : BaseSettings
        {
            return MyAPIGateway.Utilities.SerializeToXML(settings);
        }

        protected static void Save<T>(T settings, string fileName, bool world) where T : BaseSettings
        {
            var targetType = typeof(T);
            if (world)
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, targetType))
                {
                    writer.Write(GetData<T>(settings));
                }
            }
            else
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, targetType))
                {
                    writer.Write(GetData<T>(settings));
                }
            }
        }

    }

}
