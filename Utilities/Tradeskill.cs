using AOSharp.Clientless.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisImpDispenser
{
    internal static class Tradeskill
    {
        private static Dictionary<ClusterStat, int> _stats = new Dictionary<ClusterStat, int>();

        internal static void SetStats(int nanoProgramming, int quantumField, int psychology, int weaponSmithing, int pharmaTechnology, int computerLiteracy)
        {
            _stats[ClusterStat.NanoProgramming] = nanoProgramming;
            _stats[ClusterStat.QuantumFT] = quantumField;
            _stats[ClusterStat.Psychology] = psychology;
            _stats[ClusterStat.WeaponSmithing] = weaponSmithing;
            _stats[ClusterStat.PharmaTechnology] = pharmaTechnology;
            _stats[ClusterStat.ComputerLiteracy] = computerLiteracy;
        }
        
        public static bool CanCraftImplant(ImplantItem implantItem, ClusterItem clusterItem, out string errorMsg)
        {
            errorMsg = "";

            var skillToTs = SkillToTradeskill(implantItem.Ql, clusterItem);
            var currentTsStat = _stats[clusterItem.Stat.GetTradeskillStat()];

            if (skillToTs > currentTsStat)
            {
                errorMsg = $"Cannot tradeskill '{implantItem.Slot.GetDescription()}'. [Requires {clusterItem.Stat.GetTradeskillStat().GetDescription()}: {skillToTs}, Current: {currentTsStat}]";
                return false;
            }

            var requiresBump = implantItem.Ql % 10 != 0;

            if (!requiresBump)
                return true;

            var skillToBump = SkillToTradeskill(implantItem.Ql - 1, clusterItem) + 100f;

            if (skillToBump > currentTsStat)
            {
                errorMsg = $"Cannot bump '{implantItem.Slot.GetDescription()}'. [Requires {clusterItem.Stat.GetTradeskillStat().GetDescription()}: {skillToBump}, Current: {currentTsStat}]";
                return false;
            }

            return true;
        }

        private static int SkillToTradeskill(int ql, ClusterItem clusterItem) => (int)Math.Floor(ql * GetBaseClusterMultiplier(clusterItem));


        private static int GetMaxBump(int ql) => ql < 50 ? 0 : ql < 100 ? 1 : ql < 150 ? 2 : 3;

        private static float GetClusterBumpMultiplier(ClusterItem clusterItem)
        {
            switch (clusterItem.Stat.GetStatType())
            {
                case StatType.Ability:
                case StatType.Skill:
                    switch (clusterItem.Type)
                    {
                        case ClusterType.Shiny:
                            return 300f;
                        case ClusterType.Bright:
                            return 200f;
                        case ClusterType.Faded:
                            return 100f;
                    }
                    break;
                case StatType.Jobe:
                    switch (clusterItem.Type)
                    {
                        case ClusterType.Shiny:
                            return 400f;
                        case ClusterType.Bright:
                            return 300f;
                        case ClusterType.Faded:
                            return 200f;
                    }
                    break;
            }

            return 0f;
        }

        internal static int ClusterBump(int ql, ClusterItem clusterItem) => Utils.Clamp((int)Math.Floor((_stats[clusterItem.Stat.GetTradeskillStat()] - SkillToTradeskill(ql, clusterItem)) / GetClusterBumpMultiplier(clusterItem)), 0, GetMaxBump(ql));

        private static int ClustersBump(int impQl, IEnumerable<ClusterItem> preset)
        {
            int maxBump = 0;

            foreach (ClusterItem cluster in preset)
            {
                maxBump += ClusterBump(impQl, cluster);
            }
            return maxBump;
        }

        private static int GetNumOfTrickleClusters(ImplantItem implant, IEnumerable<ClusterItem> clusters)
        {
            var qlsToBump = implant.Ql % 10;

            if (qlsToBump == 0)
                return 0;

            qlsToBump -= ClustersBump(implant.Ql, clusters);

            if (qlsToBump <= 0)
                return 0;

            var clus = ImplantDesigner.GetFirstCluster(implant.Slot, ClusterType.Faded);

            return (int)Math.Ceiling((float)qlsToBump / ClusterBump(implant.Ql, clus));
        }

        internal static List<ClusterItem> GetTrickleClusters(ImplantItem implantItem, IEnumerable<ClusterItem> clusters)
        {
            var trickleList = new List<ClusterItem>();

            for (int i = 0; i < GetNumOfTrickleClusters(implantItem, clusters); i++)
            {
                var trickleCluster = ImplantDesigner.GetFirstCluster(implantItem.Slot, ClusterType.Faded);
                trickleCluster.IsTrickle = true;
                trickleList.Add(trickleCluster);
            }

            return trickleList;
        }

        internal static float GetBaseClusterMultiplier(ClusterItem clusterItem)
        {
            var multiplier = 0f;

            if (clusterItem.Stat == null || clusterItem.Stat == 0)
                return 0f;

            switch (clusterItem.Stat.GetStatType())
            {
                case StatType.Ability:
                case StatType.Skill:
                    switch (clusterItem.Type)
                    {
                        case ClusterType.Shiny:
                            multiplier = 2f;
                            break;
                        case ClusterType.Bright:
                            multiplier = 1.5f;
                            break;
                        case ClusterType.Faded:
                            multiplier = 1f;
                            break;
                    }
                    break;
                case StatType.Jobe:
                    switch (clusterItem.Type)
                    {
                        case ClusterType.Shiny:
                            multiplier = 6.25f;
                            break;
                        case ClusterType.Bright:
                            multiplier = 4.75f;
                            break;
                        case ClusterType.Faded:
                            multiplier = 3.25f;
                            break;
                    }
                    break;

            }
            return multiplier * clusterItem.Stat.GetMultiplier();
        }
    }
}