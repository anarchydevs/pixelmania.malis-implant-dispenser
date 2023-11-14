using AOSharp.Clientless;
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
            string inventory = "";

            foreach (var item in Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory))
                inventory += $"'{item.Name}' '{item.Slot}'" + Environment.NewLine;

            inventory += $"Total: {Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory).Count()}";

            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Orange, inventory));
        }
    }
}   