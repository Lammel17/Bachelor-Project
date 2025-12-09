using EditorAttributes;
using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterActionAndMovementHandler))]
[RequireComponent(typeof(ChangeAnimation))]
[RequireComponent(typeof(CharacterStatus))]
[RequireComponent(typeof(ShieldImpactHandler))]

public class EquipmentHandler : MonoBehaviour
{
    [SerializeField][ReadOnly] public String info = "Script is only for the Player!";
    [NonSerialized] public static EquipmentHandler Instance;
    [Space]
    [SerializeField] private Transform m_weaponPosition;
    [SerializeField] private Transform m_shieldPosition;
    private GameObject m_activeWeaponGameObjReference = null;
    private GameObject m_activeShieldGameObjReference = null;
    private bool m_equipmentIsReady = true;

    private CharacterStatus m_characterStatus;
    private Animator m_animator;
    private ChangeAnimation m_changeAnimation;
    private CharacterActionAndMovementHandler m_characterActionAndMovement;
    private ShieldImpactHandler m_shieldImpactHandler;
    private CharacterMovesetData m_movesetData;


    [Space]
    [SerializeField][EditorAttributes.ReadOnly] private PlayerEquipmentData m_playerEquipmentData = new PlayerEquipmentData();
    [Space]
    [SerializeField][Required] private WeaponInstanceData m_defaultEmptyWeapon;
    [SerializeField][Required] private ShieldInstanceData m_defaultEmptyShield;
    [SerializeField][Required] private ImpactCrystalInstanceData m_defaultImpactCrystal;
    [Space]
    [SerializeField] private bool m_clearActiveEquipmentMovesetAtStart = false;
    [SerializeField] private bool m_useCheatEquipment = false;
    [SerializeField] private PlayerEquipmentData m_cheatEquipmentData = null;


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
        m_changeAnimation = GetComponent<ChangeAnimation>();
        m_shieldImpactHandler = GetComponent<ShieldImpactHandler>();
        m_characterStatus = GetComponent<CharacterStatus>();
        m_movesetData = m_characterStatus.MovesetData;

        if (m_clearActiveEquipmentMovesetAtStart)
        {
            m_movesetData.weapon = m_defaultEmptyWeapon.WeaponData;
            m_movesetData.shield = m_defaultEmptyShield.ShieldData;
            m_movesetData.item = null;
        }


        m_animator = m_characterActionAndMovement.Animator;

        if (m_useCheatEquipment)
            m_playerEquipmentData = CheatEquipment();

        //this cant be empty, always a crystal equipted
        if (m_playerEquipmentData.ImpactCrystal == null || m_playerEquipmentData.ImpactCrystal.ImactCrystalData == null) m_playerEquipmentData.ImpactCrystal = m_defaultImpactCrystal;

        SetImpactCrystal();

