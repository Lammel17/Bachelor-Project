using System;
using UnityEngine;

[RequireComponent(typeof(CharacterActionAndMovementHandler))]

public class EquipmentHandler : MonoBehaviour
{
    [NonSerialized] public static EquipmentHandler Instance;

    private CharacterActionAndMovementHandler m_characterActionAndMovement;
    [SerializeField][EditorAttributes.ReadOnly] private PlayerEquipmentData m_playerEquipmentData = new PlayerEquipmentData();
    private CharacterMovesetData m_movesetData;
    [Space]
    [SerializeField] private WeaponInstanceData m_defaultEmptyWeapon;
    [SerializeField] private ShieldInstanceData m_defaultEmptyShield;
    [Space]
    [SerializeField] private bool m_clearActiveEquipmentMovesetAtStart = false;
    [SerializeField] private bool m_useCheatEquipment = false;
    [SerializeField] private PlayerEquipmentData m_cheatEquipmentData = null;
    private Animator m_animator;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        m_characterActionAndMovement = GetComponent<CharacterActionAndMovementHandler>();
        m_movesetData = m_characterActionAndMovement.MovesetData;

        if (m_clearActiveEquipmentMovesetAtStart)
        {
            m_movesetData.weapon = m_defaultEmptyWeapon.WeaponData;
            m_movesetData.shield = m_defaultEmptyShield.ShieldData;
            m_movesetData.item = null;
        }

        m_animator = m_characterActionAndMovement.Animator;

        if (m_useCheatEquipment)
            m_playerEquipmentData = CheatEquipment();

