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
    public static class StatusCommand
    {
        internal static void Process(StatusCmd cmd)
        {
            if (!OrderProcessor.Orders.TryGetValue(cmd.RequesterId, out Order order))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Orange, "You don't have an active order."));
                return;
            }

            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(order));
        }
    }
}   