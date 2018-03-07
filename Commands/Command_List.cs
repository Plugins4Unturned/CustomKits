using System.Collections.Generic;
using System.Linq;
using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_List : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "list";

        public string Help => "List saved kits";

        public string Syntax => "[player]";

        public List<string> Aliases => new List<string>() { "listkits" };

        public List<string> Permissions => new List<string> { "ck.list" };

        public const string OTHER_PERM = "ck.list.other";


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = null;

            if (command.Length == 0)
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write("<player>", ConsoleColor.Red);
                    return;
                }

                player = (UnturnedPlayer)caller;
            }
            else if (command.Length == 1)
            {
                if (caller.HasPermission(OTHER_PERM))
                {
                    player = UnturnedPlayer.FromName(command[0]);
                }
                else
                {
                    UnturnedChat.Say(caller, "You do not have permissions to execute this command.", Color.red);
                    return;
                }
            }

            if (player == null)
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", command[0]), ConsoleColor.Red);
                    return;
                }

                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("player_doesn't_exist", command[0]), Color.red);
                return;
            }

            if (KitManager.Kits.ContainsKey(player.CSteamID.m_SteamID))
            {
                if (KitManager.KitCount(player, KitManager.Kits) > 1)
                {
                    string kitList = string.Join(", ", KitManager.Kits[player.CSteamID.m_SteamID].Keys.ToArray());

                    if (caller is ConsolePlayer)
                    {
                        Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("kit_list", kitList), ConsoleColor.Green);
                        return;
                    }

                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_list", kitList), Color.green);
                }
                else if (KitManager.KitCount(player, KitManager.Kits) == 1)
                {
                    foreach (var kit in KitManager.Kits[player.CSteamID.m_SteamID].Keys)
                    {
                        if (caller is ConsolePlayer)
                        {
                            Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("kit_list", kit), ConsoleColor.Green);
                            return;
                        }

                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("kit_list", kit), Color.green);
                    }
                }
                else
                {
                    if (caller is ConsolePlayer)
                    {
                        Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), ConsoleColor.Red);
                        return;
                    }

                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), Color.red);
                }
            }
            else
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), ConsoleColor.Red);
                    return;
                }

                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("no_saved_kits"), Color.red);
            }
        }
    }
}