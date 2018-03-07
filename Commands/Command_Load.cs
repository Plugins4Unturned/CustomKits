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
            string kitName = Plugin.CustomKitsPlugin.Instance.Configuration.Instance.DefaultKitName;
            
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

            InventoryManager.Clear(callr, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits);
            KitManager.LoadKit(callr, callr, kitName, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits, KitManager.Kits);
            
            UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_loaded", kitName), Color.green);
        }
    }
}