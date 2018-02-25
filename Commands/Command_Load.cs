using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_Load : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "load";

        public string Help => "Load one of your saved kits";

        public string Syntax => "[kit name]";

        public List<string> Aliases => new List<string>() { "loadkit" };

        public List<string> Permissions => new List<string> { "ck.load" };
        
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;
            string kitName = null;

            #region Kit Name
            if (Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName == "preset_name")
            {
                foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
                {
                    if (caller.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                    {
                        if (command.Length == 0)
                        {
                            kitName = Preset.Name;
                        }
                        else
                        {
                            kitName = command[0];
                        }
                    }
                }
            }
            else
            {
                if (command.Length == 0)
                {
                    kitName = Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName;
                }
                else
                {
                    kitName = command[0];
                }
            }
            #endregion

            if (!KitManager.HasSavedKits(callr, KitManager.Kits))
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), Color.red);
                return;
            }

            if (!KitManager.HasKit(callr, kitName, KitManager.Kits))
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_kit_exists"), Color.red);
                return;
            }

            InventoryManager.Clear(callr);
            KitManager.LoadKit(callr, callr, kitName, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothing, KitManager.Kits);
            
            UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_loaded", kitName), Color.green);
        }
    }
}