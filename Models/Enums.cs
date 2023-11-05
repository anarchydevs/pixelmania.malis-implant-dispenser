using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Common.SharedEventArgs;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using System.ComponentModel;

namespace MalisImpDispenser
{
    public enum ClusterType
    {
        Shiny,
        Bright,
        Faded
    }

    public enum ImplantSlot
    {
        [Description("Eye")]
        Eye = 33,
        [Description("Head")]
        Head = 34,
        [Description("Ear")]
        Ear = 35,
        [Description("Right Arm")]
        RightArm = 36,
        [Description("Chest")]
        Chest = 37,
        [Description("Left Arm")]
        LeftArm = 38,
        [Description("Right Wrist")]
        RightWrist = 39,
        [Description("Waist")]
        Waist = 40,
        [Description("Left Wrist")]
        LeftWrist = 41,
        [Description("Right Hand")]
        RightHand = 42,
        [Description("Leg")]
        Leg = 43,
        [Description("Left Hand")]
        LeftHand = 44,
        [Description("Feet")]
        Feet = 45,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class TradeskillIdAttribute : Attribute
    {
        public ClusterStat Stat { get; }

        public TradeskillIdAttribute(ClusterStat stat)
        {
            Stat = stat;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class StatTypeAttribute : Attribute
    {
        public StatType Stat { get; }

        public StatTypeAttribute(StatType stat)
        {
            Stat = stat;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class SkillMultiplierAttribute : Attribute
    {
        public float Multiplier { get; }

        public SkillMultiplierAttribute(float multiplier)
        {
            Multiplier = multiplier;
        }
    }

    public enum StatType
    {
        Ability,
        Skill,
        Jobe,
        Ac
    }
    public enum ClusterStat
    {
        [Description("Max Health")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.5f)]
        MaxHealth = 1,

        [Description("Strength")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Strength = 16,
     
        [Description("Agility")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Agility = 17,
        
        [Description("Stamina")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Stamina = 18,
        
        [Description("Intelligence")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Intelligence = 19,
        
        [Description("Sense")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Sense = 20,
        
        [Description("Psychic")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ability)]
        [SkillMultiplier(2.25f)]
        Psychic = 21,

        [Description("Projectile AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.25f)]
        ProjectileAC = 90,
        
        [Description("Melee AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.25f)]
        MeleeAC = 91,
        
        [Description("Energy AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.25f)]
        EnergyAC = 92,
        
        [Description("Chemical AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.0f)]
        ChemicalAC = 93,
        
        [Description("Radiation AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.0f)]
        RadiationAC = 94,
        
        [Description("Cold AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.0f)]
        ColdAC = 95,
        
        [Description("Disease AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(1.75f)] //Is this disease?
        PoisonAC = 96,
        
        [Description("Fire AC")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Ac)]
        [SkillMultiplier(2.0f)]
        FireAC = 97,
        
        [Description("Martial Arts")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.5f)]
        MartialArts = 100,
        
        [Description("Multi Melee")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.25f)]
        MultiMelee = 101,
        
        [Description("1h Blunt")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        OneHandBlunt = 102,
        
        [Description("1h Edged")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.9f)]
        OneHandEdged = 103,
        
        [Description("Melee Energy")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        MeleeEnergy = 104,
        
        [Description("2h Edged")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.9f)]
        TwoHandEdged = 105,
        
        [Description("Piercing")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.6f)]
        Piercing = 106,
        
        [Description("2h Blunt")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        TwoHandBlunt = 107,
        
        [Description("Sharp Object")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.25f)]
        SharpObject = 108,
        
        [Description("Grenade")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.9f)]
        Grenade = 109,
        
        [Description("Heavy Weapons")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.0f)]
        HeavyWeapons = 110,
        
        [Description("Bow")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        Bow = 111,
        
        [Description("Pistol")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        Pistol = 112,
        
        [Description("Rifle")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.25f)]
        Rifle = 113,
        
        [Description("MG / SMG")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        MGSMG = 114,
        
        [Description("Shotgun")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.7f)]
        Shotgun = 115,
        
        [Description("Assault Rifle")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.25f)]
        AssaultRifle = 116,
        
        [Description("Vehicle Water")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.2f)]
        VehicleWater = 117,
        
        [Description("Melee Init")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        MeleeInit = 118,
        
        [Description("Ranged Init")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        RangedInit = 119,
        
        [Description("Physical Init")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        PhysicalInit = 120,
        
        [Description("Bow Special Attack")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        BowSpecialAttack = 121,
        
        [Description("Sensory Improvement")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.2f)]
        SensoryImprovement = 122,
        
        [Description("First Aid")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        FirstAid = 123,
        
        [Description("Treatment")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.15f)]
        Treatment = 124,
        
        [Description("Mechanical Engineering")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        MechanicalEngineering = 125,
        
        [Description("Electrical Engineering")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        ElectricalEngineering = 126,
        
        [Description("Material Metamorphosis")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.4f)]
        MaterialMetamorphosis = 127,
        
        [Description("Biological Metamorphosis")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.4f)]
        BiologicalMetamorphosis = 128,
        
        [Description("Psychological Modification")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.4f)]
        PsychologicalModification = 129,
        
        [Description("Material Creation")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.4f)]
        MaterialCreation = 130,
        
        [Description("Time & Space")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.4f)]
        SpaceTime = 131,
        
        [Description("Nano Pool")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(3.0f)]
        NanoPool = 132,
        
        [Description("Ranged Energy")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        RangedEnergy = 133,
        
        [Description("Multi Ranged")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        MultiRanged = 134,
        
        [Description("Trap Disarm")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        TrapDisarm = 135,
        
        [Description("Perception")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        Perception = 136,
        
        [Description("Adventuring")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.5f)]
        Adventuring = 137,

        [Description("Swimming")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.25f)]
        Swimming = 138,

        [Description("Vehicle Air")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.0f)]
        VehicleAir = 139,

        [Description("Map Navigation")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.25f)]
        MapNavigation = 140,

        [Description("Tutoring")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.3f)]
        Tutoring = 141,
        
        [Description("Brawling")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.65f)]
        Brawl = 142,
        
        [Description("Riposte")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.5f)]
        Riposte = 143,
        
        [Description("Dimach")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.25f)]
        Dimach = 144,
        
        [Description("Parry")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.1f)]
        Parry = 145,
        
        [Description("Sneak Attack")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.5f)]
        SneakAttack = 146,
        
        [Description("Fast Attack")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.9f)]
        FastAttack = 147,
        
        [Description("Burst")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.1f)]
        Burst = 148,
        
        [Description("NanoC. Init")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        NanoInit = 149,
   
        [Description("Fling Shot")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        FlingShot = 150,
        
        [Description("Aimed Shot")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.1f)]
        AimedShot = 151,
        
        [Description("Body Development")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        BodyDevelopment = 152,
        
        [Description("Duck Explosives")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        DuckExp = 153,
        
        [Description("Dodge Ranged")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        DodgeRanged = 154,
        
        [Description("Evade Close Combat")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        EvadeClsC = 155,
        
        [Description("Run Speed")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        RunSpeed = 156,
        
        [Description("Quantum Field Technology")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        QuantumFT = 157,
        
        [Description("Weapon Smithing")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        WeaponSmithing = 158,
        
        [Description("Pharma Technology")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        PharmaTechnology = 159,
        
        [Description("Nano Programming")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        NanoProgramming = 160,
        
        [Description("Computer Literacy")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        ComputerLiteracy = 161,
        
        [Description("Psychology")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        Psychology = 162,
        
        [Description("Chemistry")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        Chemistry = 163,
        
        [Description("Concealment")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.8f)]
        Concealment = 164,
        
        [Description("Break & Entry")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        BreakAndEntry = 165,
        
        [Description("Vehicle Ground")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(1.5f)]
        VehicleGround = 166,
        
        [Description("Full Auto")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.25f)]
        FullAuto = 167,
        
        [Description("Nano Resist")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.0f)]
        NanoResist = 168,
        
        [Description("Max NCU")]
        [TradeskillId(ComputerLiteracy)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        MaxNCU = 181,
        
        [Description("Max Nano")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Skill)]
        [SkillMultiplier(2.5f)]
        MaxNano = 221,
        
        [Description("Shield Projectile AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldProjectileAC = 226,
        
        [Description("Shield Melee AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldMeleeAC = 227,
        
        [Description("Shield Energy AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldEnergyAC = 228,
        
        [Description("Shield Chemical AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldChemicalAC = 229,
        
        [Description("Shield Radiation AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldRadiationAC = 230,
        
        [Description("Shield Cold AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldColdAC = 231,
        
        [Description("Shield Fire AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldFireAC = 233,
        
        [Description("Shield Poison AC")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ShieldPoisonAC = 234,
        
        [Description("Add All Offensive")]
        [TradeskillId(Psychology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        AddAllOff = 276,
        
        [Description("Add All Defensive")]
        [TradeskillId(Psychology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        AddAllDef = 277,
        
        [Description("Add Projectile Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ProjectileDamage = 278,
        
        [Description("Add Melee Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        MeleeDamage = 279,
        
        [Description("Add Energy Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        EnergyDamage = 280,
        
        [Description("Add Chemical Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        ChemicalDamage = 281,
        
        [Description("Add Radiation Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        RadiationDamage = 282,
        
        [Description("Add Fire Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        FireDamage = 316,
        
        [Description("Add Poison Damage")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        PoisonDamage = 317,
        
        [Description("Nano Cost Modifier")]
        [TradeskillId(QuantumFT)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        NanoCostModifier = 318,
        
        [Description("Experience Modifier")]
        [TradeskillId(Psychology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        XpModifier = 319,
        
        [Description("HealDelta")]
        [TradeskillId(PharmaTechnology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        HealDelta = 343,
        
        [Description("NanoDelta")]
        [TradeskillId(PharmaTechnology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        NanoDelta = 364,
        
        [Description("RangeInc. Weapon")]
        [TradeskillId(WeaponSmithing)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        RangeIncWeapon = 380,
        
        [Description("RangeInc. Nano")]
        [TradeskillId(NanoProgramming)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        RangeIncNano = 381,
        
        [Description("Skill Lock Modifier")]
        [TradeskillId(Psychology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        SkillLockModifier = 382,
        
        [Description("Nano Interrupt Modifier")]
        [TradeskillId(Psychology)]
        [StatType(StatType.Jobe)]
        [SkillMultiplier(1f)]
        NanoInterruptModifier = 383,
    }

    public enum ShopType
    {
        [Description("Advanced ICC Shiny Clusters")]
        AdvancedICCShinyClusters,
        [Description("Advanced ICC Bright Clusters")]
        AdvancedICCBrightClusters,
        [Description("Advanced ICC Faded Clusters")]
        AdvancedICCFadedClusters,
        [Description("Advanced ICC Implants")]
        AdvancedICCImplants,
        [Description("Basic ICC Implants")]
        BasicICCImplants
    }
}