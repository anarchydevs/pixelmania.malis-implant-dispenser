using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class ClusterShopItemInfo : JsonFile<Dictionary<ShopType, List<ClusterItem>>>
    {
        internal ClusterShopItemInfo(string path) : base(path) { }
    }
}