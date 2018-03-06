using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using Steamworks;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_Delete : IRocketCommand
    {
        public static List<CSteamID> Yes = new List<CSteamID>();
        
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "delete";

        public string Help => "Delete your saved kits";

        public string Syntax => "[kit name / *]";

        public List<string> Aliases => new List<string>() { "del" };

        public List<string> Permissions => new List<string> { "ck.delete" };

        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;
            string kitName = Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName;

            if (!KitManager.HasSavedKits(callr, KitManager.Kits))
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), Color.red);
                return;
            }

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

            if (command.Length == 1)
            {
                kitName = command[0];
            }

            if (kitName == "*")
            {
                if (KitManager.HasSavedKits(callr, KitManager.Kits))
                {
                    UnturnedChat.Say(callr, Plugin.CustomKitsPlugin.Instance.Translate("are_you_sure"), Color.yellow);

                    if (!Yes.Contains(callr.CSteamID))
                    {
                        Yes.Add(callr.CSteamID);
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), Color.red);
                    return;
                }
            }

            if (KitManager.HasKit(callr, kitName, KitManager.Kits))
            {
                KitManager.DeleteKit(callr, kitName, KitManager.Kits);
            }
            else
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_kit_exists"), Color.red);
                return;
            }
            
            UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_deleted", kitName), Color.green);
        }
    }
}