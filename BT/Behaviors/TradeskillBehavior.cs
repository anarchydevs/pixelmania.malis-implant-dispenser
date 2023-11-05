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
    internal static class TradeskillBehavior
    {
        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
                .UntilFailed("Tradeskill Loop")
                    .Sequence("Tradeskill Sequence")
                        .Do("Tradeskill Item", TradeskillItem)
                        .Do("Item Deleted Event", c => EventTrigger.Status("ItemDeleted"))
                    .End()
                .End()
                .Build();
        }

        private static BehaviourStatus TradeskillItem(BotContext c)
        {
            var currentOrder = OrderProcessor.CurrentOrder;
            var clusters = currentOrder.GetClusters();
            var nonTradeskilledItems = clusters.Where(x => !x.Tradeskilled);

            if (nonTradeskilledItems.Count() == 0)
            {
                var order = OrderProcessor.Orders[currentOrder.Requester];
                Client.SendPrivateMessage(currentOrder.Requester, ScriptTemplate.OrderTicket(OrderProcessor.Orders[currentOrder.Requester], $"Order Update"), false);
         
                return BehaviourStatus.Failed;
            }

            if (!Utils.LastInventoryItem().Name.Contains("Basic") && 
                clusters.Where(x => x.IsTrickle).All(x => x.Tradeskilled) && 
                clusters.Where(x => !x.IsTrickle).All(x => !x.Tradeskilled))
            {
                var lastInvItem = Utils.LastInventoryItem();
                var dissClinSlot = Inventory.Items.FirstOrDefault(x => x.Name.Contains("Disassembly"));
          
                Action.UseItemOnItem(lastInvItem.Slot, dissClinSlot.Slot);
                Logger.Information($"Tradeskill: {lastInvItem.Name} ({lastInvItem.Slot}) + {dissClinSlot.Name} ({dissClinSlot.Slot})");
            }
            else if (Utils.LastInventoryItem().Name.Contains("Basic") || clusters.Where(x => x.IsTrickle).All(x => x.Tradeskilled))
            {
                var secondLastItem = Utils.SecondLastInventoryItem();
                var lastInvItem = Utils.LastInventoryItem();

                Action.Tradeskill(secondLastItem.Slot, lastInvItem.Slot, OrderProcessor.CurrentOrder.GetImplant().Ql);
                nonTradeskilledItems.LastOrDefault().Tradeskilled = true;
                Logger.Information($"Tradeskill: {secondLastItem.Name} ({secondLastItem.Slot}) + {lastInvItem.Name} ({lastInvItem.Slot})");
            }
            else
            {
                var lastInvItem = Utils.LastInventoryItem();
                var clinic = Inventory.Items.FirstOrDefault(x => x.Name.Contains("Disassembly"));

                Action.UseItemOnItem(lastInvItem.Slot, clinic.Slot);
                Logger.Information($"Tradeskill: {lastInvItem.Name} ({lastInvItem.Slot}) + {clinic.Name} ({clinic.Slot})");
            }

            return BehaviourStatus.Succeeded;
        }
    }
}
