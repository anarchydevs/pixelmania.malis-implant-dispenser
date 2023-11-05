using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class ImplantMod : JsonFile<IReadOnlyDictionary<ImplantSlot, Dictionary<ClusterType, List<ClusterStat>>>>
    {
        internal ImplantMod(string jsonRoot) : base(jsonRoot) { }
    }
}