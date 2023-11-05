using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class ClusterAlias : Alias<ClusterStat>
    {
        internal ClusterAlias(string path) : base(path) { }
    }
}