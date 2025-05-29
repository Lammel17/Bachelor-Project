using NUnit.Framework;
using UnityEngine;
using EditorAttributes;
using System;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string Description = "";

    public PhysicalDamageType BasePhysicalType = PhysicalDamageType.Slice;

    public DamageTable Damage;
    public AnimationCurve UpgradeCurve = AnimationCurve.Linear(0, 0, 1, 1);


    [Header("Light Attack")]
    public WeaponAttack LightAttack1 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_1"));
    public WeaponAttack LightAttack2 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_3, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_2"));
    public WeaponAttack LightAttack3 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_4, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_3"));
    public WeaponAttack LightAttack4 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_5, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_4"));
    public WeaponAttack LightAttack5 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_6, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_5"));
    public WeaponAttack LightAttack6 =          new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.end,            HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_6"));
    public WeaponAttack SprintLightAttack =     new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Sprint_Light_Attack"));
    public WeaponAttack EvadeLightAttack =      new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Evade_Light_Attack"));

    public WeaponAttack SpecialLightAttack1 =   new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_2, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Light_Attack_1"));
    public WeaponAttack SpecialLightAttack2 =   new WeaponAttack(1, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Light_Attack_1"));

    [Space]
    [Header("Heavy Attack")]
    public WeaponAttack HeavyAttack1 =          new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_1"));
    public WeaponAttack HeavyAttack2 =          new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_3, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_2"));
    public WeaponAttack HeavyAttack3 =          new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_4, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_3"));
    public WeaponAttack HeavyAttack4 =          new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.end,            LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_4"));
    public WeaponAttack SprintHeavyAttack =     new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Sprint_Heavy_Attack"));
    public WeaponAttack EvadeHeavyAttack =      new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Evade_Heavy_Attack"));

    public WeaponAttack SpecialHeavyAttack1 =   new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_2, Animator.StringToHash("Special_Heavy_Attack_1"));
    public WeaponAttack SpecialHeavyAttack2 =   new WeaponAttack(1.5f, 0, 0, PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Heavy_Attack_2"));

    [Space]
    [Header("Alternate Attack")]
    public AlternateAttacks alternateAttacks;


    [System.Serializable]
    public class WeaponAttack
    {
        public AnimationData AnimData;
        [NonSerialized] public int AttackHash;

        //[ToggleGroup("settings", nameof(DamageEnergyPoise), nameof(PhysicalType), nameof(nextLight), nameof(nextHeavy), nameof(nextSpecialLight), nameof(nextSpecialHeavy))]
        //[SerializeField] private Void groupHolder;
        public Vector3 DamageEnergyPoise = new Vector3(1, 0, 0);
        //public float DamageFactor = 1;
        //public float EnergyConsumption;
        //public float PoiseDamage;
        public PhysicalDamageType PhysicalType;
        [Space]
        public LightAttack nextLight;
        public HeavyAttack nextHeavy;
        public LightAttackSpecial nextSpecialLight;
        public HeavyAttackSpecial nextSpecialHeavy;

        public WeaponAttack(float damage, float energyCost, float poiseDamage, PhysicalDamageType type, LightAttack la, HeavyAttack ha, LightAttackSpecial las, HeavyAttackSpecial has, int name)
        {
            DamageEnergyPoise = new Vector3(damage, energyCost, poiseDamage);
            PhysicalType = type;
            nextLight = la;
            nextHeavy = ha;
            nextSpecialLight = las;
            nextSpecialHeavy = has;
            AttackHash = name;
        }

    }

    [System.Serializable]
    public enum PhysicalDamageType
    {
        TypeByBase = 0,
        Slice,
        Blunt,
        Pierce,
        None
    }

    public enum LightAttack
    {
        Light_Attack_1,
        Light_Attack_2,
        Light_Attack_3,
        Light_Attack_4,
        Light_Attack_5,
        Light_Attack_6,
        Sprint_Light_Attack,
        Evade_Light_Attack,
        end
    }
    public enum LightAttackSpecial
    {
        Special_Light_Attack_1,
        Special_Light_Attack_2,
        end
    }
    public enum HeavyAttack
    {
        Heavy_Attack_1,
        Heavy_Attack_2,
        Heavy_Attack_3,
        Heavy_Attack_4,
        Sprint_Heavy_Attack,
        Evade_Heavy_Attack,
        end
    }
    public enum HeavyAttackSpecial
    {
        Special_Heavy_Attack_1,
        Special_Heavy_Attack_2,
        end
    }


    [System.Serializable]
    public class DamageTable
    {
        public Vector2 PhysicalSlice = new Vector2(0, 0);
        public Vector2 PhysicalBlunt = new Vector2(0, 0);
        public Vector2 PhysicalPierce = new Vector2(0, 0);
        public Vector2 Thermal = new Vector2(0, 0);
        public Vector2 Electrical = new Vector2(0, 0);
        public Vector2 Metaphysical = new Vector2(0, 0);

        public Vector2 CorrosionBuildUp = new Vector2(0, 0);
    }

    [System.Serializable]
    public class AlternateAttacks
    {
        [Header("Light Attack")]
        public WeaponAttack LightAttack1;
        public WeaponAttack LightAttack2;
        public WeaponAttack LightAttack3;
        public WeaponAttack LightAttack4;
        public WeaponAttack LightAttack5;
        public WeaponAttack LightAttack6;
        public WeaponAttack SprintLightAttack;
        public WeaponAttack EvadeLightAttack;
        public WeaponAttack SpecialLightAttack1;
        public WeaponAttack SpecialLightAttack2;

        [Header("Heavy Attack")]
        public WeaponAttack HeavyAttack1;
        public WeaponAttack HeavyAttack2;
        public WeaponAttack HeavyAttack3;
        public WeaponAttack HeavyAttack4;
        public WeaponAttack SprintHeavyAttack;
        public WeaponAttack EvadeHeavyAttack;
        public WeaponAttack SpecialHeavyAttack1;
        public WeaponAttack SpeciaHeavyAttack2;
    }

















}
