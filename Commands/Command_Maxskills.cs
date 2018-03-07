using System.Collections.Generic;
using System;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_Maxskills : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "maxskills";

        public string Help => "Max out skills";

        public string Syntax => "[player]";

        public List<string> Aliases => new List<string>() { "skills" };

        public List<string> Permissions => new List<string> { "ck.maxskills" };

        public const string OTHER_PERM = "ck.maxskills.other";


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                if (caller is ConsolePlayer)
                {
                    Plugin.CustomKitsPlugin.Write("<player>", ConsoleColor.Red);
                    return;
                }

                callr.MaxSkills();

                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("max_skills"), Color.green);
            }
            else if (command.Length == 1)
            {
                if (caller.HasPermission(OTHER_PERM))
                {
                    UnturnedPlayer toPlayer = UnturnedPlayer.FromName(command[0]);

                    toPlayer.MaxSkills();

                    if (caller is ConsolePlayer)
                    {
                        Plugin.CustomKitsPlugin.Write(Plugin.CustomKitsPlugin.Instance.Translate("max_skills_other", toPlayer.CharacterName), ConsoleColor.Green);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("max_skills_other", toPlayer.CharacterName), Color.green);
                    }

                    UnturnedChat.Say(toPlayer, Plugin.CustomKitsPlugin.Instance.Translate("max_skills"), Color.green);
                }
                else
                {
                    UnturnedChat.Say(caller, "You do not have permissions to execute this command.", Color.red);
                }
            }
        }
    }
}