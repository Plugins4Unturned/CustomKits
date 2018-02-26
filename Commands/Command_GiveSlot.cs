using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_GiveSlot : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "giveslot";

        public string Help => "Manually give players more slots to save their kits";

        public string Syntax => "<player> <amount> <item limit>";

        public List<string> Aliases => new List<string>() { };

        public List<string> Permissions => new List<string> { "ck.giveslot" };


        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 3)
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write(Syntax, System.ConsoleColor.Red);
                    return;
                }
                else
                {
                    UnturnedChat.Say(caller, Syntax, Color.red);
                    return;
                }
            }

            var player = UnturnedPlayer.FromName(command[0]);
            int amount = int.Parse(command[1]);
            int limit = int.Parse(command[2]);

            if (player != null)
            {
                SlotManager.AddSlot(player, amount, limit);

                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("gave_slot", player.DisplayName, amount, limit), System.ConsoleColor.Green);
                }
                else
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("gave_slot", player.DisplayName, amount, limit));
                }

                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("received_slot", caller.DisplayName, amount, limit));
            }
            else
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("player_offline"), System.ConsoleColor.Red);
                }
                else
                {
                    UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("player_offline"), Color.red);
                }
            }
        }
    }
}