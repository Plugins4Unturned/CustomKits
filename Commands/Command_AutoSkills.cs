using System.Collections.Generic;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using Steamworks;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_AutoSkills : IRocketCommand
    {
        public static List<CSteamID> AutoSkills = new List<CSteamID>();
        
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "autoskills";

        public string Help => "Toggle Auto-maxskills";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string>() { "askills" };

        public List<string> Permissions => new List<string> { "ck.autoskills" };
        

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (AutoSkills.Contains(callr.CSteamID))
            {
                AutoSkills.Remove(callr.CSteamID);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autoskills_off"), Color.green);
            }
            else
            {
                AutoSkills.Add(callr.CSteamID);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autoskills_on"), Color.green);
            }
        }
    }
}