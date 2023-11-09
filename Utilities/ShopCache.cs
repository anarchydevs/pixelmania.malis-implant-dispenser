using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    public static class ShopCache
    {
        private static ImplantPriceInfo _implantPrices;
        private static ClusterPriceInfo _clusterPrices;
        private static Dictionary<ShopType, KeyValuePair<Identity, List<ClusterItem>>> _clusterShops;
        private static Dictionary<ShopType, KeyValuePair<Identity, List<ImplantItem>>> _implantShops;
        private static int _computerLiteracy;

        internal static void Load(string jsonRoot)
        {
            ClusterShopItemInfo _clusterShopInfo = new ClusterShopItemInfo($"{jsonRoot}\\ClusterShopItemInfo.json");
            ImplantShopItemInfo _implantShopInfo = new ImplantShopItemInfo($"{jsonRoot}\\ImplantShopItemInfo.json");
   
            _implantPrices = new ImplantPriceInfo($"{jsonRoot}\\ImplantPriceInfo.json");
            _clusterPrices = new ClusterPriceInfo($"{jsonRoot}\\ClusterPriceInfo.json");

            _clusterShops = new Dictionary<ShopType, KeyValuePair<Identity, List<ClusterItem>>>();
            _implantShops = new Dictionary<ShopType, KeyValuePair<Identity, List<ImplantItem>>>();

            foreach (var vendingMachine in DynelManager.VendingMachines)
            {
                foreach (ShopType vendingMachineType in Enum.GetValues(typeof(ShopType)))
                {
                    if (vendingMachine.Name != vendingMachineType.GetDescription())
                        continue;

                    if (vendingMachine.Name.Contains("Implant"))
                    {
                        _implantShops[vendingMachineType] = new KeyValuePair<Identity, List<ImplantItem>>(vendingMachine.Identity, _implantShopInfo.Entries[vendingMachineType]);
                    }
                    else if (vendingMachine.Name.Contains("Clusters"))
                    {
                        _clusterShops[vendingMachineType] = new KeyValuePair<Identity, List<ClusterItem>>(vendingMachine.Identity, _clusterShopInfo.Entries[vendingMachineType]);
                    }
                }
            }
        }

        internal static void SetStats(int compLit) => _computerLiteracy = compLit;

        internal static int GetClusterPrice(ClusterType slot) => GetShopSellValue(_clusterPrices.Entries[slot], 1, 5.05f, _computerLiteracy);

        internal static int GetImplantPrice(int ql) => GetShopSellValue(_implantPrices.Entries[(ql / 10) * 10], 1, ql > 110 ? 10f : 1.05f, _computerLiteracy);

        internal static void BuyItem(int itemIndex) => Trade.AddItem(itemIndex);

        internal static void OpenShop(ShopType type) => Action.OpenShop(GetIdentity(type));

        internal static ShopItem FindItem(CoreItem baseItem) => baseItem is ImplantItem implantItem ? FindItem(implantItem) : FindItem((ClusterItem)baseItem);

        internal static List<ClusterItem> GetClusters(ShopType vendingMachineType)
        {
            switch (vendingMachineType)
            {
                case ShopType.AdvancedICCShinyClusters:
                case ShopType.AdvancedICCBrightClusters:
                case ShopType.AdvancedICCFadedClusters:
                    return _clusterShops[vendingMachineType].Value;
                default:
                    return new List<ClusterItem>();
            }
        }

        internal static List<ImplantItem> GetImplants(ShopType vendingMachineType)
        {
            switch (vendingMachineType)
            {
                case ShopType.AdvancedICCImplants:
                case ShopType.BasicICCImplants:
                    return _implantShops[vendingMachineType].Value;
                default:
                    return new List<ImplantItem>();
            }
        }

        private static Identity GetIdentity(ShopType vendingMachineType)
        {
            switch (vendingMachineType)
            {
                case ShopType.AdvancedICCImplants:
                case ShopType.BasicICCImplants:
                    return _implantShops[vendingMachineType].Key;
                case ShopType.AdvancedICCShinyClusters:
                case ShopType.AdvancedICCBrightClusters:
                case ShopType.AdvancedICCFadedClusters:
                    return _clusterShops[vendingMachineType].Key;
                default:
                    return Identity.None;
            }
        }

        private static int GetShopSellValue(int statValue, float shopModifier, float shopSellMod, int cl)
        {
            float clMod = (float)Math.Floor((float)cl / 40);
            float finalValue = statValue * shopModifier * shopSellMod * (1f - clMod / 100f);
            return (int)Math.Round(finalValue);
        }

        private static ShopItem FindItem(ImplantItem implantToFind)
        {
            var shopToLook = implantToFind.Ql >= 110 ? ShopType.AdvancedICCImplants : ShopType.BasicICCImplants;
            var shop = _implantShops[shopToLook];

            Logger.Information(shopToLook.ToString());
            ImplantItem closestLowerItem = shop.Value
                .Where(item => item.Slot == implantToFind.Slot && item.Ql <= implantToFind.Ql)
                .OrderByDescending(item => item.Ql)
                .FirstOrDefault();

            return new ShopItem { ShopIdentity = shop.Key, ItemIndex = shop.Value.IndexOf(closestLowerItem) };
        }

        private static ShopItem FindItem(ClusterItem clusterToFind)
        {
            KeyValuePair<Identity, List<ClusterItem>> shop = new KeyValuePair<Identity, List<ClusterItem>>();

            switch (clusterToFind.Type)
            {
                case ClusterType.Shiny:
                    shop = _clusterShops[ShopType.AdvancedICCShinyClusters];
                    break;
                case ClusterType.Bright:
                    shop = _clusterShops[ShopType.AdvancedICCBrightClusters];
                    break;
                case ClusterType.Faded:
                    shop = _clusterShops[ShopType.AdvancedICCFadedClusters];
                    break;
            }

            var clusterItem = shop.Value.FirstOrDefault(x => clusterToFind.Stat == x.Stat);

            return clusterItem == null ? null : new ShopItem { ShopIdentity = shop.Key, ItemIndex = shop.Value.IndexOf(clusterItem) };
        }
    }
}