using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_GiveSlot : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "giveslot";

        public string Help => "Manually give players more slots to save their kits";

        public string Syntax => "<player> <amount> <item limit>";

        public List<string> Aliases => new List<string>() { };

        public List<string> Permissions => new List<string> { "ck.giveslot" };


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (command.Length < 3)
            {
                UnturnedChat.Say(caller, Syntax, Color.red);
                return;
            }

            var player = UnturnedPlayer.FromName(command[0]);
            int amount = int.Parse(command[1]);
            int limit = int.Parse(command[2]);

            if (player != null)
            {
                SlotManager.AddSlot(player, amount, limit);

                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("gave_slot", player.DisplayName, amount, limit));
                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("received_slot", caller.DisplayName, amount, limit));
            }
            else
            {
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("player_offline"), Color.red);
            }
        }
    }
}