        if (m_animator != null)
            SetInitializingActiveEquippment();
        else Debug.Log("Missing Animator in EquipmentHandler Script");

    }

    public void SetInitializingActiveEquippment()
    {
        WeaponInstanceData activeWeapon = (int)m_playerEquipmentData.ActiveWeaponSlot == 1 ? m_playerEquipmentData.Weapon1 : m_playerEquipmentData.Weapon2;
        if (activeWeapon != null && activeWeapon.WeaponData != null)
            m_movesetData.weapon = activeWeapon.WeaponData;
        else 
            m_movesetData.weapon = m_defaultEmptyWeapon.WeaponData;

        ShieldInstanceData activeShield = (int)m_playerEquipmentData.ActiveShieldSlot == 1 ? m_playerEquipmentData.Shield1 : m_playerEquipmentData.Shield2;
        if (activeShield != null && activeShield.ShieldData != null)
            m_movesetData.shield = activeShield.ShieldData;
        else
            m_movesetData.shield = m_defaultEmptyShield.ShieldData;

        switch ((int)m_playerEquipmentData.ActiveItemSlot)
        {
            default: if (m_playerEquipmentData.Item1 != null && m_playerEquipmentData.Item1.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item1.ItemData; break;
            case 2: if (m_playerEquipmentData.Item2 != null && m_playerEquipmentData.Item2.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item2.ItemData; break;
            case 3: if (m_playerEquipmentData.Item3 != null && m_playerEquipmentData.Item3.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item3.ItemData; break;
            case 4: if (m_playerEquipmentData.Item4 != null && m_playerEquipmentData.Item4.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item4.ItemData; break;
            case 5: if (m_playerEquipmentData.Item5 != null && m_playerEquipmentData.Item5.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item5.ItemData; break;
            case 6: if (m_playerEquipmentData.Item6 != null && m_playerEquipmentData.Item6.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item6.ItemData; break;
        }

        ChangeAnimation.InitializeAnimationOverrideController(m_animator, m_movesetData);

        m_characterActionAndMovement.SetNextPossibleWeaponActions();
        m_characterActionAndMovement.SetNextPossibleShieldActions();

    }


    public void SwitchActiveWeapon()
    {
        m_playerEquipmentData.ActiveWeaponSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveWeaponSlot % 2) + 1);
        WeaponInstanceData activeWeapon = (int)m_playerEquipmentData.ActiveWeaponSlot == 1 ? m_playerEquipmentData.Weapon1 : m_playerEquipmentData.Weapon2;
        if (activeWeapon != null && activeWeapon.WeaponData != null)
            m_movesetData.weapon = activeWeapon.WeaponData;
        else 
            m_movesetData.weapon = m_defaultEmptyWeapon.WeaponData;

            ChangeAnimation.ChangeWeapon(m_animator, m_movesetData.weapon);
        m_characterActionAndMovement.SetNextPossibleWeaponActions();
    }

    public void SwitchActiveShield()
    {
        m_playerEquipmentData.ActiveShieldSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveShieldSlot % 2) + 1);
        ShieldInstanceData activeShield = (int)m_playerEquipmentData.ActiveShieldSlot == 1 ? m_playerEquipmentData.Shield1 : m_playerEquipmentData.Shield2;
        if (activeShield != null && activeShield.ShieldData != null)
            m_movesetData.shield = activeShield.ShieldData;
        else
            m_movesetData.shield = m_defaultEmptyShield.ShieldData;

        ChangeAnimation.ChangeShield(m_animator, m_movesetData.shield);
        m_characterActionAndMovement.SetNextPossibleShieldActions();
    }

    public void SwitchActiveItem()
    {
        ItemData getItemData(PlayerEquipmentData PED, int activeSlot)
        {
            switch (activeSlot)
            {
                default: return PED.Item1.ItemData; break;
                case 2: return PED.Item2.ItemData; break;
                case 3: return PED.Item3.ItemData; break;
                case 4: return PED.Item4.ItemData; break;
                case 5: return PED.Item5.ItemData; break;
                case 6: return PED.Item6.ItemData; break;
            }
        }

        for (int i = 1; i <= m_playerEquipmentData.ItemCount; i++)
        {
            m_playerEquipmentData.ActiveItemSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveItemSlot % m_playerEquipmentData.ItemCount) + 1);
            ItemData itemData = getItemData(m_playerEquipmentData, (int)m_playerEquipmentData.ActiveShieldSlot);
            if (itemData != null) { m_movesetData.item = itemData; break;}
        }
        ChangeAnimation.ChangeItem(m_animator, m_movesetData.item);
    }

    //public void SetActiveItem(ItemInstanceData item, bool overrideAnimation = true, bool rotateSlot = true)
    //{
    //    if (rotateSlot) m_playerEquipmentData.ActiveShieldSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveShieldSlot % m_playerEquipmentData.ItemCount) + 1);

    //    m_movesetData.item = item.ItemData;
    //    if (overrideAnimation)
    //        ChangeAnimation.ChangeItem(m_animator, m_movesetData.item);
    //}

    public void SetImpactCrystal()
    {

    }
    public void SetGears()
    {

    }
    public void SetArmor()
    {

    }




    public PlayerEquipmentData CheatEquipment()
    {
        PlayerEquipmentData cheatEquipment = m_playerEquipmentData != null ? m_playerEquipmentData : new PlayerEquipmentData();


        if (m_cheatEquipmentData.Weapon1.WeaponData != null) cheatEquipment.Weapon1 = m_cheatEquipmentData.Weapon1;
        if (m_cheatEquipmentData.Weapon2.WeaponData != null) cheatEquipment.Weapon2 = m_cheatEquipmentData.Weapon2;

        if (m_cheatEquipmentData.Shield1.ShieldData != null) cheatEquipment.Shield1 = m_cheatEquipmentData.Shield1;
        if (m_cheatEquipmentData.Shield2.ShieldData != null) cheatEquipment.Shield2 = m_cheatEquipmentData.Shield2;

        if (m_cheatEquipmentData.ImpactCrystal != null)      cheatEquipment.ImpactCrystal = m_cheatEquipmentData.ImpactCrystal;

        m_cheatEquipmentData.GearCount = m_cheatEquipmentData.Gears.Count;
        if (m_cheatEquipmentData.Gears.Count != 0) { cheatEquipment.Gears = m_cheatEquipmentData.Gears; cheatEquipment.GearCount = m_cheatEquipmentData.GearCount; }

        if (m_cheatEquipmentData.ArmorHead != null)          cheatEquipment.ArmorHead = m_cheatEquipmentData.ArmorHead;
        if (m_cheatEquipmentData.ArmorChest != null)         cheatEquipment.ArmorChest = m_cheatEquipmentData.ArmorChest;
        if (m_cheatEquipmentData.ArmorArms != null)          cheatEquipment.ArmorArms = m_cheatEquipmentData.ArmorArms;
        if (m_cheatEquipmentData.ArmorLegs != null)          cheatEquipment.ArmorLegs = m_cheatEquipmentData.ArmorLegs;

        if (m_cheatEquipmentData.Item1 != null) { if (cheatEquipment.Item1 == null) cheatEquipment.ItemCount++; cheatEquipment.Item1 = m_cheatEquipmentData.Item1; }
        if (m_cheatEquipmentData.Item2 != null) { if (cheatEquipment.Item2 == null) cheatEquipment.ItemCount++; cheatEquipment.Item2 = m_cheatEquipmentData.Item2; }
        if (m_cheatEquipmentData.Item3 != null) { if (cheatEquipment.Item3 == null) cheatEquipment.ItemCount++; cheatEquipment.Item3 = m_cheatEquipmentData.Item3; }
        if (m_cheatEquipmentData.Item4 != null) { if (cheatEquipment.Item4 == null) cheatEquipment.ItemCount++; cheatEquipment.Item4 = m_cheatEquipmentData.Item4; }
        if (m_cheatEquipmentData.Item5 != null) { if (cheatEquipment.Item5 == null) cheatEquipment.ItemCount++; cheatEquipment.Item5 = m_cheatEquipmentData.Item5; }
        if (m_cheatEquipmentData.Item6 != null) { if (cheatEquipment.Item6 == null) cheatEquipment.ItemCount++; cheatEquipment.Item6 = m_cheatEquipmentData.Item6; }


        return cheatEquipment;
    }

}
