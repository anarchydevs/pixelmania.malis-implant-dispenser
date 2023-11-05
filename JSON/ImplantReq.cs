using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class ImplantReq : ItemRange<List<Dictionary<int, ImplantReqInfo>>>
    {
        internal ImplantReq(string path) : base(path) { }

        internal ImplantReqInfo GetReq(int ql, IEnumerable<ClusterItem> clusters)
        {
            if (clusters.Count() == 0)
            {
                return new ImplantReqInfo { Ability = 0, Treatment = 0 };
            }

            int index = 0;
            IEnumerable<ClusterItem> jobeClusters = clusters.Get(StatType.Jobe);

            if (jobeClusters.Count() > 0)
            {
                var shiny = jobeClusters.Contains(ClusterType.Shiny);
                var bright = jobeClusters.Contains(ClusterType.Bright);
                var faded = jobeClusters.Contains(ClusterType.Faded);

                index = shiny && bright && faded || shiny && bright || bright && faded || faded ? 2 : 1;
            }

            ImplantReqInfo lowMod = Entries[index].FirstOrDefault(x => x.Key == 1).Value;
            ImplantReqInfo highMod = Entries[index].FirstOrDefault(x => x.Key == 200).Value;

            return new ImplantReqInfo
            {
                Ability = Interpolate(ql, lowMod.Ability, highMod.Ability),
                Treatment = Interpolate(ql, lowMod.Treatment, highMod.Treatment),
            };
        }
    }

    public class ImplantReqInfo
    {
        public int Ability;
        public int Treatment;
    }
}