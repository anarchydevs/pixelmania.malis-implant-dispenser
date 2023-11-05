using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;
using System.Diagnostics;

namespace MalisImpDispenser
{
    internal class ImplantMap : JsonFile<Dictionary<string, int[]>>
    {
        private static Dictionary<ClusterStat, byte> _clusterStatRemap = new Dictionary<ClusterStat, byte>();
        private static Dictionary<ImplantSlot, byte> _implantSlotRemap = new Dictionary<ImplantSlot, byte>();

        internal ImplantMap(string path) : base(path)   
        {
            int i = 0;
            foreach (ImplantSlot s in Enum.GetValues(typeof(ImplantSlot)))
                _implantSlotRemap[s] = (byte)i++;

            int c = 0;
            foreach (ClusterStat s in Enum.GetValues(typeof(ClusterStat)))
                _clusterStatRemap[s] = (byte)c++;
        }

        internal bool GetImplant(ImplantPreset impPreset, out int lowId, out int highId, out ClusterStat? impStat) => GetImplant(impPreset.GetClusters().Where(x => !x.IsTrickle), impPreset.GetImplant().Slot, out lowId, out highId, out impStat);
        internal bool GetImplant(PremadeImplant implant, out int lowId, out int highId, out ClusterStat? impStat) => GetImplant(implant.Clusters, implant.Implant.Slot, out lowId, out highId, out impStat);

        private bool GetImplant(IEnumerable<ClusterItem> clusters, ImplantSlot slot, out int lowId, out int highId, out ClusterStat? impStat)
        {
            lowId = 0;
            highId = 0;
            impStat = null;

            string hashSet = _implantSlotRemap[slot].ToString("X2");

            if (clusters.Count() > 0)
                foreach (var stat in clusters.OrderBy(x => (int)x.Stat).Select(x => _clusterStatRemap[x.Stat]))
                    hashSet += stat.ToString("X2");

            //   Logger.Information(hashSet);

            if (!Entries.TryGetValue(hashSet, out int[] stats))
            {
                Logger.Error("Could not find implant hashset");
                return false;
            }

            lowId = stats[0];
            highId = stats[1];

            if (Enum.IsDefined(typeof(ClusterStat), stats[2]))
                impStat = (ClusterStat)stats[2];

            return true;
        }
    }
}