        if (m_animator != null)
            SetInitializingActiveEquippment();
        else Debug.Log("Missing Animator in EquipmentHandler Script");

    }




    public void SetInitializingActiveEquippment()
    {
        WeaponInstanceData activeWeapon = (int)m_playerEquipmentData.ActiveWeaponSlot == 1 ? m_playerEquipmentData.Weapon1 : m_playerEquipmentData.Weapon2;
        if (activeWeapon == null || activeWeapon.WeaponData == null)
            activeWeapon = m_defaultEmptyWeapon;
        m_movesetData.weapon = activeWeapon.WeaponData;
        m_characterStatus.ActiveWeaponInstanceData = activeWeapon;


        ShieldInstanceData activeShield = (int)m_playerEquipmentData.ActiveShieldSlot == 1 ? m_playerEquipmentData.Shield1 : m_playerEquipmentData.Shield2;
        if (activeShield == null || activeShield.ShieldData == null)
            activeShield = m_defaultEmptyShield;
        m_movesetData.shield = activeShield.ShieldData;
        m_characterStatus.ActiveShieldInstanceData = activeShield;
        m_shieldImpactHandler.SetShieldValues(activeShield.ShieldData.ImpactAbsorbtionRecoveryDelay, activeShield.ShieldData.ImpactAbsorbtionPerfBlockTimeFrame);

        switch ((int)m_playerEquipmentData.ActiveItemSlot)
        {
            default: if (m_playerEquipmentData.Item1 != null && m_playerEquipmentData.Item1.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item1.ItemData; break;
            case 2: if (m_playerEquipmentData.Item2 != null && m_playerEquipmentData.Item2.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item2.ItemData; break;
            case 3: if (m_playerEquipmentData.Item3 != null && m_playerEquipmentData.Item3.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item3.ItemData; break;
            case 4: if (m_playerEquipmentData.Item4 != null && m_playerEquipmentData.Item4.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item4.ItemData; break;
            case 5: if (m_playerEquipmentData.Item5 != null && m_playerEquipmentData.Item5.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item5.ItemData; break;
            case 6: if (m_playerEquipmentData.Item6 != null && m_playerEquipmentData.Item6.ItemData != null) m_movesetData.item = m_playerEquipmentData.Item6.ItemData; break;
        }

        m_changeAnimation.InitializeAnimationOverrideController(m_movesetData);

        m_characterActionAndMovement.SetNextPossibleWeaponActions();
        m_characterActionAndMovement.SetNextPossibleShieldActions();

        //INSTANTIATE WEAPON AND SHIELD OBJECT
        if (!m_equipmentIsReady)
            return;

        if (m_weaponPosition != null && m_movesetData.weapon.WeaponModel != null)
        {
            m_activeWeaponGameObjReference = Instantiate(m_movesetData.weapon.WeaponModel, m_weaponPosition);
            if (m_activeWeaponGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
            {
                m_characterStatus.HitBoxManagerWeapon = hitManager;
                m_characterStatus.HitBoxManagerWeapon.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
            }

        }

        if (m_shieldPosition != null && m_movesetData.shield.ShieldModel != null)
        {
            m_activeShieldGameObjReference = Instantiate(m_movesetData.shield.ShieldModel, m_shieldPosition);
            if (m_activeShieldGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
            {
                m_characterStatus.HitBoxManagerShield = hitManager;
                m_characterStatus.HitBoxManagerShield.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
            }
        }
        

    }


    public void ReadyOrRemoveEquipment(bool isReadyEquipment)
    {
        m_equipmentIsReady = isReadyEquipment;

        if (!m_equipmentIsReady)
        {
            m_characterStatus.HitBoxManagerWeapon = null;
            m_characterStatus.HitBoxManagerShield= null;

            if (m_activeWeaponGameObjReference != null)
                Destroy(m_activeWeaponGameObjReference);
            if (m_activeShieldGameObjReference != null)
                Destroy(m_activeShieldGameObjReference);
        }
        else
        {
            if (m_weaponPosition != null && m_movesetData.weapon.WeaponModel != null)
            {
                m_activeWeaponGameObjReference = Instantiate(m_movesetData.weapon.WeaponModel, m_weaponPosition);
                if (m_activeWeaponGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
                {
                    m_characterStatus.HitBoxManagerWeapon = hitManager;
                    m_characterStatus.HitBoxManagerWeapon.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
                }
            }
            if (m_shieldPosition != null && m_movesetData.shield.ShieldModel != null)
            {
                m_activeShieldGameObjReference = Instantiate(m_movesetData.shield.ShieldModel, m_shieldPosition);
                if (m_activeShieldGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
                {
                    m_characterStatus.HitBoxManagerShield = hitManager;
                    m_characterStatus.HitBoxManagerShield.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
                }
            }
        }
    }


    public void SwitchActiveWeapon()
    {
        m_playerEquipmentData.ActiveWeaponSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveWeaponSlot % 2) + 1);
        WeaponInstanceData activeWeapon = (int)m_playerEquipmentData.ActiveWeaponSlot == 1 ? m_playerEquipmentData.Weapon1 : m_playerEquipmentData.Weapon2;
        if (activeWeapon == null || activeWeapon.WeaponData == null)
            activeWeapon = m_defaultEmptyWeapon;
        m_movesetData.weapon = activeWeapon.WeaponData;
        m_characterStatus.ActiveWeaponInstanceData = activeWeapon;

        m_changeAnimation.ChangeWeapon(m_movesetData.weapon);
        m_characterActionAndMovement.SetNextPossibleWeaponActions();

        //INSTANTIATE NEW WEAPON OBJECT
        if (!m_equipmentIsReady)
            return;

        if (m_activeWeaponGameObjReference != null)
            Destroy(m_activeWeaponGameObjReference);

        if (m_weaponPosition != null && m_movesetData.weapon.WeaponModel != null)
        {
            m_activeWeaponGameObjReference = Instantiate(m_movesetData.weapon.WeaponModel, m_weaponPosition);
            if (m_activeWeaponGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
            {
                m_characterStatus.HitBoxManagerWeapon = hitManager;
                m_characterStatus.HitBoxManagerWeapon.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
            }
        }

    }

    public void SwitchActiveShield()
    {
        m_playerEquipmentData.ActiveShieldSlot = (PlayerEquipmentData.Slot)(((int)m_playerEquipmentData.ActiveShieldSlot % 2) + 1);
        ShieldInstanceData activeShield = (int)m_playerEquipmentData.ActiveShieldSlot == 1 ? m_playerEquipmentData.Shield1 : m_playerEquipmentData.Shield2;
        if (activeShield == null || activeShield.ShieldData == null)
            activeShield = m_defaultEmptyShield;
        m_movesetData.shield = activeShield.ShieldData;
        m_characterStatus.ActiveShieldInstanceData = activeShield;
        m_shieldImpactHandler.SetShieldValues(activeShield.ShieldData.ImpactAbsorbtionRecoveryDelay, activeShield.ShieldData.ImpactAbsorbtionPerfBlockTimeFrame);

        m_changeAnimation.ChangeShield(m_movesetData.shield);
        m_characterActionAndMovement.SetNextPossibleShieldActions();

        //INSTANTIATE NEW SHIELD OBJECT
        if (!m_equipmentIsReady)
            return;

        if (m_activeShieldGameObjReference != null)
            Destroy(m_activeShieldGameObjReference);

        if (m_shieldPosition != null && m_movesetData.shield.ShieldModel != null)
        {
            m_activeShieldGameObjReference = Instantiate(m_movesetData.shield.ShieldModel, m_shieldPosition);
            if (m_activeShieldGameObjReference.TryGetComponent<HitAndHurtBoxManagerOfEquipment>(out HitAndHurtBoxManagerOfEquipment hitManager))
            {
                m_characterStatus.HitBoxManagerShield = hitManager;
                m_characterStatus.HitBoxManagerShield.ReadyHitBoxManager(m_characterStatus.HurtBoxManager);
            }
        }
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
        m_changeAnimation.ChangeItem(m_movesetData.item);
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
        ImpactCrystalData crystal = m_playerEquipmentData.ImpactCrystal.ImactCrystalData;
        m_shieldImpactHandler.SetCrystalValues(crystal.MaxEnergyPointsGain, crystal.AbsorbtionCurveSpeed, crystal.AbsorbtionCurveDuration);
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
