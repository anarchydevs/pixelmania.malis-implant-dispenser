using AOSharp.Clientless;
using AOSharp.Common.GameData;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisImpDispenser
{
    public class BotContext
    {
        public Identity TradeOrderTarget = Identity.None;
        public Identity TradeDebtTarget = Identity.None;
        public IEnumerable<ClusterItem> CurrentTradeskillClusters = new List<ClusterItem>();
        public ActiveItem ActiveItem;
        public int TrickleClusters;
    }

    public class ActiveItem
    {
        public CoreItem BaseItem;
        public ShopItem ShopItem;
    }

    public class ShopItem
    {
        public Identity ShopIdentity;
        public int ItemIndex;
    }
}   