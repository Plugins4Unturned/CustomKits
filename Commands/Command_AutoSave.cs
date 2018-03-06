using System.Collections.Generic;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using Steamworks;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_AutoSave : IRocketCommand
    {
        public static Dictionary<CSteamID, string> AutoSave = new Dictionary<CSteamID, string>();
        
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "autosave";

        public string Help => "Toggle Auto-save";

        public string Syntax => "<kit name>";

        public List<string> Aliases => new List<string>() { "asave" };

        public List<string> Permissions => new List<string> { "ck.autosave" };

        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;
            string kitName = null;

            if (command.Length == 0)
            {
                if (!AutoSave.ContainsKey(callr.CSteamID))
                {
                    if (Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName == "preset_name")
                    {
                        foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
                        {
                            if (caller.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                            {
                                kitName = Preset.Name;
                            }
                        }
                    }
                    else
                    {
                        kitName = Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName;
                    }
                }
                else
                {
                    AutoSave.Remove(callr.CSteamID);
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autosave_off"), Color.green);
                    return;
                }
            }

            int slotCount = SlotManager.Slots[callr.CSteamID.m_SteamID].Count;

            if (kitName == "*")
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("unsupported_character", "*"), Color.red);
                return;
            }

            if (KitManager.KitCount(callr, KitManager.Kits) >= slotCount)
            {
                if (!caller.IsAdmin || !caller.HasPermission("ck.admin"))
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_kits_left"), Color.red);
                    return;
                }
            }

            if (!AutoSave.ContainsKey(callr.CSteamID))
            {
                AutoSave.Add(callr.CSteamID, kitName);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autosave_on", kitName), Color.green);
            }
            else
            {
                AutoSave[callr.CSteamID] = kitName;
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autosave_on", kitName), Color.green);
            }
        }
    }
}