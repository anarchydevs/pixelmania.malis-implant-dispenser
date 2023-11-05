using BehaviourTree.FluentBuilder;
using BehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Common.Helpers;
using System.Diagnostics;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal static class DepositBehavior
    {
        internal static IBehaviour<BotContext> Process()
        {
            return FluentBuilder.Create<BotContext>()
                .Sequence("Process Deposit")
                    .Do("Move order to bag", MoveOrderToBag)
                    .Do("Implant to bag event", c => EventTrigger.Status("ContainerAddItem"))
                .End()
                .Build();
        }

        private static BehaviourStatus MoveOrderToBag(BotContext c)
        {
            var currentOrder = OrderProcessor.GetCurrentOrder();

            Utils.LastInventoryItem().MoveToContainer(currentOrder.BagIdentity);
            return BehaviourStatus.Succeeded;
        }
    }
}