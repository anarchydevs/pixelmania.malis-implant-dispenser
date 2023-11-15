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
                    .Do("Shop Opened Event", IsTradeOpenEvent)
                    .Do("Give implant bag", GiveImplantBag)
                    .Do("Trade add item event", TradeAddItemEvent)
                    .Do("Confirm trade", ConfirmTrade)
                    .Do("Trade confirm event", TradeConfirmEvent)
                    .Do("Accept trade", TradeAccept)
                    .Do("Trade completed event", TradeCompletedEvent)
                    .Do("Complete order", CompleteOrder)
                    .Do("Clear unused bags", ClearUnusedBags)
                .End()
            .End()
            .Build();
        }

        private static BehaviourStatus TradeAddItemEvent(BotContext context)
        {
            if (EventTrigger.Status("TradeDecline") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Failed;

            if (EventTrigger.Status("TradeAddItem") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            return BehaviourStatus.Running;
        }


        private static BehaviourStatus TradeConfirmEvent(BotContext context)
        {
            if (EventTrigger.Status("TradeDecline") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Failed;

            if (EventTrigger.Status("TradeConfirm") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus IsTradeOpenEvent(BotContext c)
        {
            if (Trade.IsTrading)
            {
                if (Trade.CurrentTarget == c.TradeOrderTarget)
                {
                    Client.SendPrivateMessage(c.TradeOrderTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, "I have opened trade with you, if you don't see it please rezone."));
                    return BehaviourStatus.Succeeded;
                }
                else
                    return BehaviourStatus.Failed;
            }

            var tradeOpened = EventTrigger.Status("TradeOpened");
            var alreadyInTrade = EventTrigger.Status("AlreadyInTrade");

            if (tradeOpened == BehaviourStatus.Succeeded)
            {
                Client.SendPrivateMessage(c.TradeOrderTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, "I have opened trade with you, if you don't see it please rezone."));
                return BehaviourStatus.Succeeded;
            }
            else if (alreadyInTrade == BehaviourStatus.Succeeded)
            {
                Client.SendPrivateMessage(c.TradeOrderTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, $"You are already in a trade. Please cancel your current trade."));
                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus TradeCompletedEvent(BotContext c)
        {
            if (!OrderProcessor.Orders.TryGetValue(c.TradeOrderTarget.Instance, out _))
                return BehaviourStatus.Failed;

            if (EventTrigger.Status("TradeDecline") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Failed;

            if (EventTrigger.Status("TradeCompleted") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus AnyTradeTarget(BotContext c)
        {
            c.TradeOrderTarget = Identity.None;
            DynamicEvent.Reset();

            if (OrderProcessor.Orders.Count() == 0)
                return BehaviourStatus.Failed;

            if (Trade.IsTrading)
            {
                Client.SendPrivateMessage(Trade.CurrentTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, $"Please don't engage trades with me. If your order is complete, move very closely to me and wait for my trade request."));
                Trade.Decline();
            }

            foreach (Dynel dynel in DynelManager.Players)
            {
                int requester = dynel.Identity.Instance;

                if (!OrderProcessor.Orders.TryGetValue(requester, out Order order))
                    continue;

                if (!order.IsCompleted())
                    continue;

                if (dynel.DistanceFrom(DynelManager.LocalPlayer) > 2f)
                    continue;

                c.TradeOrderTarget = dynel.Identity;

                break;
            }

            return c.TradeOrderTarget != Identity.None ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        private static BehaviourStatus TradePlayer(BotContext c)
        {
            if (Trade.IsTrading && Trade.CurrentTarget != c.TradeOrderTarget)
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
            if (!Trade.IsTrading)
                return BehaviourStatus.Failed;

            if (!OrderProcessor.Orders.TryGetValue(c.TradeOrderTarget.Instance, out Order order))
                return BehaviourStatus.Failed;

            Item bag = Inventory.Items.FirstOrDefault(x => x.UniqueIdentity == order.BagIdentity);

            if (bag == null)
                return BehaviourStatus.Failed;

            Logger.Information($"Adding to trade: {bag.UniqueIdentity} {bag.Slot}");
            Client.SendPrivateMessage(c.TradeOrderTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, $"Inserting backpack to trade. If you don't see the backpack please rezone."));

            Trade.AddItem(bag.Slot);

            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus ConfirmTrade(BotContext c)
        {
            if (Trade.IsTrading && c.TradeOrderTarget != Trade.CurrentTarget || !Trade.IsTrading || !OrderProcessor.Orders.TryGetValue(Trade.CurrentTarget.Instance, out Order order))
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
            if (!Trade.IsTrading)
                return BehaviourStatus.Failed;

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