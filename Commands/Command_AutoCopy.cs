using System.Collections.Generic;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.API;
using Steamworks;
using UnityEngine;

namespace Teyhota.CustomKits.Commands
{
    public class Command_AutoCopy : IRocketCommand
    {
        public static List<CSteamID> AutoCopy = new List<CSteamID>();
        public static Dictionary<CSteamID, CSteamID> Murdered = new Dictionary<CSteamID, CSteamID>();
        
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "autocopy";

        public string Help => "Toggle Auto-copy";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string>() { "acopy" };

        public List<string> Permissions => new List<string> { "ck.autocopy" };
        

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer callr = (UnturnedPlayer)caller;

            if (AutoCopy.Contains(callr.CSteamID))
            {
                AutoCopy.Remove(callr.CSteamID);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autocopy_off"), Color.green);
            }
            else
            {
                AutoCopy.Add(callr.CSteamID);
                UnturnedChat.Say(caller, Plugin.CustomKitsPlugin.Instance.Translate("autocopy_on"), Color.green);
            }
        }
    }
}