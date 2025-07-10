using NUnit.Framework;
using UnityEngine;
using EditorAttributes;
using System;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string Description = "";

    public PhysicalDamageType BasePhysicalType = PhysicalDamageType.Slice;

    public DamageTableData DamageTabel;


    [Header("Light Attack")]
    public WeaponAttack LightAttack1 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_1"));
    public WeaponAttack LightAttack2 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_3, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_2"));
    public WeaponAttack LightAttack3 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_4, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_3"));
    public WeaponAttack LightAttack4 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_5, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_4"));
    public WeaponAttack LightAttack5 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_6, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_5"));
    public WeaponAttack LightAttack6 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.end, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Light_Attack_6"));
    public WeaponAttack SprintLightAttack =     new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Sprint_Light_Attack"));
    public WeaponAttack EvadeLightAttack =      new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_1, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Evade_Light_Attack"));

    public WeaponAttack SpecialLightAttack1 =   new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_2, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Light_Attack_1"));
    public WeaponAttack SpecialLightAttack2 =   new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_2, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Light_Attack_1"));

    [Space]
    [Header("Heavy Attack")]
    public WeaponAttack HeavyAttack1 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_1"));
    public WeaponAttack HeavyAttack2 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_3, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_2"));
    public WeaponAttack HeavyAttack3 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_4, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_3"));
    public WeaponAttack HeavyAttack4 =          new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.end, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Heavy_Attack_4"));
    public WeaponAttack SprintHeavyAttack =     new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Sprint_Heavy_Attack"));
    public WeaponAttack EvadeHeavyAttack =      new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Evade_Heavy_Attack"));

    public WeaponAttack SpecialHeavyAttack1 =   new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_2, Animator.StringToHash("Special_Heavy_Attack_1"));
    public WeaponAttack SpecialHeavyAttack2 =   new WeaponAttack(PhysicalDamageType.TypeByBase, LightAttack.Light_Attack_1, HeavyAttack.Heavy_Attack_2, LightAttackSpecial.Special_Light_Attack_1, HeavyAttackSpecial.Special_Heavy_Attack_1, Animator.StringToHash("Special_Heavy_Attack_2"));

    [Space]
    [Header("Alternate Attack")]
    public AlternateAttacks alternateAttacks; //Still unused


    [System.Serializable]
    public class WeaponAttack
    {
        public AnimationData AnimData;
        [NonSerialized] public int AttackHash;
        public ActionDamageData actionDamageData = new ActionDamageData(1, 1, 1, 1);
        public int EnergyCost = 20;
        public float SpecialEnergyCost = 0;
        public PhysicalDamageType PhysicalType;
        [Space]
        public LightAttack nextLight;
        public HeavyAttack nextHeavy;
        public LightAttackSpecial nextSpecialLight;
        public HeavyAttackSpecial nextSpecialHeavy;

        public WeaponAttack(/*float damage, float poiseDamage, float energyCost, float specialEnergyCost,*/ PhysicalDamageType type, LightAttack la, HeavyAttack ha, LightAttackSpecial las, HeavyAttackSpecial has, int name)
        {
            //Dmg_Poise_EP_SEP = new Vector4(damage, poiseDamage, energyCost, specialEnergyCost);
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
