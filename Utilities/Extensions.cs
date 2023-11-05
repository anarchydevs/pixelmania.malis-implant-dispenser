using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MalisImpDispenser
{
    public static class Extensions
    {

        public static void MoveTo(this LocalPlayer localPlayer, Vector3 position, Quaternion heading)
        {
            DynelManager.LocalPlayer.MovementComponent.ChangeMovement(MovementAction.LeaveSit);

            Client.Send(new CharDCMoveMessage
            {
                Position = position,
                Heading = heading,
                MoveType = MovementAction.ForwardStop,
                Unknown = 1,
            });
        }

        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo == null) return null;

            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

            return attribute?.Description;
        }

        public static ClusterStat GetTradeskillStat(this Enum clusterStat)
        {
            var fieldInfo = clusterStat.GetType().GetField(clusterStat.ToString());
            var tradeskillIdAttribute = (TradeskillIdAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TradeskillIdAttribute));

            return tradeskillIdAttribute.Stat;

        }

        public static StatType GetStatType(this Enum clusterStat)
        {
            var fieldInfo = clusterStat.GetType().GetField(clusterStat.ToString());
            var statTypeAttribute = (StatTypeAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(StatTypeAttribute));

            return statTypeAttribute.Stat;
        }

        public static bool Contains(this IEnumerable<ClusterItem> clusters, ClusterType type) => clusters.Any(x => x.Type == type && !x.IsTrickle);
     
        public static IEnumerable<ClusterItem> Get(this IEnumerable<ClusterItem> clusters, StatType statType) => clusters.Where(x => x.Stat.GetStatType() == statType && !x.IsTrickle);

        public static float GetMultiplier(this Enum clusterStat)
        {
            var fieldInfo = clusterStat.GetType().GetField(clusterStat.ToString());
            var statTypeAttribute = (SkillMultiplierAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(SkillMultiplierAttribute));

            return statTypeAttribute.Multiplier;
        }

        internal static bool IsDistinct(this IEnumerable<ClusterItem> clusters)
        {
            List<ClusterType> slots = clusters.Select(x => x.Type).ToList();
            return !(slots.Count() > slots.Distinct().Count());
        }
    }
}