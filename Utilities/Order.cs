using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisImpDispenser
{
    public class Order
    {
        internal int TotalCredits;
        internal int Total => ImplantPresets.Count();
        internal int Limit;
        internal List<ImplantPreset> ImplantPresets = new List<ImplantPreset>();
        protected int RequesterId;
        internal int Tradeskilled => ImplantPresets.Count(x => x.Tradeskilled);
        internal readonly Identity BagIdentity;
        internal float RemainingTime => _orderTimer.TimeRemains;
        private Timer _orderTimer;
        private Timer _tradeTimer;
        private int _requesterId;

        public Order(int requesterId, bool assignBagIdentity = true, int orderLimit = 21)
        {
            _requesterId = requesterId;
            ImplantPresets = new List<ImplantPreset>();
            _tradeTimer = new Timer(Main.Settings.TradeExpireTimeInSeconds);
            _orderTimer = new Timer(Main.Settings.OrderExpireTimeInSeconds);
            Limit = orderLimit;

            if (assignBagIdentity)
                BagIdentity = BagOwners.AddOrGetOwner(requesterId);
        }

        public void Tick(int intervalInSeconds)
        {
           if (OrderTickExpired(intervalInSeconds) || TradeTickExpired(intervalInSeconds))
                OrderProcessor.TryRemoveEntry(_requesterId, true);
        }

        private bool OrderTickExpired(int intervalInSeconds)
        {
            if (!IsCompleted())
                return false;

            if (!_orderTimer.HasStarted())
            {
                _orderTimer.Start();
                Client.SendPrivateMessage(_requesterId, ScriptTemplate.RespondMsg(Color.Green, $"Order ready for pickup. Expire time is set to 30 minutes.\n"+
                    $" I require the following in order to accept the trade:\n" +
                    $" - {TotalCredits} credits\n" +
                    $" - Backpack (non unique)"));

                Client.SendPrivateMessage(_requesterId, ScriptTemplate.RespondMsg(Color.Orange, $" Short guidelines:\n" +
                    $" - Approach me closely and I will engage a trade when I am ready. \n - If I am processing another order (you will notice me moving my hands), please wait patiently\n" +
                    $" - If I am not engaging a trade with you AND idling around (not moving my hands), please report the issue to the host.\n" +
                    $" - You can keep adding more orders to this ticket, however I will only engage a trade when I fully complete your updated order\n" +
                    $" - Your timer will only tick down if I am not processing any other orders.\n\n"));

            }

            if (OrderProcessor.ActiveOrders.Count > 0)
                return false;

            _orderTimer.Tick(intervalInSeconds);

            if (!_orderTimer.HasExpired())
                return false;

            return true;
        }

        private bool TradeTickExpired(int intervalInSeconds)
        {
            if (!Trade.IsTrading)
                return false;

            if (Trade.CurrentTarget != new Identity(IdentityType.SimpleChar, _requesterId))
                return false;

            if (!_tradeTimer.HasStarted())
            {
                _tradeTimer.Start();
                Client.SendPrivateMessage(_requesterId, ScriptTemplate.RespondMsg(Color.Green, $"Trade will expire in {_tradeTimer.TimeLimit} seconds. Move away to pause the timer."));
            }

            _tradeTimer.Tick(intervalInSeconds);

            if (!_tradeTimer.HasExpired())
                return false;

            Trade.Decline();
            return true;
        }

        public bool CanOrder() => Total < Limit;

        public float Status() => (float)Tradeskilled / Total;

        public bool IsCompleted() => Status() == 1;
    }
}