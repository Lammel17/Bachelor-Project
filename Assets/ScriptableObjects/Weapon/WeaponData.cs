using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string Description = "";

    public PhysicalDamageType BasePhysicalType = PhysicalDamageType.Normal;

    public DamageTable Damage;
    public AnimationCurve UpgradeCurve = AnimationCurve.Linear(0, 0, 1, 1);


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
    public WeaponAttack SpecialHeavyAttack2;

    [Space]
    [Header("Alternate Attack")]
    public AlternateAttacks alternateAttacks;


    [System.Serializable]
    public class WeaponAttack
    {
        public AnimationData AnimData;
        public float DamageFactor = 1;
        public float EnergyConsumption;
        public float PoiseDamage;
        public PhysicalDamageType PhysicalType;

    }

    [System.Serializable]
    public enum PhysicalDamageType
    {
        TypeByBase = 0,
        Normal,
        Slice,
        Blunt,
        Pierce,
        None
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




    public WeaponActionCount weaponActionCount = new WeaponActionCount();


    //this class counts the number of anim that a weapon has of those attack types
    public class WeaponActionCount
    {
        public int LightAttacks = 0;
        public int SprintLightAttacks = 0;
        public int EvadeLightAttacks = 0;
        public int SpecialLightAttacks = 0;

        public int HeavyAttacks = 0;
        public int SprintheavyAttacks = 0;
        public int EvadeHeavyAttacks = 0;
        public int SpecialHeavyAttacks = 0;
    }


}
