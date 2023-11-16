using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
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
    public static class AOSetupsCommand
    {
        internal static void Process(AoSetupsCmd cmd)
        {
            if (!AOSetupsPreset.CanProcess)
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "I was busy processing another AOSetup. Ask me again."));
                return;
            }

            AOSetupsPreset.CanProcess = false;

            AOSetupsPreset.GetImplants(cmd.Url).ContinueWith(implants =>
            {
                if (implants.Result == null)
                {
                    AOSetupsPreset.CanProcess = true;
                    Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Invalid url or I can't access the site right now."));
                    return;
                }

                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Orange, "Order accepted and added to queue. I will update you as I progress your order"));
                OrderProcessor.SetOrder(cmd.RequesterId, implants.Result);

                if (OrderProcessor.Orders.ContainsKey(cmd.RequesterId))
                    Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(OrderProcessor.Orders[cmd.RequesterId]), false);
            });
        }
    }
}   