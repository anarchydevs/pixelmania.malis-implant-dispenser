using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    public class AOSetupsPreset
    {
        internal static bool CanProcess = true;

        private static Dictionary<int, ClusterStat> _aoSetupsClusterRemap = new Dictionary<int, ClusterStat>
        {
            { 2, ClusterStat.OneHandBlunt },
            { 3, ClusterStat.OneHandEdged },
            { 4, ClusterStat.TwoHandBlunt },
            { 5, ClusterStat.TwoHandEdged },
            { 6, ClusterStat.Adventuring },
            { 7, ClusterStat.Agility },
            { 8, ClusterStat.AimedShot },
            { 9, ClusterStat.AssaultRifle },
            { 10, ClusterStat.BiologicalMetamorphosis },
            { 11, ClusterStat.BodyDevelopment },
            { 12, ClusterStat.Bow },
            { 13, ClusterStat.BowSpecialAttack },
            { 14, ClusterStat.Brawl },
            { 15, ClusterStat.BreakAndEntry },
            { 16, ClusterStat.Burst },
            { 17, ClusterStat.ChemicalAC },
            { 18, ClusterStat.Chemistry },
            { 19, ClusterStat.ColdAC },
            { 20, ClusterStat.ComputerLiteracy },
            { 21, ClusterStat.Concealment },
            { 22, ClusterStat.Dimach },
            { 24, ClusterStat.DodgeRanged },
            { 25, ClusterStat.DuckExp },
            { 26, ClusterStat.ElectricalEngineering },
            { 27, ClusterStat.EnergyAC },
            { 28, ClusterStat.EvadeClsC },
            { 29, ClusterStat.FastAttack },
            { 30, ClusterStat.FireAC },
            { 31, ClusterStat.FirstAid },
            { 32, ClusterStat.FlingShot },
            { 33, ClusterStat.FullAuto },
            { 34, ClusterStat.Grenade },
            { 35, ClusterStat.HeavyWeapons },
            { 36, ClusterStat.ProjectileAC },
            { 37, ClusterStat.Intelligence },
            { 38, ClusterStat.MapNavigation },
            { 39, ClusterStat.MartialArts },
            { 40, ClusterStat.MaterialCreation },
            { 41, ClusterStat.MaterialMetamorphosis },
            { 42, ClusterStat.MaxHealth },
            { 43, ClusterStat.MaxNano },
            { 44, ClusterStat.MechanicalEngineering },
            { 45, ClusterStat.MeleeEnergy },
            { 46, ClusterStat.MeleeInit },
            { 47, ClusterStat.MeleeAC },
            { 48, ClusterStat.MGSMG },
            { 49, ClusterStat.MultiMelee },
            { 50, ClusterStat.MultiRanged },
            { 51, ClusterStat.NanoInit },
            { 52, ClusterStat.NanoPool },
            { 53, ClusterStat.NanoProgramming },
            { 54, ClusterStat.NanoResist },
            { 55, ClusterStat.Parry },
            { 56, ClusterStat.Perception },
            { 57, ClusterStat.PharmaTechnology },
            { 58, ClusterStat.PhysicalInit },
            { 59, ClusterStat.Piercing },
            { 60, ClusterStat.Pistol },
            { 61, ClusterStat.Psychic },
            { 62, ClusterStat.PsychologicalModification },
            { 63, ClusterStat.Psychology },
            { 64, ClusterStat.QuantumFT },
            { 65, ClusterStat.RadiationAC },
            { 66, ClusterStat.RangedEnergy },
            { 67, ClusterStat.RangedInit },
            { 68, ClusterStat.Rifle },
            { 69, ClusterStat.Riposte },
            { 70, ClusterStat.RunSpeed },
            { 71, ClusterStat.Sense },
            { 72, ClusterStat.SensoryImprovement },
            { 73, ClusterStat.SharpObject },
            { 74, ClusterStat.Shotgun },
            { 75, ClusterStat.SneakAttack },
            { 76, ClusterStat.Stamina },
            { 77, ClusterStat.Strength },
            { 78, ClusterStat.Swimming },
            { 79, ClusterStat.SpaceTime },
            { 80, ClusterStat.TrapDisarm },
            { 81, ClusterStat.Treatment },
            { 82, ClusterStat.Tutoring },
            { 83, ClusterStat.VehicleAir },
            { 84, ClusterStat.VehicleGround },
            { 85, ClusterStat.VehicleWater },
            { 86, ClusterStat.WeaponSmithing },
            { 87, ClusterStat.NanoDelta },
            { 88, ClusterStat.HealDelta },
            { 89, ClusterStat.AddAllDef },
            { 90, ClusterStat.AddAllOff },
            { 91, ClusterStat.MaxNCU },
            { 92, ClusterStat.XpModifier },
            { 93, ClusterStat.NanoInterruptModifier },
            { 94, ClusterStat.ChemicalDamage },
            { 95, ClusterStat.EnergyDamage },
            { 96, ClusterStat.FireDamage },
            { 97, ClusterStat.MeleeDamage },
            { 98, ClusterStat.PoisonDamage },
            { 99, ClusterStat.ProjectileDamage },
            { 100, ClusterStat.RadiationDamage },
            { 101, ClusterStat.ShieldChemicalAC },
            { 102, ClusterStat.ShieldColdAC },
            { 103, ClusterStat.ShieldEnergyAC },
            { 104, ClusterStat.ShieldFireAC },
            { 105, ClusterStat.ShieldMeleeAC },
            { 106, ClusterStat.ShieldPoisonAC },
            { 107, ClusterStat.ShieldProjectileAC },
            { 108, ClusterStat.ShieldRadiationAC },
            { 109, ClusterStat.SkillLockModifier },
            { 110, ClusterStat.NanoCostModifier },
            { 111, ClusterStat.RangeIncNano },
            { 112, ClusterStat.PoisonAC },
            { 130, ClusterStat.RangeIncWeapon },
        };
        private static Dictionary<string, ImplantSlot> _aoSetupsImplantRemap = new Dictionary<string, ImplantSlot>
        {
            { "eye", ImplantSlot.Eye },
            { "head", ImplantSlot.Head },
            { "ear", ImplantSlot.Ear },
            { "rarm", ImplantSlot.RightArm },
            { "chest", ImplantSlot.Chest },
            { "larm", ImplantSlot.LeftArm },
            { "rwrist", ImplantSlot.RightWrist },
            { "waist", ImplantSlot.Waist },
            { "lwrist", ImplantSlot.LeftWrist },
            { "rhand", ImplantSlot.RightHand },
            { "legs", ImplantSlot.Leg },
            { "lhand", ImplantSlot.LeftHand },
            { "feet", ImplantSlot.Feet }
        };


        internal async static Task<List<AOSImplantData>> GetImplants(string id)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"https://aosetups.com/api/equip/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                List<AOSImplantData> implantData = new List<AOSImplantData>();

                AOSetupsApi setupsPage = JsonConvert.DeserializeObject<AOSetupsApi>(await response.Content.ReadAsStringAsync());

                foreach (AOSImplantData implant in setupsPage.implants)
                {
                    if (implant.type == "symbiant" || implant.clusters == null)
                        continue;
                    implantData.Add(implant);
                }

                CanProcess = true;
                return implantData;  
            }
            catch (Exception ex)
            {
                CanProcess = true;

                Logger.Warning(ex.Message);
                return new List<AOSImplantData>();
            }
        }

        internal class AOSetupsApi
        {
            public List<AOSImplantData> implants { get; set; }
        }

        internal class AOSImplantData
        {
            public string slot { get; set; }
            public string type { get; set; }
            public int ql { get; set; }
            public Dictionary<ClusterType, AOSCluster> clusters { get; set; }

            public ImplantSlot GetSlot() => _aoSetupsImplantRemap[slot];
            public ClusterStat GetCluster(int cluster) => _aoSetupsClusterRemap[cluster];
        }

        internal class AOSCluster
        {
            public int ClusterID { get; set; }
        }
    }
}