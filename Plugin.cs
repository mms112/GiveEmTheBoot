using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace GiveEmTheBoot
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class GiveEmTheBootPlugin : BaseUnityPlugin
    {
        internal const string ModName = "GiveEmTheBoot";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource GiveEmTheBootLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            
            skillBonus = config("2 - Adjustments", "Skill Bonus", 50F, "How much bonus knockback force to add at 100 Unarmed skill. Scales proportionally with different skill levels.");
            bootBonus = config("2 - Adjustments", "Boot Bonus", 50F, "How much bonus knockback force to add when wearing boots weighing 15 pounds. Scales proportionally with different boots.");
            staggerBonus = config("2 - Adjustments", "Stagger Bonus", 25F, "How much bonus knockback force to add when the enemy is staggered.");
            flatBonus = config("2 - Adjustments", "Flat Bonus", 0F, "An unconditional knockback bonus to the kick for when you just want to kick a troll a mile away. Try setting this to 500.");
            weightFactor = config("2 - Adjustments", "Weight Factor", 0.8F, "How much of the enemy's weight to factor into the knockback. 1 means heavy creatures will basically not move. 0 means heavy creatures will be knocked just as far as light creatures.");
            xPushFactor = config("2 - Adjustments", "X Push Factor", 0.375F, "This is a direct multiplier against the x value in which the creature/player will be pushed when kicked.");
            yPushFactor = config("2 - Adjustments", "Y Push Factor", 0.375F, "This is a direct multiplier against the y value in which the creature/player will be pushed when kicked.");
            zPushFactor = config("2 - Adjustments", "Z Push Factor", 1.0F, "This is a direct multiplier against the z value in which the creature/player will be pushed when kicked.");
            showDialog = config("3 - Features", "Show Dialog", Toggle.On, "Whether or not to show a funny message when your kick connects to an enemy.", false);
            dialogSelection = config("3 - Features", "Dialog Selection", "PUNT|WHAM|BAM|YEET|SEEYA|BYE|HOOF|BOOT|GTFO|BEGONE", "Words that can appear if the Show Dialog option is enabled, separated by | character.", false);
            kickHotkey = config("3 - Features", "Kick Hotkey", new KeyboardShortcut(KeyCode.G), "Customizable kick hotkey so you can kick while holding a weapon. If you want to use a mouse key, include a space: mouse 3, for example. Valid inputs: https://docs.unity3d.com/ScriptReference/KeyCode.html", false);

            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                GiveEmTheBootLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                GiveEmTheBootLogger.LogError($"There was an issue loading your {ConfigFileName}");
                GiveEmTheBootLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        public static ConfigEntry<float> skillBonus = null!;
        public static ConfigEntry<float> bootBonus = null!;
        internal static ConfigEntry<float> staggerBonus = null!;
        internal static ConfigEntry<float> flatBonus = null!;
        internal static ConfigEntry<float> weightFactor = null!;
        internal static ConfigEntry<float> xPushFactor = null!;
        internal static ConfigEntry<float> yPushFactor = null!;
        internal static ConfigEntry<float> zPushFactor = null!;
        internal static ConfigEntry<Toggle> showDialog = null!;
        public static ConfigEntry<string> dialogSelection = null!;
        public static ConfigEntry<KeyboardShortcut> kickHotkey = null!;
        

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        #endregion
    }
}