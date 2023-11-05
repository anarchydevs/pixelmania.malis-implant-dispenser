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

namespace MalisImpDispenser
{
    internal static class TradeDebtBehavior
    {
        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
            .UntilFailed("Trade Until No Targets Left")
                .Sequence($"Trade Player Sequence")
                    .Do("Any Trade Targets Available?", AnyTradeTarget)
                    .Do("Trade player", TradePlayer)
                    .Do("Shop Opened Event", c => EventTrigger.Status("TradeOpened"))
                    .Do("Confirm trade", ConfirmTrade)
                    .Do("Trade confirm event", c => EventTrigger.Status("TradeConfirm"))
                    .Do("Accept trade", TradeAccept)
                    .Do("Completed trade event", c => EventTrigger.Status("TradeCompleted"))
                    .Do("Complete order", CompleteOrder)
                .End()
            .End()
            .Build();
        }

        private static BehaviourStatus AnyTradeTarget(BotContext c)
        {
            if (OrderProcessor.Orders.Count(x=>x.Value.Completed != x.Value.Total)!= 0)
                return BehaviourStatus.Failed;

            c.TradeDebtTarget = Identity.None;

            foreach (Dynel dynel in DynelManager.Players)
            {
                int requester = dynel.Identity.Instance;

                if (!Main.Settings.Blacklist.ContainsKey(requester))
                    continue;

                if (dynel.DistanceFrom(DynelManager.LocalPlayer) > 2f)
                    continue;

                Logger.Information($"Trade target '{requester}' acquired.");
                c.TradeDebtTarget = dynel.Identity;

                break;
            }

            return c.TradeDebtTarget != Identity.None ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        private static BehaviourStatus TradePlayer(BotContext c)
        {
            if (Trade.IsTrading)
            {
                Trade.Decline();
                return BehaviourStatus.Failed;
            }

            Trade.Open(c.TradeDebtTarget);

            Logger.Debug($"Opening trade with {c.TradeDebtTarget}");
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus ConfirmTrade(BotContext c)
        {
            if (OrderProcessor.Orders.Count(x => x.Value.Completed != x.Value.Total) != 0)
            {
                if (Trade.IsTrading)
                    return BehaviourStatus.Failed;
            }

            if (!Trade.IsTrading || !Main.Settings.Blacklist.TryGetValue(Trade.CurrentTarget.Instance, out int debt))
                return BehaviourStatus.Failed;

            bool itemCheck = Trade.TargetWindowCache.Items.Count == 0;
            bool credCheck = debt == Trade.TargetWindowCache.Credits;
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
                    respondMsg += $"\n  - Trade must not contain any items";

                if (!credCheck)
                    respondMsg += $"\n  - Credits required: {debt}";

                Client.SendPrivateMessage(c.TradeDebtTarget.Instance, ScriptTemplate.RespondMsg(Color.Red, respondMsg));
                Trade.Decline();

                return BehaviourStatus.Failed;
            }

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus TradeAccept(BotContext c)
        {
            Trade.Accept();
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus CompleteOrder(BotContext c)
        {
            Main.Settings.Blacklist.Remove(c.TradeDebtTarget.Instance);
            Main.Settings.Save();
            return BehaviourStatus.Succeeded;
        }
    }
}