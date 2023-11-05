using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;
using AOSharp.Clientless;
using System.Diagnostics;

namespace MalisImpDispenser
{
    public class ImplantDesigner
    {
        internal static Dictionary<int, Order> Previews = new Dictionary<int, Order>();
        private static ClusterMod _clusterMods;
        private static ImplantReq _implantReqs;
        private static ClusterAlias _clusterAlias;
        private static ImplantAlias _implantAlias;
        private static ImplantMod _implantMods;
        private static ImplantMap _implantMap;

        internal static void Load(string jsonRoot)
        {
            Logger.Information("Implant Designer Loaded");

            _implantAlias = new ImplantAlias($"{jsonRoot}\\ImplantAlias.json");
            Logger.Information("Implant Shops Loaded");

            _clusterAlias = new ClusterAlias($"{jsonRoot}\\ClusterAlias.json");
            Logger.Information("Cluster Shops Loaded");

            _implantMods = new ImplantMod($"{jsonRoot}\\ImplantMods.json");
            Logger.Information("Implant Mods Loaded");

            _clusterMods = new ClusterMod($"{jsonRoot}\\ClusterMods.json");      
            Logger.Information("Cluster Mods Loaded");

            _implantMap = new ImplantMap($"{jsonRoot}\\ImplantMap.json");
            Logger.Information("Implant Map Loaded");

            _implantReqs = new ImplantReq($"{jsonRoot}\\ImplantReqs.json");
            Logger.Information("Implant Reqs Loaded");
        }

        internal static ImplantReqInfo GetReq(int ql, IEnumerable<ClusterItem> clusters) => _implantReqs.GetReq(ql, clusters);

        internal static bool GetImplantInfo(ImplantPreset preset, out int lowId, out int highId, out ClusterStat? req) => _implantMap.GetImplant(preset, out lowId, out highId, out req);
     
        internal static bool GetImplantInfo(PremadeImplant preset, out int lowId, out int highId, out ClusterStat? req) => _implantMap.GetImplant(preset, out lowId, out highId, out req);

        internal static int GetMod(int ql, ClusterItem clusterItem) => _clusterMods.GetModifier(ql, clusterItem);

        internal static bool TryGetImplant(string tag, out ImplantSlot slot) => _implantAlias.TryGet(tag, out slot);

        internal static Dictionary<ClusterStat, string> GetClusterAlias() => _clusterAlias.Entries.OrderBy(x => x.Key.ToString()).ToDictionary(x => x.Key, y => string.Join(", ", y.Value.OrderBy(x=>x.Length)));

        internal static Dictionary<ImplantSlot, string> GetImplantAlias() => _implantAlias.Entries.OrderBy(x => x.Key.ToString()).ToDictionary(x => x.Key, y => string.Join(", ", y.Value.OrderBy(x => x.Length)));

        private static bool TryGetCluster(string tag, out ClusterStat stat) => _clusterAlias.TryGet(tag.ToLower(), out stat);

        internal static bool IsQlValid(int ql) => !(ql < 10 || ql > 200);

        internal static bool GetBestQl(int ability, int treatment, List<ClusterItem> clusters, int? starIndex, out int ql, out string errorMsg)
        {
            ql = 0;
            errorMsg = "";

            if (starIndex == null)
            {
                return (ql = GetQlByReqs(ability, treatment, clusters)) != 0;
            }
            else
            {
                int maxQl = GetQlByReqs(ability, treatment, clusters);
                ClusterItem starredCluster = clusters[starIndex.Value];

                for (int i = maxQl; i > 1; i--)
                {
                    if (GetMod(i - 1, starredCluster) != GetMod(i, starredCluster))
                        return (ql = i) != 0;
                }
            }

            errorMsg = "Could not obtain best ql for given stats. This is likely a maths error.";

            return false;
        }

        private static int GetQlByReqs(int ability, int treatment, IEnumerable<ClusterItem> clusters)
        {
            var highReq = GetReq(200, clusters);
            return Utils.Clamp((int)Math.Round(Math.Min((float)ability / highReq.Ability, (float)treatment / highReq.Treatment) * 200) - 1, 1, 200);
        }

        private static bool IsClusterAllowed(ClusterItem clusterItem)
        {
            switch (clusterItem.Stat)
            {
                case ClusterStat.Swimming:
                case ClusterStat.MapNavigation:
                    return false;
                case ClusterStat.NanoDelta:
                    if (clusterItem.Type == ClusterType.Bright || clusterItem.Type == ClusterType.Shiny)
                        return false;
                    return true;
                default:
                    return true;
            }
        }

        internal static ImplantPreset MakePreset(int requesterId, int impQl, ImplantSlot impSlot, List<ClusterItem> clusters)
        {
            ImplantPreset newPreset = new ImplantPreset(requesterId);
            ImplantItem implant = new ImplantItem { Ql = impQl, Slot = impSlot };

            foreach (var cluster in clusters.ToList())
            {
                if (!IsClusterAllowed(cluster))
                {
                    Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Cluster with stat: {cluster.Stat} not allowed."));
                    clusters.Remove(cluster);
                }

                if (!Tradeskill.CanCraftImplant(implant, cluster, out string errorMsg2))
                {
                    clusters.Remove(cluster);
                    Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, errorMsg2));
                }
            }

            if (clusters == null)
                return null;

            newPreset.AddToShopList(clusters);
            newPreset.AddToShopList(Tradeskill.GetTrickleClusters(implant, clusters));
            newPreset.AddToShopList(implant);

            return newPreset;
        }

        internal static bool TryGetClusterPreset(ImplantSlot implantSlot, List<string> tags, out List<ClusterItem> presets)
        {
            presets = new List<ClusterItem>();

            foreach (string tag in tags)
            {
                if (!TryFindCluster(implantSlot, tag, out ClusterItem clusterPreset))
                    return false;

                presets.Add(clusterPreset);
            }

            if (presets.Count > 0 && presets.IsDistinct())
                return true;

            return false;
        }

        internal static bool TryFindCluster(ImplantSlot implantSlot, string clusterTag, out ClusterItem preset)
        {
            preset = null;

            if (clusterTag == null)
                return true;

            if (!TryGetCluster(clusterTag, out ClusterStat stat))
                return false;

            if (!TryGetClusterTypeByStat(implantSlot, stat, out ClusterType clusterSlot))
                return false;

            preset = new ClusterItem { Type = clusterSlot, Stat = stat };
            return true;
        }

        internal static bool TryGetClusterTypeByStat(ImplantSlot implantSlot, ClusterStat stat, out ClusterType clusterSlot)
        {
            KeyValuePair<ClusterType, List<ClusterStat>> cluster = _implantMods.Entries[implantSlot].FirstOrDefault(x => x.Value.Contains(stat));

            clusterSlot = new ClusterType();

            if (cluster.Value == null)
                return false;

            clusterSlot = cluster.Key;

            return true;
        }

        internal static List<ClusterStat> GetClustersBySlot(ImplantSlot implantSlot, ClusterType type) => _implantMods.Entries[implantSlot][type];

        internal static ClusterItem GetFirstCluster(ImplantSlot implantSlot, ClusterType slot, ClusterStat clusterStat = ClusterStat.NanoProgramming)
        {
            return new ClusterItem 
            {
                Stat = _implantMods.Entries[implantSlot][slot].Where(x => x.GetTradeskillStat() == clusterStat && x.GetStatType() != StatType.Jobe).OrderBy(x => x.GetMultiplier()).FirstOrDefault(),
                Type = slot
            };
        }
    }
}