using UnityEngine;
using System.Collections.Generic;
using System;


[System.Serializable]
public class PlayerEquipmentData
{
    [SerializeField] public WeaponInstanceData Weapon1 = null;
    [SerializeField] public WeaponInstanceData Weapon2 = null;
    [NonSerialized] public Slot ActiveWeaponSlot = Slot.First;
    [Space]
    [SerializeField] public ShieldInstanceData Shield1 = null;
    [SerializeField] public ShieldInstanceData Shield2 = null;
    [NonSerialized] public Slot ActiveShieldSlot = Slot.First;
    [Space]
    [SerializeField] public ImpactCrystalInstanceData ImpactCrystal = null;
    [Space]
    [SerializeField] public List<GearInstanceData> Gears = null;
    [NonSerialized] public int GearCount = 0;
    [Space]
    [SerializeField] public ArmorInstanceData ArmorHead = null;
    [SerializeField] public ArmorInstanceData ArmorChest = null;
    [SerializeField] public ArmorInstanceData ArmorArms = null;
    [SerializeField] public ArmorInstanceData ArmorLegs = null;
    
    [Space]
    [SerializeField] public ItemInstanceData Item1 = null;     //Maybe turn it into an array of 6 
    [SerializeField] public ItemInstanceData Item2 = null;
    [SerializeField] public ItemInstanceData Item3 = null;
    [SerializeField] public ItemInstanceData Item4 = null;
    [SerializeField] public ItemInstanceData Item5 = null;
    [SerializeField] public ItemInstanceData Item6 = null;
    [NonSerialized] public Slot ActiveItemSlot = Slot.First;
    [NonSerialized] public int ItemCount = 0;

    public enum Slot
    {
        First = 1,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth
    }
}
