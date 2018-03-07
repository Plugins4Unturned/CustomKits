using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Items;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_Save : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "save";

        public string Help => "Save your current inventory as a kit";

        public string Syntax => "[kit name]";

        public List<string> Aliases => new List<string>() { "savekit" };

        public List<string> Permissions => new List<string> { "ck.save" };


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            string kitName = Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName;
            string presetName = "ADMIN_KIT";

            List<InventoryManager.Item> itemList = InventoryManager.ListItems(callr);
            int inventoryCount = itemList.Count;
            
            if (!caller.IsAdmin || !caller.HasPermission("ck.admin"))
            {
                string[] blackList = new string[] { };
                int itemLimit = int.MaxValue;

                foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
                {
                    if (caller.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                    {
                        presetName = Preset.Name;

                        if (Preset.Blacklist != "")
                        {
                            blackList = Preset.Blacklist.Split(',');
                            break;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("set_permissions"), Color.red);
                        return;
                    }
                }

                if (KitManager.KitCount(callr, KitManager.Kits) >= SlotManager.SlotCount(callr))
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_kits_left"), Color.red);
                    return;
                }

                var v = KitManager.KitCount(callr, KitManager.Kits);
                var slot = SlotManager.Slots[callr.CSteamID.m_SteamID][v];

                itemLimit = slot.itemLimit;

                if (blackList.Length > 0)
                {
                    foreach (InventoryManager.Item item in itemList)
                    {
                        List<int> bList = new List<int>();
                        foreach (var itemID in blackList)
                        {
                            bList.Add(int.Parse(itemID));
                        }

                        if (bList.Contains(item.id))
                        {
                            UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("blacklisted", UnturnedItems.GetItemAssetById(item.id)), Color.red);
                        }
                    }
                }

                if (inventoryCount > itemLimit)
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("item_limit", itemLimit), Color.red);
                    return;
                }
            }

            if (Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName == "preset_name")
            {
                kitName = presetName;
            }

            if (command.Length == 1)
            {
                kitName = command[0];
            }
            
            if (kitName == "*")
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("unsupported_character", "*"), Color.red);
                return;
            }
            
            if (inventoryCount < 1 || itemList == null)
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("empty_inventory"), Color.red);
                return;
            }

            if (KitManager.HasKit(callr, kitName, KitManager.Kits))
            {
                KitManager.DeleteKit(callr, kitName, KitManager.Kits);
            }

            KitManager.SaveKit(callr, callr, kitName, KitManager.Kits);
            
            UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_saved", kitName), Color.green);
        }
    }
}