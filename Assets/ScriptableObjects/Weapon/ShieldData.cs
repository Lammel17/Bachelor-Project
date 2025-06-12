using System;
using UnityEngine;
using static WeaponData;

[CreateAssetMenu(fileName = "ShieldData", menuName = "Scriptable Objects/ShieldData")]
public class ShieldData : ScriptableObject
{
    public string Description = "";

    public ShieldAction shieldIdle;
    public ShieldAction shieldingUpperBody;
    



    [System.Serializable]
    public class ShieldAction
    {
        public AnimationData AnimData;
        [NonSerialized] public int ActionkHash;

        ////[ToggleGroup("settings", nameof(DamageEnergyPoise), nameof(PhysicalType), nameof(nextLight), nameof(nextHeavy), nameof(nextSpecialLight), nameof(nextSpecialHeavy))]
        ////[SerializeField] private Void groupHolder;
        //public Vector3 DamageEnergyPoise = new Vector3(1, 0, 0);
        ////public float DamageFactor = 1;
        ////public float EnergyConsumption;
        ////public float PoiseDamage;
        //public PhysicalDamageType PhysicalType;
        //[Space]
        //public LightAttack nextLight;
        //public HeavyAttack nextHeavy;
        //public LightAttackSpecial nextSpecialLight;
        //public HeavyAttackSpecial nextSpecialHeavy;

        //public ShieldMove(float damage, float energyCost, float poiseDamage, PhysicalDamageType type, LightAttack la, HeavyAttack ha, LightAttackSpecial las, HeavyAttackSpecial has, int name)
        //{
        //    DamageEnergyPoise = new Vector3(damage, energyCost, poiseDamage);
        //    PhysicalType = type;
        //    nextLight = la;
        //    nextHeavy = ha;
        //    nextSpecialLight = las;
        //    nextSpecialHeavy = has;
        //    AttackHash = name;
        //}

    }







}
