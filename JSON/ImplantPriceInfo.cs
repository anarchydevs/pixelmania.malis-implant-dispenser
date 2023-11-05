using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class ImplantPriceInfo : JsonFile<Dictionary<int, int>>
    {
        internal ImplantPriceInfo(string path) : base(path) { }
    }
}