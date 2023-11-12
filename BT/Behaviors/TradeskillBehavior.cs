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
using SmokeLounge.AOtomation.Messaging.GameData;

namespace MalisImpDispenser
{
    internal static class TradeskillBehavior
    {
        private static AutoResetInterval _redoTimer = new AutoResetInterval(10000);

        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
                .UntilFailed("Tradeskill Loop")
                    .Sequence("Tradeskill Sequence")
                        .Do("Get Next Clusters", GetNextClusters)
                        .Selector("Pick tradeskill action")
                            .Sequence("TradeSkill Sequence 1")
                                .Condition("Tradeskill Condition 1", CleanImplantCondition1)
                                .Do("Clean Implant", CleanImplant)
                                .Do("Item Deleted Event", ItemDeleteEvent)
                            .End()
                            .Sequence("TradeSkill Sequence 2")
                                .Condition("Tradeskill Condition 2", TradeSkillCondition2)
                                .Do("Tradeskill Source", TradeskillChangeSource)
                                .Do("Tradeskill Source Event", TradeskillSourceEvent)
                                .Do("Tradeskill Target", TradeskillChangeTarget)
                                .Do("Tradeskill Target Event", TradeskillTargetEvent)
                                .Do("Tradeskill Result Event", TradeskillResultEvent)
                                .Do("Tradeskill Build", TradeskillImplant)
                                .Do("Item Deleted Event", ItemTradeskillEvent)
                            .End()
                            .Sequence("TradeSkill Sequence 3")
                                .Condition("Clean Implant Condition 2", CleanImplantCondition2)
                                .Do("Clean Implant", CleanImplant)
                                .Do("Item Deleted Event", ItemDeleteEvent)
                            .End()
                        .End()
                    .End()
                .End()
                .Build();
        }

        private static BehaviourStatus ItemTradeskillEvent(BotContext c)
        {
            if (_redoTimer.Elapsed)
                return BehaviourStatus.Succeeded;

            if (EventTrigger.Status("TradeskillFailed") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            var itemDelete = EventTrigger.Status("ItemDeleted");

            if (itemDelete == BehaviourStatus.Succeeded)
                c.CurrentTradeskillClusters.LastOrDefault().Tradeskilled = true;

            return itemDelete;
        }

        private static BehaviourStatus ItemDeleteEvent(BotContext context)
        {
            if (_redoTimer.Elapsed)
                return BehaviourStatus.Succeeded;

            if (EventTrigger.Status("TradeskillFailed") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            return EventTrigger.Status("ItemDeleted");
        }

        private static BehaviourStatus TradeskillSourceEvent(BotContext context)
        {
            if (_redoTimer.Elapsed)
                return BehaviourStatus.Succeeded;

            return EventTrigger.Status("TradeskillSource");
        }

        private static BehaviourStatus TradeskillTargetEvent(BotContext context)
        {
            if (_redoTimer.Elapsed)
                return BehaviourStatus.Succeeded;

            return EventTrigger.Status("TradeskillTarget");
        }

        private static BehaviourStatus TradeskillResultEvent(BotContext context)
        {
            if (_redoTimer.Elapsed)
                return BehaviourStatus.Succeeded;

            if (EventTrigger.Status("TradeskillResult") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            if (EventTrigger.Status("TradeskillNotValid") == BehaviourStatus.Succeeded)
                return BehaviourStatus.Succeeded;

            return BehaviourStatus.Running;
        }

        private static BehaviourStatus CleanImplant(BotContext c)
        {
            var lastInvItem = Utils.LastInventoryItem();
            var dissClinSlot = Inventory.Items.FirstOrDefault(x => x.Name.Contains("Disassembly"));

            Action.UseItemOnItem(lastInvItem.Slot, dissClinSlot.Slot);
            Logger.Information($"Tradeskill: {lastInvItem.Name} ({lastInvItem.Slot}) + {dissClinSlot.Name} ({dissClinSlot.Slot})");

            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus TradeskillChangeSource(BotContext c)
        {
            Action.TradeSkillAdd(CharacterActionType.TradeskillSourceChanged, Utils.SecondLastInventoryItem().Slot);
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus TradeskillChangeTarget(BotContext context)
        {
            Action.TradeSkillAdd(CharacterActionType.TradeskillTargetChanged, Utils.LastInventoryItem().Slot);
            return BehaviourStatus.Succeeded;
        }

        private static BehaviourStatus TradeskillImplant(BotContext context)
        {
            var secondLastItem = Utils.SecondLastInventoryItem();
            var lastItem = Utils.LastInventoryItem();

            Action.Tradeskill(OrderProcessor.CurrentOrder.GetImplant().Ql);
            Logger.Information($"Tradeskill: {secondLastItem.Name} ({secondLastItem.Slot}) + {lastItem.Name} ({lastItem.Slot})");

            return BehaviourStatus.Succeeded;
        }

        private static bool CleanImplantCondition1(BotContext c)
        {
            return !Utils.LastInventoryItem().Name.Contains("Basic") &&  //NEW NODE
                c.CurrentTradeskillClusters.Where(x => x.IsTrickle).All(x => x.Tradeskilled) &&
                c.CurrentTradeskillClusters.Where(x => !x.IsTrickle).All(x => !x.Tradeskilled);
        }

        private static bool TradeSkillCondition2(BotContext c)
        {
            return Utils.LastInventoryItem().Name.Contains("Basic") ||
                 c.CurrentTradeskillClusters.Where(x => x.IsTrickle).All(x => x.Tradeskilled); ;
        }

        private static bool CleanImplantCondition2(BotContext c)
        {
            return true;
        }

        private static BehaviourStatus GetNextClusters(BotContext c)
        {
            _redoTimer.Reset();
            DynamicEvent.Reset();
            Logger.Information("Resetting events");

            var currentOrder = OrderProcessor.CurrentOrder;
            var currentTsClusters = currentOrder.GetClusters().Where(x => !x.Tradeskilled);

            if (currentTsClusters.Count() == 0)
            {
                Logger.Information("Out of clusters, moving implant to backpack");
                Client.SendPrivateMessage(currentOrder.Requester, ScriptTemplate.OrderTicket(OrderProcessor.Orders[currentOrder.Requester], $"Order Update"), false);
                return BehaviourStatus.Failed;
            }

            c.CurrentTradeskillClusters = currentTsClusters;

            return BehaviourStatus.Succeeded;
        }
    }
}
