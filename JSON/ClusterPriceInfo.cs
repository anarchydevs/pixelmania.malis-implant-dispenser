using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class ClusterPriceInfo : JsonFile<Dictionary<ClusterType, int>>
    {
        internal ClusterPriceInfo(string path) : base(path) { }
    }
}