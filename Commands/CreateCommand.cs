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
    public static class CreateCommand
    {
        internal static void Process(CommandBase cmd)
        {
            if (cmd is TradeSkillCmd tsCmd)
            {
                ProcessCreateCmd(tsCmd);
            }
            else if (cmd is TradeskillBestCmd tsBestCmd)
            {
                ProcessCreateBestCmd(tsBestCmd);
            }
        }

        private static void ProcessCreateCmd(TradeSkillCmd cmd)
        {
            if (!ImplantDesigner.IsQlValid(cmd.Ql))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Ql Error: Invalid range (valid range: 10-200)."));
                return;
            }

            if (!ImplantDesigner.TryGetImplant(cmd.Implant, out ImplantSlot impSlot))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Implant Slot Error: Invalid implant slot."));
                return;
            }

            if (!ImplantDesigner.TryGetClusterPreset(impSlot, cmd.Clusters, out List<ClusterItem> clusters))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Cluster Error: Invalid cluster(s)."));
                return;
            }

            OrderProcessor.SetOrder(ImplantDesigner.MakePreset(cmd.RequesterId, cmd.Ql, impSlot, clusters));

            if (OrderProcessor.Orders.ContainsKey(cmd.RequesterId))
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(OrderProcessor.Orders[cmd.RequesterId]), false);
        }

        private static void ProcessCreateBestCmd(TradeskillBestCmd cmd)
        {
            if (!ImplantDesigner.TryGetImplant(cmd.Implant, out ImplantSlot impSlot))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Implant Slot Error: Invalid implant slot."));
                return;
            }

            if (!ImplantDesigner.TryGetClusterPreset(impSlot, cmd.Clusters, out List<ClusterItem> clusters))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Cluster Error: Invalid cluster(s)."));
                return;
            }

            if (!ImplantDesigner.GetBestQl(cmd.Ability, cmd.Treatment, clusters, cmd.StarIndex, out int ql, out string errorMsg))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, errorMsg));
                return;
            }

            ImplantPreset preset = ImplantDesigner.MakePreset(cmd.RequesterId, ql, impSlot, clusters);

            OrderProcessor.SetOrder(preset);
            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(OrderProcessor.Orders[cmd.RequesterId]));
        }
    }
}   