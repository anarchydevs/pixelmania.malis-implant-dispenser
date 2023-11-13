using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using Scriban;
using SmokeLounge.AOtomation.Messaging.Messages;

namespace MalisImpDispenser
{
    internal static class ScriptTemplate
    {
        public static string RespondMsg(string color, string message)
        {
            var template = GetTemplate("RespondTemplate").Render(new
            {
                Message = message,
                Color = color,
            });

            return template;
        }

        public static string ImplantPreview(int index, Order preview, string scriptName = "Implant Template")
        {
            var implant = preview.ImplantPresets.FirstOrDefault(x=>x.Index == index).GetImplant();

            var template = GetTemplate("ImplantTemplate").Render(new
            {
                Index = index,
                Color = GetColors(),
                Name = $"[Index: {index}] Modify {scriptName}",
                Botname = DynelManager.LocalPlayer.Name,
                Qlrange = Enumerable.Range(1, 20).Select(i => i * 10),
                Shinyclusters = ImplantDesigner.GetClustersBySlot(implant.Slot, ClusterType.Shiny).Select(x => new { Tag = x, Name = x.GetDescription() }),
                Brightclusters = ImplantDesigner.GetClustersBySlot(implant.Slot, ClusterType.Bright).Select(x => new { Tag = x, Name = x.GetDescription() }),
                Fadedclusters = ImplantDesigner.GetClustersBySlot(implant.Slot, ClusterType.Faded).Select(x => new { Tag = x, Name = x.GetDescription() }),
            });

            return template;
        }

        public static string Help()
        {
            return GetTemplate("HelpTemplate").Render(new
            {
                Color = GetColors(),
            });
        }

        public static string Alias()
        {
            return GetTemplate("AliasTemplate").Render(new
            {
                Color = GetColors(),
                Implantalias = ImplantDesigner.GetImplantAlias(),
                Clusteralias = ImplantDesigner.GetClusterAlias()    
            });
        }

        public static string OrderTicket(Order order, string scriptName = "Order Ticket", bool previewMode = false)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(order.RemainingTime);

            var template = GetTemplate("OrderTemplate").Render(new
            {
                Botname = DynelManager.LocalPlayer.Name,
                Previewmode = previewMode,
                Implantpreset = Enum.GetValues(typeof(ImplantSlot)).Cast<ImplantSlot>().Select(x => new { Name = x.GetDescription(), Type = x.ToString() }),
                Name = scriptName,
                Color = GetColors(),
                Order = new
                {
                    Expire = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds),
                    Completed = order.Tradeskilled,
                    Total = order.Total,
                    Credits = order.TotalCredits.ToString("N0", CultureInfo.InvariantCulture),
                    Limit = order.Limit,
                },
                Implants = order.ImplantPresets.Select(x =>
                {
                    var implant = x.GetImplant();
                    var implantQl = implant.Ql;
                    var shinyCluster = x.GetCluster(ClusterType.Shiny);
                    var brightCluster = x.GetCluster(ClusterType.Bright);
                    var fadedCluster = x.GetCluster(ClusterType.Faded);
                    ImplantDesigner.GetImplantInfo(x, out int lowId, out int highId, out ClusterStat? req);
                    var impReq = ImplantDesigner.GetReq(implantQl, x.GetClusters());

                    return new
                    {
                        Index = x.Index,
                        Tradeskilled = x.Tradeskilled,
                        Implant = new
                        {
                            Name = implant.Slot.GetDescription(),
                            Ql = implantQl.ToString(),
                            Lowid = lowId,
                            Highid = highId,
                            Abilitystat = req == null ? "Unk" : req.GetDescription().Substring(0, 3),
                            Abilityamount = impReq.Ability,
                            Treatmentamount = impReq.Treatment,
                        },
                        Shiny = shinyCluster == null ? null : new
                        {
                            Name = shinyCluster.Stat.GetDescription(),
                            Amount = ImplantDesigner.GetMod(implantQl, shinyCluster),
                            Tradeskilled = shinyCluster.Tradeskilled,
                        },
                        Bright = brightCluster == null ? null : new
                        {
                            Name = brightCluster.Stat.GetDescription(),
                            Amount = ImplantDesigner.GetMod(implantQl, brightCluster),
                            Tradeskilled = brightCluster?.Tradeskilled
                        },
                        Faded = fadedCluster == null ? null : new
                        {
                            Name = fadedCluster.Stat.GetDescription(),
                            Amount = ImplantDesigner.GetMod(implantQl, fadedCluster),
                            Tradeskilled = fadedCluster?.Tradeskilled
                        },
                    };
                }).ToList()
            });

            return template;
        }

        private static object GetColors()
        {
            return new
            {
                Orange = Color.Orange,
                Darkblue = Color.DarkBlue,
                Lightblue = Color.LightBlue,
                Green = Color.Green,
                Implant = Color.Red,
                Cluster = Color.Cluster,
                Ql = Color.Ql,
                Darkgreen = Color.DarkGreen,
                Lightgreen = Color.LightGreen,
                Reqs = Color.Reqs,
            };
        }

        private static Template GetTemplate(string templateFile) => Template.Parse(File.ReadAllText($"{Main.PluginDir}\\Templates\\{templateFile}.txt"));
    }

    public class Color
    {
        public const string Reqs = "7E8439";
        public const string DarkBlue = "398484";
        public const string LightBlue = "96E9ED";
        public const string Orange = "FFC67B";
        public const string Green = "21e052";
        public const string Cluster = "bbc0a6";
        public const string Ql = "ffd800";
        public const string Red = "e53f4b";
        public const string DarkGreen = "92C100";
        public const string LightGreen = "bdff00";
    }
}
