using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class ImplantShopItemInfo : JsonFile<Dictionary<ShopType, List<ImplantItem>>>
    {
        internal ImplantShopItemInfo(string path) : base(path) { }
    }
}