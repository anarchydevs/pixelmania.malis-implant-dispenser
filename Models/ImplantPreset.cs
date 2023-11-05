using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Common.SharedEventArgs;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.ComponentModel;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    public class ImplantPreset
    {
        internal int Index;

        internal int Requester;
        
        internal bool ShopListEmpty => _shopList.All(x => x.Purchased);

        internal bool Tradeskilled => _shopList.OfType<ClusterItem>().All(x => x.Tradeskilled);
      
        internal ImplantItem GetImplant() => _shopList.OfType<ImplantItem>().FirstOrDefault();

        internal ClusterItem GetCluster(ClusterType slot) => _shopList.OfType<ClusterItem>().FirstOrDefault(x => x.Type == slot && !x.IsTrickle);

        internal IEnumerable<ClusterItem> GetClusters() => _shopList.Count() == 0 ? new List<ClusterItem>() : _shopList.OfType<ClusterItem>();

        internal bool Contains(ClusterType slot) => _shopList.OfType<ClusterItem>().FirstOrDefault(x => x.Type == slot && x.IsTrickle == false) != null;

        internal CoreItem GetNextShopItem() => _shopList.FirstOrDefault(x => !x.Purchased);

        private List<CoreItem> _shopList = new List<CoreItem>();

        public ImplantPreset(int requesterId)
        {
            Requester = requesterId;
        }

        internal void AddToShopList(IEnumerable<CoreItem> items)
        {
            _shopList.AddRange(items);
        }

        internal void AddToShopList(CoreItem item)
        {
            _shopList.Add(item);
        }

        internal void RemoveFromShopList(CoreItem item)
        {
            _shopList.Remove(item);
        }

        internal void ReplaceCluster(ClusterType type, ClusterItem item)
        {
            var existingCluster = GetCluster(type);

            if (existingCluster != null)
                RemoveFromShopList(existingCluster);

            AddToShopList(item);
        }

        internal int GetTotalPrice()
        {
            int totalPrice = 0;

            foreach (CoreItem baseItem in _shopList)
                totalPrice += baseItem is ImplantItem implant ? 
                    ShopCache.GetImplantPrice(implant.Ql) : 
                    baseItem is ClusterItem cluster ? 
                    ShopCache.GetClusterPrice(cluster.Type) : 
                    0;

            return totalPrice;
        }
    }

    public class ImplantItem : CoreItem
    {
        public int Ql;
        public ImplantSlot Slot;
    }

    public class ClusterItem : CoreItem
    {
        public ClusterType Type;
        public ClusterStat Stat;
        public bool IsTrickle = false;
        public bool Tradeskilled = false;
    }

    public class CoreItem
    {
        public bool Purchased = false;
    }
}
