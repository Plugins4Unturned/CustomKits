using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Core.Commands;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Unturned;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.CustomKits.Plugin
{
    public class CustomKitsPlugin : RocketPlugin<CustomKitsConfig>
    {
        public static string PluginName = "CustomKits";
        public static string PluginVersion = "1.7.0";
        public static string BuildVersion = "21";
        public static string RocketVersion = "4.9.3.0";
        public static string UnturnedVersion = "3.23.5.0";
        
        public static string ThisDirectory = System.IO.Directory.GetCurrentDirectory() + @"\Plugins\CustomKits\";
        
        public static bool HasPerms;
        public const string PERMISSION = "ck.preset.";
        
        public static CustomKitsPlugin Instance;
        
        public static void Write(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void Write(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public void CheckForUpdates(string xmlUrl)
        {
            string updateDir = System.IO.Directory.GetCurrentDirectory() + @"\Updates\";
            string downloadURL = "";
            string newVersion = "";
            string updateInfo = "";
            XmlTextReader reader = null;

            try
            {
                reader = new XmlTextReader(xmlUrl);
                reader.MoveToContent();
                string elementName = "";

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "appinfo"))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = reader.Value;
                                        break;
                                    case "url":
                                        downloadURL = reader.Value;
                                        break;
                                    case "about":
                                        updateInfo = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                Logger.LogError("Update server down, please try again later\n");
                return;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            
            if (newVersion == PluginVersion)
                return;

            if (!System.IO.Directory.Exists(updateDir))
            {
                System.IO.Directory.CreateDirectory(updateDir);
            }

            if (File.Exists(updateDir + "Update-" + newVersion + ".zip"))
                return;

            try
            {
                new WebClient().DownloadFile(downloadURL, updateDir + "Update-" + newVersion + ".zip");
                
                Write(string.Format(updateInfo, newVersion) + "\n", ConsoleColor.Green);
            }
            catch
            {
                Logger.LogError("The update has failed to download\n");
            }
        }
        public void PlayerConnected(UnturnedPlayer player)
        {
            int maxValue = 50;

            foreach (CustomKitsConfig.Preset Preset in Configuration.Instance.Presets)
            {
                if (player.IsAdmin)
                {
                    SlotManager.AddSlot(player, maxValue, int.MaxValue);
                }
                else
                {
                    if (!HasPerms)
                    {
                        SlotManager.AddSlot(player, 1, int.MaxValue);
                    }
                    else
                    {
                        if (player.HasPermission(PERMISSION + "*"))
                        {
                            SlotManager.AddSlot(player, maxValue, Preset.ItemLimit);
                        }
                        else if (player.HasPermission(PERMISSION + Preset.Name))
                        {
                            if (Preset.Name == "*")
                            {
                                SlotManager.AddSlot(player, maxValue, Preset.ItemLimit);
                            }
                            else
                            {
                                SlotManager.AddSlot(player, Preset.SlotCount, Preset.ItemLimit);
                            }
                        }
                    }
                }
            }

            if (!KitManager.Kits.ContainsKey(player.CSteamID.m_SteamID))
            {
                KitManager.Kits.Add(player.CSteamID.m_SteamID, new Dictionary<string, InventoryManager.Inventory>());
            }
        }

        protected override void Load()
        {
            Instance = this;
            SlotManager.Slots = new Dictionary<ulong, List<SlotManager.Slot>>();
            KitManager.AutoSaveKits = new Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>();

            UnturnedPlayerEvents.OnPlayerRevive += OnPlayerRevive;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerInventoryRemoved += OnItemRemoved;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            R.Plugins.OnPluginsLoaded += OnPluginsLoad;
            Events.OnKitDeleted += OnKitDeleted;
            Events.OnKitSaved += OnKitSaved;

            Write("\n" + PluginName + " " + PluginVersion + " BETA-" + BuildVersion);
            Write("Made by Teyhota");
            Write("for Rocket " + RocketVersion + "\n");

            // update check
            if (Instance.Configuration.Instance.DisableAutoUpdate != "true")
            {
                CheckForUpdates("http://plugins.4unturned.tk/plugins/CustomKits/update.xml");
            }

            // dependency check
            if (!File.Exists(System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Libraries" + Path.DirectorySeparatorChar + "Newtonsoft.Json.dll"))
            {
                Logger.LogError("Dependency 'Newtonsoft.Json.dll' was not found!\nThe 'RetainKitsOnRestart' feature has been disabled!" + "\n");

                Configuration.Instance.KeepKitsOnRestart = false;
            }

            // permission check
            string rocketPerms = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Permissions.config.xml").ToLower();

            if (rocketPerms.Contains(PERMISSION))
            {
                HasPerms = true;
            }
            else
            {
                HasPerms = false;

                Logger.LogError("You need to setup the correct permissions \nif you want players to be able to save more than 1 kit!\n");
            }

            // load offline kits
            if (Instance.Configuration.Instance.KeepKitsOnRestart)
            {
                if (File.Exists(ThisDirectory + "StoredKits.json"))
                {
                    string json = File.ReadAllText(ThisDirectory + "StoredKits.json");

                    if (json == "null" || json == "{}" || json.Length == 0)
                    {
                        KitManager.Kits = new Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>();

                        Write("No StoredKits.json file to load!\n", ConsoleColor.Yellow);
                    }
                    else
                    {
                        KitManager.Kits = new Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>(JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>>(json));

                        Write("Successfully loaded StoredKits.json file!\n", ConsoleColor.Green);
                    }
                }
                else
                {
                    File.Create(ThisDirectory + "StoredKits.json").Close();

                    KitManager.Kits = new Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>();
                }
            }
            else
            {
                KitManager.Kits = new Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>>();
            }

            // if Rocket is Reloaded...
            foreach (SteamPlayer sP in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(sP);

                PlayerConnected(player);
            }
        }

        #region Yes command
        [RocketCommand("yes", "", "", AllowedCaller.Player), RocketCommandPermission("ck.delete")]
        public void ExecuteYes(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (Commands.Command_Delete.Yes.Contains(callr.CSteamID))
            {
                // Delete all kits
                KitManager.DeleteKit(callr, "*", KitManager.Kits);
                UnturnedChat.Say(caller, Translate("all_kits_deleted"), Color.green);

                Commands.Command_Delete.Yes.Remove(callr.CSteamID);
            }
            else
            {
                UnturnedChat.Say(caller, "Command not found.", Color.red);
            }
        }
        #endregion

        private void OnPluginsLoad()
        {
            if (Configuration.Instance.KeepKitsOnRestart == true)
            {
                if (U.Settings.Instance.AutomaticSave.Enabled == true)
                {
                    StartCoroutine(KitManager.AutoStoreKits());
                }
            }
        }

        private void OnItemRemoved(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (LevelManager.levelType == ELevelType.ARENA && player.Player.movement.isSafe)
            {
                var safeZoneInfo = player.Player.movement.isSafeInfo;

                if (safeZoneInfo.noWeapons && safeZoneInfo.noBuildables) // aka the Lobby
                {
                    ItemManager.askClearRegionItems(player.Player.movement.region_x, player.Player.movement.region_y);
                }
            }
        }

        private void OnKitSaved(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName)
        {
            if (Configuration.Instance.KeepKitsOnRestart == true)
            {
                KitManager.TryStoreKits();
            }
        }

        private void OnKitDeleted(UnturnedPlayer player, string kitName)
        {
            if (Configuration.Instance.KeepKitsOnRestart == true)
            {
                KitManager.TryStoreKits();
            }
        }

        private void OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            if (Commands.Command_AutoSkills.AutoSkills.Contains(player.CSteamID))
            {
                player.MaxSkills();
                UnturnedChat.Say(player, Translate("max_skills"), Color.green);
            }

            if (Commands.Command_AutoCopy.AutoCopy.Contains(player.CSteamID))
            {
                InventoryManager.AutoCopy(player);
            }

            if (Commands.Command_AutoSave.AutoSave.ContainsKey(player.CSteamID))
            {
                InventoryManager.AutoSave(player);
            }

            if (Commands.Command_AutoLoad.AutoLoad.ContainsKey(player.CSteamID))
            {
                string kitName = Commands.Command_AutoLoad.AutoLoad[player.CSteamID];
                
                if (!KitManager.HasSavedKits(player, KitManager.Kits))
                {
                    UnturnedChat.Say(player, Instance.Translate("no_saved_kits"), Color.red);
                    return;
                }

                if (!KitManager.HasKit(player, kitName, KitManager.Kits))
                {
                    UnturnedChat.Say(player, Translate("no_kit_exists"), Color.red);
                    return;
                }

                StartCoroutine(KitManager.DelayedLoad(player, kitName, 0.3f));
            }
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            ItemManager.askClearRegionItems(player.Player.movement.region_x, player.Player.movement.region_y);

            if (Commands.Command_AutoSave.AutoSave.ContainsKey(player.CSteamID))
            {
                string kitName = Commands.Command_AutoSave.AutoSave[player.CSteamID];

                KitManager.SaveKit(player, player, kitName, KitManager.AutoSaveKits);
            }

            if (Commands.Command_AutoCopy.Murdered.ContainsKey(player.CSteamID))
            {
                Commands.Command_AutoCopy.Murdered.Remove(player.CSteamID);
            }

            if (Configuration.Instance.KeepKitsOnDeath == false)
            {
                KitManager.DeleteKit(player, "*", KitManager.Kits);
            }

            IRocketPlayer mMurderer = UnturnedPlayer.FromCSteamID(murderer);

            if (!(mMurderer is ConsolePlayer))
            {
                Commands.Command_AutoCopy.Murdered.Add(player.CSteamID, murderer);
            }
        }
        
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            PlayerConnected(player);
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            SlotManager.ClearSlots(player);

            if (Commands.Command_Delete.Yes.Contains(player.CSteamID))
            {
                Commands.Command_Delete.Yes.Remove(player.CSteamID);
            }
            
            if (Configuration.Instance.KeepKitsOnDisconnect == false)
            {
                KitManager.DeleteKit(player, "*", KitManager.Kits);
            }
        }

        protected override void Unload()
        {
            KitManager.TryStoreKits();

            UnturnedPlayerEvents.OnPlayerRevive -= OnPlayerRevive;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerInventoryRemoved -= OnItemRemoved;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            R.Plugins.OnPluginsLoaded -= OnPluginsLoad;
            Events.OnKitDeleted -= OnKitDeleted;
            Events.OnKitSaved -= OnKitSaved;

            Write("Visit Plugins.4Unturned.tk for more!", ConsoleColor.Green);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"autoskills_on", "Auto-skills have been enabled!"},
                    {"autoskills_off", "Auto-skills have been disabled!"},
                    {"autocopy_on", "Auto-copy has been enabled!"},
                    {"autocopy_off", "Auto-copy has been disabled!"},
                    {"autosave_on", "Auto-save has been enabled and will be saved as \"{0}\"!"},
                    {"autosave_off", "Auto-save has been disabled!"},
                    {"autoload_on", "Auto-load has been enabled for \"{0}\"!"},
                    {"autoload_off", "Auto-load has been disabled!"},
                    {"max_skills", "All of your skills have been maxed out!"},
                    {"max_skills_other", "You maxed out {0}'s skills!"},
                    {"copy_bypass", "{0}'s inventory cannot be copied!"},
                    {"copied", "You copied {0}'s inventory!"},
                    {"auto_stored", "Successfully stored all kits!"},
                    {"are_you_sure", "Type \"/yes\" to delete all kits!"},
                    {"all_kits_deleted", "Successfully deleted all kits!"},
                    {"kit_deleted", "Successfully deleted, {0}"},
                    {"kit_saved", "Successfully saved, {0}"},
                    {"kit_loaded", "Successfully loaded, {0}"},
                    {"kit_list", "Your saved kits: {0}"},
                    {"kit_list_other", "{0}'s saved kits: {1}"},
                    {"no_saved_kits", "You don't have any saved kits!"},
                    {"no_saved_kits_other", "{0} doesn't have any saved kits!"},
                    {"no_kit_exists", "A kit with that name doesn't exist!"},
                    {"no_kits_left", "You have reached the maximum amount of slots for saving kits"},
                    {"blacklisted", "{0} is blacklisted and won't be included in your saved kit"},
                    {"item_limit", "You can't have more than {0} items in your kit!"},
                    {"ci_bypass", "{0}'s inventory cannot be cleared!"},
                    {"inventory_cleared", "Your inventory has been cleared!"},
                    {"inventory_cleared_other", "You cleared {0}'s inventory!"},
                    {"player_doesn't_exist", "{0} either doesn't exist, or is offline!"},
                    {"empty_inventory", "Failed, because your inventory is empty!"},
                    {"unsupported_character", "You cannot use {0} as your kit name!"},
                    {"set_permissions", "An error has occured because permissions were not set up."},
                    {"gave_slot", "You gave {0} {1} slots with item limit of {2}"},
                    {"received_slot", "{0} gave you {1} slots with item limit of {2}"}
                };
            }
        }
    }
}