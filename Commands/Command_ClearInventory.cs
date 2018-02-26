using System.Collections.Generic;
using System;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_ClearInventory : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "clearinventory";

        public string Help => "Clear somebody's inventory";

        public string Syntax => "[player]";

        public List<string> Aliases => new List<string>() { "ci" };

        public List<string> Permissions => new List<string> { "ck.clearinventory" };

        public const string OTHER_PERM = "ck.clearinventory.other";
        public const string BYPASS_PERM = "ck.clearinventory.bypass";


        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write("<player>", ConsoleColor.Red);
                    return;
                }

                UnturnedPlayer callr = (UnturnedPlayer)caller;

                InventoryManager.Clear(callr);

                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("inventory_cleared"));
            }

            if (command.Length == 1)
            {
                UnturnedPlayer toPlayer = UnturnedPlayer.FromName(command[0]);
                
                if (toPlayer.HasPermission(BYPASS_PERM))
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("ci_bypass", toPlayer.CharacterName), Color.red);
                    return;
                }

                if (caller is ConsolePlayer)
                {
                    if (toPlayer == null)
                    {
                        Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", command[0]), ConsoleColor.Red);
                        return;
                    }

                    InventoryManager.Clear(toPlayer);

                    Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("inventory_cleared_other", toPlayer.CharacterName), ConsoleColor.Cyan);
                    return;
                }

                if (caller.HasPermission(OTHER_PERM))
                {
                    if (toPlayer == null)
                    {
                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", command[0]), Color.red);
                        return;
                    }

                    InventoryManager.Clear(toPlayer);

                    UnturnedChat.Say(toPlayer, Plugin.CustomKitsPlugin.Instance.Translate("inventory_cleared"));
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("inventory_cleared_other", toPlayer.CharacterName));
                }
                else
                {
                    UnturnedChat.Say(caller, "You do not have permissions to execute this command.", Color.red);
                }
            }
        }
    }
}