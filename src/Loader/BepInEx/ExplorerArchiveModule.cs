using BepInEx;
using BepInEx.Configuration;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Loader;
using UnityExplorer;
using UnityExplorer.Config;

[assembly: ArchiveModule(ExplorerCore.GUID, ExplorerCore.NAME, ExplorerCore.VERSION)]

namespace UnityExplorer.Loader.BepInEx
{
    public class ExplorerArchiveModule : IArchiveModule, IExplorerLoader
    {
        public bool ApplyHarmonyPatches => false;

        public bool UsesLegacyPatches => false;

        public ArchiveLegacyPatcher Patcher { get; set; }

        public string ModuleGroup => ExplorerCore.NAME;

        public Dictionary<Language, string> ModuleGroupLanguages => new()
        {
            { Language.Chinese, "Unity Explorer" },
            { Language.English, "Unity Explorer" }
        };

        public string ExplorerFolderDestination => Paths.PluginPath;

        public string ExplorerFolderName => ExplorerCore.DEFAULT_EXPLORER_FOLDER_NAME;

        public string UnhollowedModulesFolder => Path.Combine(Paths.BepInExRootPath, "interop");

        public ConfigHandler ConfigHandler => _configHandler;
        private ArchiveModuleBepInExConfigHandler _configHandler;

        public Action<object> OnLogMessage => Logs.LogMessage;

        public Action<object> OnLogWarning => Logs.LogWarning;

        public Action<object> OnLogError => Logs.LogError;

        public static ExplorerArchiveModule Instance { get; private set; }

        public ConfigFile Config
        {
            get
            {
                if (_config == null)
                {
                    BepInPlugin bepInPlugin = new BepInPlugin(ExplorerCore.GUID, ExplorerCore.NAME, ExplorerCore.VERSION);
                    _config = new ConfigFile(Utility.CombinePaths(new string[]
                    {
                        Paths.ConfigPath,
                        bepInPlugin.GUID + ".cfg"
                    }), false, bepInPlugin);
                }
                return _config;
            }
        }
        private ConfigFile _config;

        public void Init()
        {
            Instance = this;            
            _configHandler = new ArchiveModuleBepInExConfigHandler();
            ExplorerCore.Init(this);
        }

        public void OnExit()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }
    }

    public class ArchiveModuleBepInExConfigHandler : ConfigHandler
    {
        private ConfigFile Config => ExplorerArchiveModule.Instance.Config;

        private const string CTG_NAME = "UnityExplorer";

        public override void Init()
        {
            // Not necessary
        }

        public override void RegisterConfigElement<T>(ConfigElement<T> config)
        {
            ConfigEntry<T> entry = Config.Bind(CTG_NAME, config.Name, config.Value, config.Description);

            entry.SettingChanged += (object o, EventArgs e) =>
            {
                config.Value = entry.Value;
            };
        }

        public override T GetConfigValue<T>(ConfigElement<T> element)
        {
            if (Config.TryGetEntry(CTG_NAME, element.Name, out ConfigEntry<T> configEntry))
                return configEntry.Value;
            else
                throw new Exception("Could not get config entry '" + element.Name + "'");
        }

        public override void SetConfigValue<T>(ConfigElement<T> element, T value)
        {
            if (Config.TryGetEntry(CTG_NAME, element.Name, out ConfigEntry<T> configEntry))
                configEntry.Value = value;
            else
                ExplorerCore.Log("Could not get config entry '" + element.Name + "'");
        }

        public override void LoadConfig()
        {
            foreach (KeyValuePair<string, IConfigElement> entry in ConfigManager.ConfigElements)
            {
                string key = entry.Key;
                ConfigDefinition def = new(CTG_NAME, key);
                if (Config.ContainsKey(def) && Config[def] is ConfigEntryBase configEntry)
                {
                    IConfigElement config = entry.Value;
                    config.BoxedValue = configEntry.BoxedValue;
                }
            }
        }

        public override void SaveConfig()
        {
            Config.Save();
        }
    }

    internal static class Logs
    {
        private static IArchiveLogger _logger;
        private static IArchiveLogger Logger => _logger ??= LoaderWrapper.CreateLoggerInstance(ExplorerCore.GUID);

        public static void LogDebug(object data)
        {
            Logger.Debug(data.ToString());
        }

        public static void LogError(object data)
        {
            Logger.Error(data.ToString());
        }

        public static void LogInfo(object data)
        {
            Logger.Info(data.ToString());
        }

        public static void LogMessage(object data)
        {
            Logger.Msg(ConsoleColor.White, data.ToString());
        }

        public static void LogWarning(object data)
        {
            Logger.Warning(data.ToString());
        }

        public static void LogNotice(object data)
        {
            Logger.Notice(data.ToString());
        }

        public static void LogSuccess(object data)
        {
            Logger.Success(data.ToString());
        }

        public static void LogException(Exception ex)
        {
            Logger.Exception(ex);
        }
    }
}
