using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CrocsItems.Equipment;
using CrocsItems.Interactables;
using CrocsItems.Items;
using HarmonyLib;
using R2API;
using R2API.ContentManagement;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]

namespace CrocsItems
{
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    // [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "HIFU";
        public const string PluginName = "CrocsItems";
        public const string PluginVersion = "0.0.1";
        public static ManualLogSource ModLogger;
        public static AssetBundle bundle;
        public static Main Instance;

        public static ConfigFile config;
        public static ConfigFile backupConfig;

        public static ConfigEntry<bool> enableLogging { get; set; }
        public ConfigEntry<bool> enableAutoConfig { get; private set; }
        public ConfigEntry<string> latestVersion { get; private set; }

        public void Awake()
        {
            Instance = this;

            ModLogger = base.Logger;

            SetUpConfig();
            SetUpContent();
        }

        public void SetUpConfig()
        {
            config = Config;
            backupConfig = new ConfigFile(BepInEx.Paths.ConfigPath + "\\com.HIFU.CrocsItems.Backup.cfg", true);
            backupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");
            enableAutoConfig = config.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop CrocsItems from syncing config whenever a new version is found.");
            bool _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(config, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = config.Bind("Config", "Latest Version", PluginVersion, "DO NOT CHANGE THIS");

            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != PluginVersion)))
            {
                latestVersion.Value = PluginVersion;
                ConfigManager.VersionChanged = true;
                ModLogger.LogInfo("Config Autosync Enabled.");
            }

            AutoRunCollector.HandleAutoRun();
            ConfigManager.HandleConfigAttributes(Assembly.GetExecutingAssembly(), Config);
        }

        public void SetUpContent()
        {
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)Activator.CreateInstance(equipmentType);
                if (LoadEquipment(equipment))
                {
                    equipment.Init();
                }
            }

            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)Activator.CreateInstance(itemType);
                if (LoadItem(item))
                {
                    item.Init();
                }
            }

            ScanTypes<InteractableBase>((x) =>
           {
               if (LoadInteractable(x))
               {
                   x.Init();
               }
           });
        }

        public bool LoadEquipment(EquipmentBase equipment)
        {
            var enabled = EquipmentBase.DefaultEnabledCallback(equipment);
            return enabled;
        }

        public bool LoadItem(ItemBase item)
        {
            var enabled = ItemBase.DefaultEnabledCallback(item);
            return enabled;
        }

        public bool LoadInteractable(InteractableBase interactable)
        {
            var enabled = InteractableBase.DefaultEnabledCallback(interactable);
            return enabled;
        }

        internal static void ScanTypes<T>(Action<T> action)
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types)
            {
                T instance = (T)Activator.CreateInstance(type);
                action(instance);
            }
        }
    }
}
