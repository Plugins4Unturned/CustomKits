using System.Collections.Generic;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using Steamworks;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_AutoLoad : IRocketCommand
    {
        public static Dictionary<CSteamID, string> AutoLoad = new Dictionary<CSteamID, string>();
        
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "autoload";

        public string Help => "Auto-load any of your kits when you die";

        public string Syntax => "<kit name>";

        public List<string> Aliases => new List<string>() { "aload" };

        public List<string> Permissions => new List<string> { "ck.autoload" };
        
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;
            string kitName = null;

            if (command.Length == 0)
            {
                if (!AutoLoad.ContainsKey(callr.CSteamID))
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
                    AutoLoad.Remove(callr.CSteamID);
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autoload_off"), Color.green);
                    return;
                }
            }
            else
            {
                kitName = command[0];
            }
            
            if (!KitManager.HasKit(callr, kitName, KitManager.Kits))
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_kit_exists"), Color.red);
                return;
            }

            if (!AutoLoad.ContainsKey(callr.CSteamID))
            {
                AutoLoad.Add(callr.CSteamID, kitName);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autoload_on", kitName), Color.green);
            }
            else
            {
                AutoLoad[callr.CSteamID] = kitName;
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autoload_on", kitName), Color.green);
            }
        }
    }
}