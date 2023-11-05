using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisImpDispenser
{
    public class PremadeImplant
    {
        public ImplantItem Implant;
        public List<ClusterItem> Clusters;
        public KeyValuePair<ClusterStat, int> AbilityReq => Requirement().FirstOrDefault(x => x.Key != ClusterStat.Treatment);
        public KeyValuePair<ClusterStat, int> TreatmentReq => Requirement().FirstOrDefault(x => x.Key == ClusterStat.Treatment);

        public Dictionary<ClusterStat, int> Requirement()
        {
            Dictionary<ClusterStat,int> reqs = new Dictionary<ClusterStat, int>();   
          
            if (!ImplantDesigner.GetImplantInfo(this, out _, out _, out ClusterStat? req))
                return reqs;

            var reqInfo = ImplantDesigner.GetReq(Implant.Ql, Clusters);

            reqs.Add(req.Value, reqInfo.Ability);
            reqs.Add(ClusterStat.Treatment, reqInfo.Treatment);

            return reqs;
        }

        public Dictionary<ClusterStat, int> Modifier()
        {
            Dictionary<ClusterStat, int> mods = new Dictionary<ClusterStat, int>();
        
            foreach (var cluster in Clusters)
                mods.Add(cluster.Stat, ImplantDesigner.GetMod(Implant.Ql, cluster));

            return mods;
        }
    }

    public class SkillInfo
    {
        public ClusterStat Stat;
        public int Amount;
    }

    public static class LadderImplant
    {
        public static PremadeImplant TreatmentHead = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Head },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.Treatment },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Sense }
            },
        };

        public static PremadeImplant TreatmentEye = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Eye },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.Rifle },
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Treatment },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Pistol }
            },
        };

        public static PremadeImplant TreatmentRightHand = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.RightHand },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Pistol },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Treatment }
            },
        };

        public static PremadeImplant AgiStamLeg = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Leg },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.Agility },
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Stamina },
            },
        };

        public static PremadeImplant StamStrChest = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Chest },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.Stamina },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Strength },
            },
        };

        public static PremadeImplant AgiFoot = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Feet },
            Clusters = new List<ClusterItem>
            {                
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.SneakAttack },
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Agility },
            },
        };

        public static PremadeImplant SenseAgiWaist = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Waist },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.ColdAC },
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Sense },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Agility },
            },
        }; 
        
        public static PremadeImplant SenseStamWaist = new PremadeImplant
        {
            Implant = new ImplantItem { Ql = 50, Slot = ImplantSlot.Waist },
            Clusters = new List<ClusterItem>
            {
                new ClusterItem { Type = ClusterType.Shiny, Stat = ClusterStat.ColdAC },
                new ClusterItem { Type = ClusterType.Bright, Stat = ClusterStat.Sense },
                new ClusterItem { Type = ClusterType.Faded, Stat = ClusterStat.Stamina },
            },
        };
    }
    public static class LadderCommand
    {
        internal static void Process(LadderCmd cmd)
        {
            if (!Enum.TryParse(Utils.ToTitleCase(cmd.Skill1), out ClusterStat stat1) || 
                !Enum.TryParse(Utils.ToTitleCase(cmd.Skill2), out ClusterStat stat2) || 
                !Enum.TryParse(Utils.ToTitleCase(cmd.Skill3), out ClusterStat stat3))
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "Skill Error: Invalid skill (valid skills: treatment)"));
                return;
            }

            Dictionary<ClusterStat, int> baseSkills = new Dictionary<ClusterStat, int>();

            baseSkills.Add(stat1, cmd.Amount1);
            baseSkills.Add(stat2, cmd.Amount2);
            baseSkills.Add(stat3, cmd.Amount3);

            if (!baseSkills.TryGetValue(ClusterStat.Agility, out int agiStat) ||
                !baseSkills.TryGetValue(ClusterStat.Stamina, out int stamStat) ||
                !baseSkills.TryGetValue(ClusterStat.Treatment, out int trtStat))
                return;

            if (trtStat < 242 || agiStat < 104 || stamStat < 104)
            {
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Red, $"Can't ladder implants below ql 50. Cancelling request."));
                return;
            }
            var trtImps = new List<PremadeImplant> { LadderImplant.TreatmentHead, LadderImplant.TreatmentEye, LadderImplant.TreatmentRightHand };
            var agiImps = new List<PremadeImplant> { LadderImplant.StamStrChest, LadderImplant.SenseStamWaist, LadderImplant.AgiStamLeg, LadderImplant.AgiFoot, LadderImplant.SenseAgiWaist };


            while (true)
            {
                LadderCycle(cmd.RequesterId, GetSkillsWithImps(agiImps, baseSkills), trtImps, out var presets1);

                foreach (var preset in presets1)
                    OrderProcessor.SetOrder(preset);

                LadderCycle(cmd.RequesterId, GetSkillsWithImps(trtImps, baseSkills), agiImps, out var presets2);

                foreach (var preset in presets2)
                    OrderProcessor.SetOrder(preset);

                if (presets1.Count == 0 && presets2.Count == 0)
                    break;
            }

            Dictionary<ClusterStat, int> newSkills = GetSkillsWithImps(agiImps, baseSkills);
            newSkills = GetSkillsWithImps(trtImps, newSkills);

            //while (true)
            //{
            //    Logger.Information(baseSkills[ClusterStat.Treatment].ToString());
            //    LadderCycle(cmd.RequesterId, baseSkills, trtImps, out var presets1);

            //    foreach (var preset in presets1)
            //        OrderProcessor.SetOrder(preset);

            //    LadderCycle(cmd.RequesterId, baseSkills, trtImps, out var presets2);

            //    foreach (var preset in presets2)
            //        OrderProcessor.SetOrder(preset);

            //    //LadderCycle(cmd.RequesterId, skills, agiImps, out var presets2);

            //    //foreach (var preset in presets2)
            //    //    OrderProcessor.SetOrder(preset);

            //    if (presets2.Count == 0 && presets1.Count == 0)
            //        break;
            //}

            //  Logger.Information($"{preset.GetImplant().Ql} {preset.GetImplant().Slot.GetDescription()}");

            if (!OrderProcessor.Orders.TryGetValue(cmd.RequesterId, out var order))
                return;

            foreach (var skill in newSkills)
                Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.RespondMsg(Color.Green, $"{skill.Key} increased from: {baseSkills[skill.Key]} to {skill.Value}"));

            Client.SendPrivateMessage(cmd.RequesterId, ScriptTemplate.OrderTicket(order));
        }

        private static Dictionary<ClusterStat, int> GetSkillsWithImps(List<PremadeImplant> imps, Dictionary<ClusterStat, int> startSkills)
        {
            Dictionary<ClusterStat, int> stats = startSkills.ToDictionary(x => x.Key, y => y.Value);

            foreach (var skill in stats.ToDictionary(x => x.Key, y => y.Value))
            {
                foreach (var imp in imps)
                {
                    if (!imp.Modifier().TryGetValue(skill.Key, out int skillAmount))
                        continue;

                    stats[skill.Key] += skillAmount;
                }
            }

            return stats;
        }

        private static void TotalSkillMinusCluster(Dictionary<ClusterStat, int> totalSkills, PremadeImplant implant, out int abilitySkill, out int treatmentSkill)
        {
            abilitySkill = totalSkills[implant.AbilityReq.Key];
            treatmentSkill = totalSkills[implant.TreatmentReq.Key];

            if (implant.Clusters.Any(x => x.Stat == implant.AbilityReq.Key))
            {
                abilitySkill = abilitySkill - implant.Modifier()[implant.AbilityReq.Key];
            }

            if (implant.Clusters.Any(x => x.Stat == implant.TreatmentReq.Key))
            {
                treatmentSkill = treatmentSkill - implant.Modifier()[implant.TreatmentReq.Key];
            }
        }

        private static void LadderCycle(int requesterId, Dictionary<ClusterStat, int> skills, List<PremadeImplant> ladderImps, out List<ImplantPreset> presets)
        {
            presets = new List<ImplantPreset>();
            Dictionary<ClusterStat, int> startSkills = skills.ToDictionary(x => x.Key, y => y.Value);

          //  List<PremadeImplant> currentImps = ladderImps.Select(x => new PremadeImplant { Clusters = x.Clusters.Select(y => new ClusterItem { Stat = y.Stat, Type = y.Type }).ToList(), Implant = new ImplantItem { Slot = x.Implant.Slot, Ql = x.Implant.Ql } }).ToList();

            bool skillChange = true;

            while (skillChange)
            {
                var totalSkills = GetSkillsWithImps(ladderImps, startSkills);

                skillChange = false;

                for (int i = 0; i < ladderImps.Count(); i++)
                {
                    var ladderImp = ladderImps[i];
                    var currentMods = ladderImp.Modifier().ToDictionary(x => x.Key, y => y.Value);

                    TotalSkillMinusCluster(totalSkills, ladderImp, out int abilitySkill, out int treatSkill);
                    ImplantDesigner.GetBestQl(abilitySkill, treatSkill, ladderImp.Clusters, null, out int ql, out _);
                    ladderImp.Implant.Ql = ql;

                    var newTValues = ladderImp.Modifier().Where(x => startSkills.ContainsKey(x.Key)).Select(x => x.Value);
                    var oldValues = currentMods.Where(x => startSkills.ContainsKey(x.Key)).Select(x => x.Value);

                    if (!newTValues.SequenceEqual(oldValues))
                    {
                        presets.Add(ImplantDesigner.MakePreset(requesterId, ql, ladderImp.Implant.Slot, ladderImp.Clusters.Select(x => new ClusterItem { Stat = x.Stat, Type = x.Type }).ToList()));
                        skillChange = true;
                    }

                    totalSkills = GetSkillsWithImps(ladderImps, startSkills);
                }
            }

            //List<PremadeImplant> startImps = ladderImps.Select(x => new PremadeImplant { Clusters = x.Clusters.Select(y => new ClusterItem { Stat = y.Stat, Type = y.Type }).ToList(), Implant = new ImplantItem { Slot = x.Implant.Slot, Ql = x.Implant.Ql } }).ToList();

            //Dictionary<ClusterStat, int> startSkills = skills.ToDictionary(x => x.Key, y => y.Value);
            //bool hasChanged = true;

            //presets = new List<ImplantPreset>();
            //while (hasChanged)
            //{
            //  //  Logger.Information("CYCLE");
            //    hasChanged = false; // Reset the flag before each iteration

            //    for (int i = 0; i < ladderImps.Count(); i++)
            //    {
            //        var ladderImp = ladderImps[i];

            //        foreach (var skill in skills.ToDictionary(x=>x.Key,y=>y.Value))
            //        {
            //            if (!ladderImp.Modifier().TryGetValue(skill.Key, out var item))
            //                continue;

            //            var ladderMod = ladderImp.Implant.Ql != 50 ? item : 0;
            //            skills[skill.Key] = startSkills[skill.Key] + ladderImps.Where(x => x.Implant.Ql != 50).SelectMany(x => x.Modifier().Where(y => y.Key == skill.Key).Select(y => y.Value)).Sum() - ladderMod;
            //        }

            //        if (!ImplantDesigner.GetBestQl(skills[ladderImp.AbilityReq.Key], skills[ClusterStat.Treatment], ladderImp.Clusters, null, out int ql, out string error))
            //            Logger.Information(error);

            //        ladderImp.Implant.Ql = ql;


            //        if (!ladderImp.Modifier().Values.SequenceEqual(startImps[i].Modifier().Values))
            //        {
            //            startImps[i].Implant.Ql = ql;

            //            presets.Add(ImplantDesigner.MakePreset(requesterId, ql, ladderImp.Implant.Slot, ladderImp.Clusters.Select(x => new ClusterItem { Stat = x.Stat, Type = x.Type })));
            //            hasChanged = true;
            //        }
            //    }
            //}

            //for (int i = 0; i < ladderImps.Count(); i++)
            //{
            //    var ladderImp = ladderImps[i];

            //    foreach (var skill in skills.ToDictionary(x => x.Key, y => y.Value))
            //    {
            //        if (!ladderImp.Modifier().TryGetValue(skill.Key, out var item))
            //            continue;

            //        var ladderMod = ladderImp.Implant.Ql != 50 ? item : 0;
            //        skills[skill.Key] = startSkills[skill.Key] + ladderImps.Where(x => x.Implant.Ql != 50).SelectMany(x => x.Modifier().Where(y => y.Key == skill.Key).Select(y => y.Value)).Sum();
            //    }
            //}
        }
    }
}   