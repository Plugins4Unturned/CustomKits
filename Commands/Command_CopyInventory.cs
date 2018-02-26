using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_CopyInventory : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "copyinventory";

        public string Help => "Copy someone else's inventory to your own";

        public string Syntax => "<player>";

        public List<string> Aliases => new List<string>() { "copy" };

        public List<string> Permissions => new List<string> { "ck.copyinventory" };

        public const string OTHER_PERM = "ck.copyinventory.other";
        public const string BYPASS_PERM = "ck.copyinventory.bypass";


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, Syntax, Color.red);
                return;
            }
            else
            {
                if (caller.HasPermission(OTHER_PERM))
                {
                    UnturnedPlayer fromPlayer = UnturnedPlayer.FromName(command[0]);
                    
                    if (fromPlayer.HasPermission(BYPASS_PERM))
                    {
                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("copy_bypass", fromPlayer.CharacterName), Color.red);
                        return;
                    }
                    else
                    {
                        InventoryManager.Copy(fromPlayer, callr, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothing);

                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("copied", fromPlayer.CharacterName), Color.green);
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "You do not have permissions to execute this command.", Color.red);
                }
            }
        }
    }
}