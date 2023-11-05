using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using System;
using System.Linq;

namespace MalisImpDispenser
{
    public class BotBehavior
    {
        private IBehaviour<BotContext> _behavior;
        internal static BotContext Context;
        private AutoResetInterval _autoResetInterval;   

        public BotBehavior(int tickIntervalMs)
        {
            Context = new BotContext(); 
            _behavior = RootBehavior();
            _autoResetInterval = new AutoResetInterval(tickIntervalMs);
            Client.OnUpdate += OnUpdate;
        }

        private void OnUpdate(object sender, double delta)
        {
            if (!_autoResetInterval.Elapsed)
                return;
            try
            {
                _behavior.Tick(Context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private static IBehaviour<BotContext> RootBehavior()
        {
            return FluentBuilder.Create<BotContext>()
                .Selector("Root")
                    .Sequence("Process Order Sequence")
                        .Do("Any available orders?", CanTakeOrder)
                        .Subtree(BuyingBehavior.Process())
                        .Subtree(TradeskillBehavior.Process())
                        .Subtree(DepositBehavior.Process())
                    .End()
                    .Sequence("Trade Behavior")
                        .Subtree(TradeOrderBehavior.Process())
                        .Subtree(TradeDebtBehavior.Process())
                    .End()
                .End()
                .Build();
        }

        private static BehaviourStatus CanTakeOrder(BotContext c)
        {
            if (OrderProcessor.HasOrders())
            {
                OrderProcessor.ProcessOrder();
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Failed;
        }
    }
}