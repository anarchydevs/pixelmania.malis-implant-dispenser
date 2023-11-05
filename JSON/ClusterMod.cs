using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class ClusterMod : ItemRange<Dictionary<int, Dictionary<ClusterType, List<ModifierInfo>>>>
    {
        internal ClusterMod(string path) : base(path) { }

        private int FindMod(int ql, ClusterItem clusterItem)
        {
            if (!Entries.TryGetValue(ql, out var modInfo))
                return 0;

            return modInfo[clusterItem.Type].FirstOrDefault(x => x.Stats.Contains(clusterItem.Stat)).Modifier;
        }

        internal int GetModifier(int ql, ClusterItem clusterItem)
        {
            var lowMod = FindMod(1, clusterItem);

            if (ql == 1)
                return lowMod;

            var highMod = FindMod(200, clusterItem);

            if (ql == 200)
                return highMod;

            return Interpolate(ql, lowMod, highMod);
        }
    }

    public class ModifierInfo
    {
        public List<ClusterStat> Stats = new List<ClusterStat>();
        public int Modifier;
    }
}