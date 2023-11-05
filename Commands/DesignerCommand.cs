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
    public static class DesignerCommand
    {
        internal static void Process(DesignerCmd cmd)
        {
            bool sendFeedback;

            if (cmd == null)
                return;

            if (cmd is DesignerShowCmd showCmd)
            {
                sendFeedback = DesignShowRequest(showCmd, out _);
            }
            else if (cmd is DesignerCreateCmd createCmd)
            {
                sendFeedback = DesignCreateRequest(createCmd);
            }
            else if (cmd is DesignerModifyClusterCmd modifyCmd)
            {
                sendFeedback = DesignModifyClusterRequest(modifyCmd);
            }
            else if (cmd is DesignerModifyQlCmd modifyQlCmd)
            {
                sendFeedback = DesignModifyQlRequest(modifyQlCmd);
            }
            else if (cmd is DesignerRemoveCmd removeCmd)
            {
                sendFeedback = DesignRemoveRequest(removeCmd);
            }
           else if (cmd is DesignerOrderCmd orderCmd)
            {
                sendFeedback = DesignOrderRequest(orderCmd);
            }
            else
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.ImplantPreview(cmd.Index, ImplantDesigner.Previews[cmd.RequesterId], "Modify Template"));
                sendFeedback = false;
            }

            if (!sendFeedback)
                return;

            if (sendFeedback && ImplantDesigner.Previews[cmd.RequesterId] != null)
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(ImplantDesigner.Previews[cmd.RequesterId], "Designer Preview", true));
        }

        private static bool DesignOrderRequest(DesignerOrderCmd cmd)
        {
            if (!ImplantDesigner.Previews.TryGetValue(cmd.RequesterId, out Order order ))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Order placement failed, designer is empty."));
            }
            else if (order.ImplantPresets.Any(x => x.GetClusters().Count(c => !c.IsTrickle) == 0))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Order placement failed, Each implant needs to have at least one valid cluster."));
            }
            else
            {
                if (Main.Settings.Blacklist.ContainsKey(cmd.RequesterId))
                {
                    Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, $"Request rejected (due to blacklist). You can trade me your debt of {Main.Settings.Blacklist[cmd.RequesterId]} to get removed"));
                }
                else
                {
                    foreach (var preset in order.ImplantPresets)
                    {
                        OrderProcessor.SetOrder(ImplantDesigner.MakePreset(cmd.RequesterId,
                              preset.GetImplant().Ql,
                              preset.GetImplant().Slot,
                              preset.GetClusters().Where(x => !x.IsTrickle).ToList()));
                    }

                    ImplantDesigner.Previews.Remove(cmd.RequesterId);
                }
            }


            return false;
        }

        private static bool DesignShowRequest(DesignerCmd cmd, out Order preview)
        {
            if (!ImplantDesigner.Previews.TryGetValue(cmd.RequesterId, out preview))
            {
                preview = new Order(cmd.RequesterId);
                ImplantDesigner.Previews[cmd.RequesterId] = preview;
            }

            return true;
        }

        private static bool DesignRemoveRequest(DesignerRemoveCmd cmd)
        {
            if (!ImplantDesigner.Previews.TryGetValue(cmd.RequesterId, out Order order))
                return false;

            if (order.ImplantPresets.Count < cmd.Index)
                return false;

            var preset = order.ImplantPresets.FirstOrDefault(x => x.Index == cmd.Index);
            order.ImplantPresets.Remove(preset);

            return true;
        }

        private static bool DesignModifyQlRequest(DesignerModifyQlCmd cmd)
        {
            if (!ImplantDesigner.Previews.TryGetValue(cmd.RequesterId, out Order order))
                return false;

            if (!ImplantDesigner.IsQlValid(cmd.Ql))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Ql Error: Invalid range (valid range: 10-200)."));
                return false;
            }

            ImplantPreset preset = order.ImplantPresets.FirstOrDefault(x => x.Index == cmd.Index);

            if (preset == null)
                return false;

            preset.GetImplant().Ql = cmd.Ql;

            return true;
        }

        private static bool DesignModifyClusterRequest(DesignerModifyClusterCmd cmd)
        {
            if (!ImplantDesigner.Previews.TryGetValue(cmd.RequesterId, out Order order))
                return false;

            ImplantPreset preset = order.ImplantPresets.FirstOrDefault(x => x.Index == cmd.Index);

            if (preset == null)
                return false;

            if (!ImplantDesigner.TryFindCluster(preset.GetImplant().Slot, cmd.ClusterName, out ClusterItem clusterItem))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Invalid cluster."));
                return false;
            }

            preset.ReplaceCluster(clusterItem.Type, clusterItem);

            return true;
        }

        private static bool DesignCreateRequest(DesignerCreateCmd cmd)
        {
            if (cmd.Slot == null || !ImplantDesigner.TryGetImplant(cmd.Slot, out ImplantSlot impSlot))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Implant Slot Error: Invalid implant slot."));
                return false;
            }

            DesignShowRequest(cmd, out Order preview);

            var firstAvailableIndex = 0;

            if (preview.ImplantPresets.Count() > 0)
                firstAvailableIndex = Enumerable.Range(0, preview.ImplantPresets.Count() + 1).Except(preview.ImplantPresets.Select(x => x.Index)).FirstOrDefault();

            var preset = ImplantDesigner.MakePreset(cmd.RequesterId, 200, impSlot, new List<ClusterItem>());
            preview.ImplantPresets.Add(preset);
            preset.Index = firstAvailableIndex;

            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.ImplantPreview(preset.Index, ImplantDesigner.Previews[cmd.RequesterId], $"{impSlot.GetDescription()} Template"));

            return true;
        }
    }
}   