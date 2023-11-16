using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Clientless.Net;
using AOSharp.Common.GameData;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisImpDispenser
{
    public static class DebugCommand
    {
        internal static void Process(CommandBase cmd)
        {
            string debugMsg = "";

            foreach (var item in Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory))
                debugMsg += $"'{item.Name}' '{item.Slot}'" + Environment.NewLine;

            debugMsg += $"Total: {Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory).Count()}" + Environment.NewLine;
            debugMsg += $"Tcp client game server connection: {Client.Connected}";

            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Orange, debugMsg));
            DynelManager.LocalPlayer.MovementComponent.ChangeMovement(MovementAction.LeaveSit);
        }
    }
}   