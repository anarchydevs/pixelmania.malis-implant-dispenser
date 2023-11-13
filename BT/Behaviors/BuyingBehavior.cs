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
    internal static class BuyingBehavior
    {
        private static AutoResetInterval _internalReset = new AutoResetInterval(5000);

        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
                .UntilSuccess("Buying Loop")
                    .Sequence($"Buying Sequence")
                        .Subtree(TryToOpenShop())
                        .Do("Buy Item", BuyItem)
                        .Do("Item Bought Event", TradeCompletedEvent)
                        .Do("Check Shop List", CheckShopList)
                    .End()
                .End()
                .Build();
        }

        private static BehaviourStatus TradeCompletedEvent(BotContext context)
        {
            return EventTrigger.Status("TradeCompleted");
        }

        internal static IBehaviour<BotContext> TryToOpenShop()
        {
            return FluentBuilder.Create<BotContext>()
                .UntilSuccess("Open Trade Loop")
                    .Sequence("Open Shop Sequence")
                        .Do("Open Shop", OpenShop)
                        .Do("Shop Opened Event", ShopOpenedEvent)
                    .End()
                .End()
                .Build();
        }

        private static BehaviourStatus ShopOpenedEvent(BotContext c)
        {
            if (Trade.IsTrading)
                return Trade.CurrentTarget == IdentityType.VendingMachine ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;

            return _internalReset.Elapsed ? BehaviourStatus.Failed : BehaviourStatus.Running;
        }

        private static BehaviourStatus CheckShopList(BotContext c)
        {
            var isShopListEmpty = OrderProcessor.CurrentOrder.ShopListEmpty? BehaviourStatus.Succeeded: BehaviourStatus.Failed;

            if (isShopListEmpty == BehaviourStatus.Succeeded)
                DynamicEvent.Reset();

            return isShopListEmpty;
        }

        private static BehaviourStatus OpenShop(BotContext c)
        {
            CoreItem baseItem = OrderProcessor.CurrentOrder.GetNextShopItem();

            DynamicEvent.Reset();
            _internalReset.Reset();

            if (Trade.IsTrading)
            {
                Trade.Decline();
                Client.SendPrivateMessage(Trade.CurrentTarget.Instance, ScriptTemplate.RespondMsg(Color.Orange, $"Please don't engage trades with me. I am currently finishing a order. If your order is complete, please wait patiently."));
                return BehaviourStatus.Running;
            }

            c.ActiveItem = new ActiveItem
            {
                ShopItem = ShopCache.FindItem(baseItem),
                BaseItem = baseItem,
            };

            if (c.ActiveItem.ShopItem == null)
            {
                Logger.Information("Shop item doesn't exist in ShopCache.");
                return BehaviourStatus.Failed;
            }

            Logger.Information($"Opening shop: {c.ActiveItem.ShopItem.ShopIdentity}");
            Action.OpenShop(c.ActiveItem.ShopItem.ShopIdentity);
        
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus BuyItem(BotContext c)
        {
            var buyAmount = c.ActiveItem.BaseItem is ClusterItem cluster && cluster.IsTrickle ? OrderProcessor.CurrentOrder.GetClusters().Where(x => x.IsTrickle).Count() : 1;
         
            for (int i = 0; i < buyAmount; i++)
            {
                Trade.AddItem(c.ActiveItem.ShopItem.ItemIndex);
                OrderProcessor.CurrentOrder.GetNextShopItem().Purchased = true;
            }

            Trade.Accept();

            return BehaviourStatus.Succeeded;
        }
    }
}