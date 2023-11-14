using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using static MalisImpDispenser.AOSetupsPreset;
using AOSharp.Clientless.Logging;
using AOSharp.Clientless;
using AOSharp.Common.GameData;
using MalisItemFinder;

namespace MalisImpDispenser
{
    public static class OrderProcessor
    {
        internal static ImplantPreset CurrentOrder;
        internal static Queue<ImplantPreset> ActiveOrders = new Queue<ImplantPreset>();
        internal static Dictionary<int, Order> Orders = new Dictionary<int, Order>();
        private static AutoResetInterval _updateLoop = new AutoResetInterval(2000);
        internal static bool HasOrders() => ActiveOrders.Count > 0;

        internal static void ProcessOrder()
        {
            if (ActiveOrders.Count == 0)
            {
                Logger.Information("Out of orders");
                return;
            }

            CurrentOrder = ActiveOrders.Dequeue();
        }

        internal static void UpdateLoop(object sender, double delta)
        {
            try
            {
                if (!_updateLoop.Elapsed)
                    return;

                foreach (var order in Orders.Where(x=>x.Value.IsCompleted()).ToDictionary(x => x.Key, y => y.Value))
                    order.Value.Tick(2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void TryRemoveEntry(int charId, bool shouldBlacklist = false)
        {
            BagOwners.TryRemoveOwner(charId, out Identity bagIdentity);

            if (!Orders.TryGetValue(charId, out Order order))
                return;

            if (shouldBlacklist)
            {
                if (Trade.IsTrading)
                    Trade.Decline();

                Container bag = Inventory.Containers.FirstOrDefault(x => x.Identity == bagIdentity);

                foreach (var item in bag.Items)
                    item.Delete();

                // Main.Settings.Blacklist.Add(charId, order.TotalCredits);
                Main.Settings.Save();

                Client.SendPrivateMessage(charId, ScriptTemplate.RespondMsg(Color.Red, $"Order expired, if something went wrong, please provide feedback to the host in order to get it resolved for the future. Thanks for your patience."));

                // Client.SendPrivateMessage(charId, ScriptTemplate.RespondMsg(Color.Red, $"Order expired, you have been blacklisted. You can trade me your debt of {Main.Settings.Blacklist[charId]} credits to get removed."));
            }

            Orders.Remove(charId);
        }

        internal static Order GetCurrentOrder() => Orders[CurrentOrder.Requester];

        internal static void SetOrder(int requesterId, IEnumerable<AOSImplantData> preset)
        {
            foreach (AOSImplantData implant in preset)
            {
                if (implant.type == "symbiant" || implant.clusters == null)
                    continue;

                SetOrder(ImplantDesigner.MakePreset(requesterId, implant.ql, implant.GetSlot(), implant.clusters.Select(x => new ClusterItem { Type = x.Key, Stat = implant.GetCluster(x.Value.ClusterID) }).ToList()));
            }
        }

        internal static void SetOrder(ImplantPreset preset)
        {
            if (preset == null)
                return;

            if (IsBlacklisted(preset.Requester))
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, $"Request rejected (due to blacklist). You can trade me your debt of {Main.Settings.Blacklist[preset.Requester]} to get removed"));
                return;
            }

            if (preset.GetClusters().Count(x => !x.IsTrickle) == 0)
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, $"Skipping '{preset.GetImplant().Slot}' due to no valid clusters."));
                return;
            }

            if (preset.GetImplant().Ql < 50 && preset.GetImplant().Ql % 10 != 0)
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, $"Skipping '{preset.GetImplant().Slot}' due to being unable to trickle implants below ql 50."));
                return;
            }

            if (!BagOwners.HasAvailableBags)
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, "Queue is full. Please try again later."));
                return;
            }

            if (Orders.Count() == Inventory.Items.Where(x => x.UniqueIdentity.Type == IdentityType.Container).Count())
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, "Global order limit reached. Please try again later."));
                return;
            }

            if (!Orders.TryGetValue(preset.Requester, out Order order))
            {
                order = new Order(preset.Requester);
                Orders.Add(preset.Requester, order);
            }

            if (!order.CanOrder())
            {
                Client.SendPrivateMessage(preset.Requester, ScriptTemplate.RespondMsg(Color.Red, $"Order limit of {order.Limit} reached."));
                return;
            }

            order.TotalCredits += preset.GetTotalPrice();
            ActiveOrders.Enqueue(preset);
            Orders[preset.Requester].ImplantPresets.Add(preset);
        }

        internal static bool IsBlacklisted(int charId) => Main.Settings.Blacklist.Keys.Contains(charId);
    }
}