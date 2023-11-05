using BehaviourTree.FluentBuilder;
using BehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Common.Helpers;
using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using SmokeLounge.AOtomation.Messaging.Messages.SystemMessages;

namespace MalisImpDispenser
{
    internal static class TradeOrderBehavior
    {
        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
            .UntilFailed("Trade Until No Targets Left")
                .Sequence($"Trade Player Sequence")
                    .Do("Any Trade Targets Available?", AnyTradeTarget)
                    .Do("Trade player", TradePlayer)
                    .Do("Shop Opened Event", c => EventTrigger.Status("TradeOpened"))
                    .Do("Give implant bag", GiveImplantBag)
                    .Do("Trade add item event", c => EventTrigger.Status("TradeAddItem"))
                    .Do("Confirm trade", ConfirmTrade)
                    .Do("Trade confirm event", c => EventTrigger.Status("TradeConfirm"))
                    .Do("Accept trade", TradeAccept)
                    .Do("Accept trade", TradeCompleted)
                    .Do("Complete order", CompleteOrder)
                    .Do("Clear unused bags", ClearUnusedBags)
                .End()
            .End()
            .Build();
        }

        private static BehaviourStatus TradeCompleted(BotContext c)
        {
            if (!OrderProcessor.Orders.TryGetValue(c.TradeOrderTarget.Instance, out Order order))
                return BehaviourStatus.Failed;

            return EventTrigger.Status("TradeCompleted");
        }

        private static BehaviourStatus AnyTradeTarget(BotContext c)
        {
            c.TradeOrderTarget = Identity.None;

            if (OrderProcessor.Orders.Count() == 0)
                return BehaviourStatus.Failed;

            foreach (Dynel dynel in DynelManager.Players)
            {
                int requester = dynel.Identity.Instance;

                if (!OrderProcessor.Orders.TryGetValue(requester, out Order order))
                    continue;

                if (!order.IsCompleted())
                    continue;

                if (dynel.DistanceFrom(DynelManager.LocalPlayer) > 2f)
                    continue;

                Logger.Information($"Trade target '{requester}' acquired, timing out in 30 seconds");
                c.TradeOrderTarget = dynel.Identity;

                break;
            }

            return c.TradeOrderTarget != Identity.None ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        private static BehaviourStatus TradePlayer(BotContext c)
        {
            if (Trade.IsTrading)
            {
                Trade.Decline();
                return BehaviourStatus.Failed;
            }

            Trade.Open(c.TradeOrderTarget);
            Logger.Debug($"Opening trade with {c.TradeOrderTarget}");

            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus GiveImplantBag(BotContext c)
        {
            if (!OrderProcessor.Orders.TryGetValue(c.TradeOrderTarget.Instance, out Order order))
                return BehaviourStatus.Failed;

            Item bag = Inventory.Items.FirstOrDefault(x => x.UniqueIdentity == order.BagIdentity);

            if (bag == null)
                return BehaviourStatus.Failed;

            Logger.Information($"Adding to trade: {bag.UniqueIdentity} {bag.Slot}");
            Trade.AddItem(bag.Slot);

            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus ConfirmTrade(BotContext c)
        {
            if (!Trade.IsTrading || !OrderProcessor.Orders.TryGetValue(Trade.CurrentTarget.Instance, out Order order))
                return BehaviourStatus.Failed;

            bool itemCheck = Trade.TargetWindowCache.Items.Count == 1 && 
                             Trade.TargetWindowCache.Items[0].UniqueIdentity.Type == IdentityType.Container &&
                             !UniqueContainerIds.Contains(Trade.TargetWindowCache.Items[0].Id);
            bool credCheck = order.TotalCredits == Trade.TargetWindowCache.Credits;
            bool tradeAcceptReq = itemCheck && credCheck;

            bool shouldAcceptTrade = Trade.Status == TradeStatus.Accept && tradeAcceptReq || Trade.Status == TradeStatus.None && tradeAcceptReq;

            if (shouldAcceptTrade)
            {
                Trade.Confirm();
                return BehaviourStatus.Succeeded;
            }

            if (Trade.Status == TradeStatus.Accept)
            {
                string respondMsg = "\nDeclining trade due to:";

                if (!itemCheck)
                    respondMsg += $"\n  - Missing container or container is unique";

                if (!credCheck)
                    respondMsg += $"\n  - Credits required: {order.TotalCredits}";

                Client.SendPrivateMessage(c.TradeOrderTarget.Instance, ScriptTemplate.RespondMsg(Color.Red, respondMsg));
                Trade.Decline();

                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus TradeAccept(BotContext c)
        {
            if (!OrderProcessor.Orders.ContainsKey(c.TradeOrderTarget.Instance))
                return BehaviourStatus.Failed;

            Trade.Accept();
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus CompleteOrder(BotContext c)
        {
            if (!OrderProcessor.Orders.ContainsKey(c.TradeOrderTarget.Instance))
                return BehaviourStatus.Failed;

            OrderProcessor.Orders.Remove(c.TradeOrderTarget.Instance);
            BagOwners.TryRemoveOwner(c.TradeOrderTarget.Instance, out _);
            Utils.OpenBags();

            return BehaviourStatus.Succeeded;
        }


        private static BehaviourStatus ClearUnusedBags(BotContext context)
        {
            if (Inventory.Containers.Any(x => !x.IsOpen))
                return BehaviourStatus.Running;

            var unusedBags = Inventory.Containers.Where(bags => !BagOwners.UsedBagIdentities().Contains(bags.Identity)).ToList();

            if (unusedBags.Count() != 0)
            {
                foreach (var bag in unusedBags)
                {
                    foreach (var item in bag.Items)
                        item.Delete();
                }
            }

            return BehaviourStatus.Succeeded;
        }
    }
}