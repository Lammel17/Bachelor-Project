using EditorAttributes;
using System;
using UnityEngine;

[RequireComponent(typeof(CharacterActionAndMovementHandler))]
[RequireComponent(typeof(HurtBoxManager))]
public class CharacterStatus : MonoBehaviour
{
    [Header("ImportantData Stuff")]
    [SerializeField] private CharacterStatsData m_characterStatsData;
    [SerializeField] private CharacterMovesetData m_movesetData;
    [SerializeField][ReadOnly] private CharacterActionAndMovementHandler m_playerMovement;
    [SerializeField][ReadOnly] private HurtBoxManager m_hurtBoxManager;
    [Space]
    [Header("Must set if its not the Player")]
    [SerializeField] private HitBoxManager m_activeWeaponHitBoxManager = null;
    [SerializeField] private WeaponInstanceData m_activeWeaponInstance;
    [SerializeField] private HitBoxManager m_activeShieldHitBoxManager = null;
    [SerializeField] private ShieldInstanceData m_activeShieldInstance;
    [Space]
    [Space]
    [Space]
    [Header("Character Stats")]
    [SerializeField] private bool m_infinteStamina = false;
    [Space]
    [SerializeField] private float m_energyRecoverySpeed = 1f;
    [SerializeField] private float m_energyRecoveryPause = 1f;
    [Space]
    [SerializeField] private float m_specialEnergyRecoverySpeed = 1f;
    [Space]
    [SerializeField] private float m_PoiseRecoverySpeed = 1f;
    [SerializeField] private float m_buildUpsRecoverySpeed = 1f;

    [Space]
    [SerializeField] private float m_minRecoveredEnergyForAction = 40f;
    [SerializeField] private float m_minRecoveredEnergyConstantForAction = 90f;

    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_hp = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_ep = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_sep = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_poise = 1;
    [Space]
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_thermic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_electric = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_metaphysic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_contamination = 0;


    private bool m_isPauseEnergyRecoveryDueAction = false;
    private bool m_isPauseEnergyRecoveryDueEmpty = false;
    private bool m_isEnergyExhausted = false;
    private bool m_thisFrameEnergyWasConsumed = false;

    private float m_healthLostOfFrame = 0;
    private float m_healthGainOfFrame = 0;
    private float m_energyCostsOfFrame = 0;
    private float m_energyGainOfFrame = 0;
    private float m_specialEnergyGainOfFrame = 0;
    private float m_gainingHealthForTimeFactor = 0;
    private float m_loosingHealthForTimeFactor = 0;

    private Coroutine m_pauseEnergyRecoveryCoroutine;
    public enum AilmentType
    {
        none = 0,
        Thermic,
        Electric,
        Metaphysic,
        Contamination
    }

    public CharacterMovesetData MovesetData { get => m_movesetData; }
    public HurtBoxManager HurtBoxManager { get => m_hurtBoxManager; }
    public HitBoxManager HitBoxManagerWeapon { get => m_activeWeaponHitBoxManager; set => m_activeWeaponHitBoxManager = value; }
    public HitBoxManager HitBoxManagerShield { get => m_activeShieldHitBoxManager; set => m_activeShieldHitBoxManager = value; }
    public WeaponInstanceData ActiveWeaponInstanceData { get => m_activeWeaponInstance; set => m_activeWeaponInstance = value; }
    public ShieldInstanceData ActiveShieldInstanceData { get => m_activeShieldInstance; set => m_activeShieldInstance = value; }



    private void Start()
    {
        if (m_characterStatsData == null)
        {
            m_characterStatsData = new CharacterStatsData();
            m_infinteStamina = true;
        }

        m_playerMovement = GetComponent<CharacterActionAndMovementHandler>();
        m_hurtBoxManager = GetComponent<HurtBoxManager>();

        m_characterStatsData.HealthPoints.x = m_characterStatsData.HealthPoints.y;
        m_characterStatsData.EnergyPoints.x = m_characterStatsData.EnergyPoints.y;
        m_characterStatsData.SpecialEnergyPoints.x = m_characterStatsData.SpecialEnergyPoints.y;
        m_characterStatsData.PoisePoints.x = m_characterStatsData.PoisePoints.y;
        m_characterStatsData.ThermicBuildUp.x = 0;
        m_characterStatsData.ElectricBuildUp.x = 0;
        m_characterStatsData.MetaphysicBuildUp.x = 0;
        m_characterStatsData.ContaminationBuildUp.x = 0;


        //m_activeWeaponInstance.DamageData = new DamageData( ); Here i stopped
    }


    private void Update()
    {
        SetHealthEveryFrame();
        SetEnergyEveryFrame();
        SetSpecialEnergyEveryFrame();

        RecoverPoise();
        RecoverBuildUps();

        m_hp = (float)m_characterStatsData.HealthPoints.x / (float)m_characterStatsData.HealthPoints.y;
        m_ep = (float)m_characterStatsData.EnergyPoints.x / (float)m_characterStatsData.EnergyPoints.y;
        m_sep = (float)m_characterStatsData.SpecialEnergyPoints.x / (float)m_characterStatsData.SpecialEnergyPoints.y;
        m_poise = m_characterStatsData.PoisePoints.x / m_characterStatsData.PoisePoints.y;

        m_thermic = m_characterStatsData.ThermicBuildUp.x >= 0 ? m_characterStatsData.ThermicBuildUp.x / m_characterStatsData.ThermicBuildUp.y : m_characterStatsData.ThermicBuildUp.x / m_characterStatsData.ThermicBuildUp.z;
        m_electric = m_characterStatsData.ElectricBuildUp.x >= 0 ? m_characterStatsData.ElectricBuildUp.x / m_characterStatsData.ElectricBuildUp.y : m_characterStatsData.ElectricBuildUp.x / m_characterStatsData.ElectricBuildUp.z;
        m_metaphysic = m_characterStatsData.MetaphysicBuildUp.x >= 0 ? m_characterStatsData.MetaphysicBuildUp.y / m_characterStatsData.MetaphysicBuildUp.y : m_characterStatsData.MetaphysicBuildUp.y / m_characterStatsData.MetaphysicBuildUp.z;
        m_contamination = m_characterStatsData.ContaminationBuildUp.x / m_characterStatsData.ContaminationBuildUp.y;

    }
    public void StopAll()
    {
        m_gainingHealthForTimeFactor = 0;
        m_loosingHealthForTimeFactor = 0;
        StopAllCoroutines();
        m_pauseEnergyRecoveryCoroutine = null;
    }

    public void TakeDamageByDamageData(DamageData damageData)
    {
        int damage = damageData.PhysicalSliceDamage * (1 - (m_characterStatsData.PhysicalSliceNegation / 100))
                      + damageData.PhysicalBluntDamage * (1 - (m_characterStatsData.PhysicalBluntNegation / 100))
                      + damageData.PhysicalPierceDamage * (1 - (m_characterStatsData.PhysicalPierceNegation / 100))
                      + damageData.ThermicDamageAndBuildUp.x * (1 - (m_characterStatsData.ThermicNegation / 100))
                      + damageData.ElectricDamageAndBuildUp.x * (1 - (m_characterStatsData.ElectricNegation / 100))
                      + damageData.MetaphysicDamageAndBuildUp.x * (1 - (m_characterStatsData.MetaphysicNegation / 100));
        LooseFixedHealthPoints(damage);

        TakeAilmentBuildUpDamage(AilmentType.Thermic, damageData.ThermicDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Electric, damageData.ElectricDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Metaphysic, damageData.MetaphysicDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Contamination, damageData.ContaminationBuildUpDamage);

        TakePoiseDamage(damageData.PoiseDamage);
    }

    public DamageData GetActionDamageData(WeaponData.WeaponAttack attackData, Vector3 playerDirection, WeaponData.PhysicalDamageType Physicaltype)
    {
        WeaponData.PhysicalDamageType physicalType = (attackData.PhysicalType != WeaponData.PhysicalDamageType.TypeByBase) ? attackData.PhysicalType : Physicaltype;
        DamageTableData tableData = m_activeWeaponInstance.WeaponData.DamageTabel;
        float levelFactor = tableData.UpgradeCurve.Evaluate(m_activeWeaponInstance.WeaponLevelCurrentMax.x / m_activeWeaponInstance.WeaponLevelCurrentMax.y);
        DamageData baseDmgDat = CalculateBaseDamageData(tableData, levelFactor);
        return CalculateActionDamageData(baseDmgDat, attackData.actionDamageData, playerDirection, physicalType);
    }
    public DamageData GetActionDamageData(ShieldData.ShieldAction actionkData, Vector3 playerDirection, WeaponData.PhysicalDamageType Physicaltype)
    {
        WeaponData.PhysicalDamageType physicalType = (actionkData.PhysicalType != WeaponData.PhysicalDamageType.TypeByBase) ? actionkData.PhysicalType : Physicaltype;
        DamageTableData tableData = m_activeShieldInstance.ShieldData.DamageTabel;
        float levelFactor = tableData.UpgradeCurve.Evaluate(m_activeShieldInstance.ShieldLevelCurrentMax.x / m_activeShieldInstance.ShieldLevelCurrentMax.y);
        DamageData baseDmgDat = CalculateBaseDamageData(tableData, levelFactor);
        return CalculateActionDamageData(baseDmgDat, actionkData.actionDamageData, playerDirection, physicalType);
    }
    public DamageData CalculateActionDamageData(DamageData baseDmgDat, DamageMultiplikatorData actionDamageMultiplikator, Vector3 playerDirection, WeaponData.PhysicalDamageType physicaltype)
    {
        DamageData dmgDat = new DamageData(
            physicaltype != WeaponData.PhysicalDamageType.Slice ? 0 : (int)(baseDmgDat.PhysicalSliceDamage * actionDamageMultiplikator.PhysicalFactor),
            physicaltype != WeaponData.PhysicalDamageType.Blunt ? 0 : (int)(baseDmgDat.PhysicalBluntDamage * actionDamageMultiplikator.PhysicalFactor),
            physicaltype != WeaponData.PhysicalDamageType.Pierce ? 0 : (int)(baseDmgDat.PhysicalPierceDamage * actionDamageMultiplikator.PhysicalFactor),
            new Vector2Int((int)(baseDmgDat.ThermicDamageAndBuildUp.x * actionDamageMultiplikator.ThermicFactor), baseDmgDat.ThermicDamageAndBuildUp.y),
            new Vector2Int((int)(baseDmgDat.ElectricDamageAndBuildUp.x * actionDamageMultiplikator.ElectricFactor), baseDmgDat.ElectricDamageAndBuildUp.y),
            new Vector2Int((int)(baseDmgDat.MetaphysicDamageAndBuildUp.x * actionDamageMultiplikator.MetaphysicFactor), baseDmgDat.MetaphysicDamageAndBuildUp.y),
            (int)(baseDmgDat.ContaminationBuildUpDamage * actionDamageMultiplikator.AilmentsFactor),
            (int)(baseDmgDat.PoiseDamage * actionDamageMultiplikator.PoiseDamageFactor),
            Quaternion.LookRotation(playerDirection) * baseDmgDat.Direction);

        return dmgDat;
    }
    public DamageData CalculateBaseDamageData(DamageTableData tableData, float levelFactor)
    {
        DamageData dmgDat = new DamageData(
            (int)(Mathf.Lerp(tableData.PhysicalSlice.x, tableData.PhysicalSlice.y, levelFactor)),
            (int)(Mathf.Lerp(tableData.PhysicalBlunt.x, tableData.PhysicalBlunt.y, levelFactor)),
            (int)(Mathf.Lerp(tableData.PhysicalPierce.x, tableData.PhysicalPierce.y, levelFactor)),
            new Vector2Int((int)(Mathf.Lerp(tableData.Thermic.x, tableData.Thermic.y, levelFactor)), (int)(Mathf.Lerp(tableData.Thermic.x, tableData.Thermic.y, levelFactor))), ////////////////////////hmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
            new Vector2Int((int)(Mathf.Lerp(tableData.Electric.x, tableData.Electric.y, levelFactor)), (int)(Mathf.Lerp(tableData.Electric.x, tableData.Electric.y, levelFactor))),
            new Vector2Int((int)(Mathf.Lerp(tableData.Metaphysic.x, tableData.Metaphysic.y, levelFactor)), (int)(Mathf.Lerp(tableData.Metaphysic.x, tableData.Metaphysic.y, levelFactor))),
            (int)(Mathf.Lerp(tableData.ContaminationBuildUp.x, tableData.ContaminationBuildUp.y, levelFactor)),
            (int)(Mathf.Lerp(tableData.Poise.x, tableData.Poise.y, levelFactor)),
            Vector3.forward);

        return dmgDat;
    }




    #region HEALTH POINTS
    public void SetHealthEveryFrame()
    {
        if (m_gainingHealthForTimeFactor != 0)
        {
            m_healthGainOfFrame += m_gainingHealthForTimeFactor * Time.deltaTime;
            if ((int)m_healthGainOfFrame > 0)
            {
                GainFixedHealthPoints((int)m_healthGainOfFrame);
                m_healthGainOfFrame -= (int)m_healthGainOfFrame;
            }
        }
        if (m_loosingHealthForTimeFactor != 0)
        {
            m_healthLostOfFrame += m_loosingHealthForTimeFactor * Time.deltaTime;
            if ((int)m_healthLostOfFrame > 0)
            {
                LooseFixedHealthPoints((int)m_healthLostOfFrame);
                m_healthLostOfFrame -= (int)m_healthLostOfFrame;
            }
        }

    }

    public void LooseFixedHealthPoints(int loosePoints)
    {
        m_characterStatsData.HealthPoints.x = Mathf.Max(m_characterStatsData.HealthPoints.x - loosePoints, 0);

    }

    public void GainFixedHealthPoints(int gainPoints)
    {
        m_characterStatsData.HealthPoints.x = Mathf.Min(m_characterStatsData.HealthPoints.x + gainPoints, m_characterStatsData.HealthPoints.y);

    }


    public void GainHealthForTime(float time, float healthByTime) //this is stackable
    {
        m_gainingHealthForTimeFactor += healthByTime;
        Action stopHealthRecovery = () => { m_gainingHealthForTimeFactor -= healthByTime; };
        StartCoroutine(UtilityFunctions.Wait(time, stopHealthRecovery));
    }

    public void LooseHealthForTime(float time, float healthByTime) //this is stackable
    {
        m_loosingHealthForTimeFactor += healthByTime;
        Action stopHealthLost = () => { m_loosingHealthForTimeFactor -= healthByTime; };
        StartCoroutine(UtilityFunctions.Wait(time, stopHealthLost));
    }

    #endregion

    #region ENERGY POINTS
    private void SetEnergyEveryFrame()
    {
        if (m_thisFrameEnergyWasConsumed)
        {
            m_thisFrameEnergyWasConsumed = false;
            m_characterStatsData.EnergyPoints.x = Mathf.Max(m_characterStatsData.EnergyPoints.x - (int)m_energyCostsOfFrame, 0);
            m_energyCostsOfFrame -= (int)m_energyCostsOfFrame;
        }
        else if (!m_isPauseEnergyRecoveryDueAction && !m_isPauseEnergyRecoveryDueEmpty)
        {
            m_energyGainOfFrame += m_energyRecoverySpeed * Time.deltaTime;
            m_characterStatsData.EnergyPoints.x = Mathf.Min(m_characterStatsData.EnergyPoints.x + (int)m_energyGainOfFrame, m_characterStatsData.EnergyPoints.y);
            m_energyGainOfFrame -= (int)m_energyGainOfFrame;
        }


        if (m_characterStatsData.EnergyPoints.x == 0 && !m_isEnergyExhausted)
        {
            m_isEnergyExhausted = true;
            m_isPauseEnergyRecoveryDueEmpty = true;

            //if currently is action, the coroutine will be called at the end of the action anyways
            if (!m_isPauseEnergyRecoveryDueAction)
                ContinueEnergyRegenerationInTime();
        }
    }

    public void ExpendEnergyPoints(float expenses)
    {
        m_thisFrameEnergyWasConsumed = true;
        m_energyCostsOfFrame += expenses;
    }



    public void PauseEnergyRegenerationByAction()
    {
        if (m_pauseEnergyRecoveryCoroutine != null) { StopCoroutine(m_pauseEnergyRecoveryCoroutine); m_pauseEnergyRecoveryCoroutine = null; }
        m_isPauseEnergyRecoveryDueAction = true;
    }
    public void ContinueEnergyRegenerationInTime(float pauseTime = 0.25f)
    {
        if (m_isPauseEnergyRecoveryDueEmpty)
            pauseTime = 1;

        if (m_pauseEnergyRecoveryCoroutine != null) { StopCoroutine(m_pauseEnergyRecoveryCoroutine); m_pauseEnergyRecoveryCoroutine = null; }
        Action stopPauseEnergyRecoveryAction = () => 
        {
            m_isPauseEnergyRecoveryDueEmpty = false; 
            m_isPauseEnergyRecoveryDueAction = false;
            m_pauseEnergyRecoveryCoroutine = null; 
        };
        m_pauseEnergyRecoveryCoroutine = StartCoroutine(UtilityFunctions.Wait(pauseTime, stopPauseEnergyRecoveryAction));

    }


    public bool CheckIfCanExpendEnergy()
    {
        if (m_infinteStamina) return true;
        if (!m_isEnergyExhausted) return true;
        else if (m_isEnergyExhausted && m_characterStatsData.EnergyPoints.x >= m_minRecoveredEnergyForAction)
        {
            m_isEnergyExhausted = false;
            return true;
        }
        else
            return false;
    }
    public bool CheckIfCanConsumeConstantEnergy()
    {
        if (m_infinteStamina) return true;
        if (!m_isEnergyExhausted) return true;
        else if (m_isEnergyExhausted && m_characterStatsData.EnergyPoints.x >= m_minRecoveredEnergyConstantForAction)
        {
            m_isEnergyExhausted = false;
            return true;
        }
        else
            return false;
    }

    #endregion

    #region SPECIAL ENERGY

    private void SetSpecialEnergyEveryFrame()
    {
        if (m_characterStatsData.EnergyPoints.x == m_characterStatsData.EnergyPoints.y)
        {
            m_specialEnergyGainOfFrame += m_specialEnergyRecoverySpeed * Time.deltaTime;
            m_characterStatsData.SpecialEnergyPoints.x = Mathf.Min(m_characterStatsData.SpecialEnergyPoints.x + (int)m_specialEnergyGainOfFrame, m_characterStatsData.SpecialEnergyPoints.y);
            m_specialEnergyGainOfFrame -= (int)m_specialEnergyGainOfFrame;
        }
    }

    public bool CheckIfCanExpendSpecialEnergy(int cost)
    {
        if (m_characterStatsData.SpecialEnergyPoints.x >= cost)
            return true;
        else
            return false;
    }

    public void ExpendSpecialEnergyPoints(float expenses)
    {
        m_characterStatsData.SpecialEnergyPoints.x = Mathf.Max(m_characterStatsData.SpecialEnergyPoints.x - (int)expenses, 0);
    }

    #endregion

    #region POISE
    private void RecoverPoise()
    {
        if (m_characterStatsData.PoisePoints.x != m_characterStatsData.PoisePoints.y)
            m_characterStatsData.PoisePoints.x = Mathf.Min(m_characterStatsData.PoisePoints.x + m_PoiseRecoverySpeed * Time.deltaTime, m_characterStatsData.PoisePoints.y);
    }

    public void TakePoiseDamage(int poiseDamage)
    {
        if (poiseDamage == 0)
            return;

        m_characterStatsData.PoisePoints.x = Mathf.Max(m_characterStatsData.PoisePoints.x - poiseDamage, 0);

        m_playerMovement.TriggerDamage();
    }

    #endregion
    
    #region BUILDUP
    private void RecoverBuildUps()
    {
        if (m_characterStatsData.ThermicBuildUp.x != 0)
            m_characterStatsData.ThermicBuildUp.x = m_characterStatsData.ThermicBuildUp.x > 0 ? Mathf.Max(m_characterStatsData.ThermicBuildUp.x - m_buildUpsRecoverySpeed * Time.deltaTime, 0)
                                                                                              : Mathf.Min(m_characterStatsData.ThermicBuildUp.x + m_buildUpsRecoverySpeed * Time.deltaTime, 0);
        if (m_characterStatsData.ElectricBuildUp.x != 0)
            m_characterStatsData.ElectricBuildUp.x = m_characterStatsData.ElectricBuildUp.x > 0 ? Mathf.Max(m_characterStatsData.ElectricBuildUp.x - m_buildUpsRecoverySpeed * Time.deltaTime, 0)
                                                                                                : Mathf.Min(m_characterStatsData.ElectricBuildUp.x + m_buildUpsRecoverySpeed * Time.deltaTime, 0);
        if (m_characterStatsData.MetaphysicBuildUp.x != 0)
            m_characterStatsData.MetaphysicBuildUp.x = m_characterStatsData.MetaphysicBuildUp.x > 0 ? Mathf.Max(m_characterStatsData.MetaphysicBuildUp.x - m_buildUpsRecoverySpeed * Time.deltaTime, 0)
                                                                                                    : Mathf.Min(m_characterStatsData.MetaphysicBuildUp.x + m_buildUpsRecoverySpeed * Time.deltaTime, 0);
        if (m_characterStatsData.ContaminationBuildUp.x != 0) 
            m_characterStatsData.ThermicBuildUp.x = Mathf.Max(m_characterStatsData.ThermicBuildUp.x - m_buildUpsRecoverySpeed * Time.deltaTime, 0);
    }

    public void TakeAilmentBuildUpDamage(AilmentType type, float amount)
    {
        if (amount == 0) 
            return;

        switch (type)
        {
            case AilmentType.Thermic:
                m_characterStatsData.ThermicBuildUp.x = amount >= 0     ? Mathf.Min(m_characterStatsData.ThermicBuildUp.x + amount, m_characterStatsData.ThermicBuildUp.y)
                                                                        : Mathf.Max(m_characterStatsData.ThermicBuildUp.x + amount, m_characterStatsData.ThermicBuildUp.z);
                break;
            case AilmentType.Electric:
                m_characterStatsData.ElectricBuildUp.x = amount >= 0    ? Mathf.Min(m_characterStatsData.ElectricBuildUp.x + amount, m_characterStatsData.ElectricBuildUp.y)
                                                                        : Mathf.Max(m_characterStatsData.ElectricBuildUp.x + amount, m_characterStatsData.ElectricBuildUp.z);
                break;
            case AilmentType.Metaphysic:
                m_characterStatsData.MetaphysicBuildUp.x = amount >= 0  ? Mathf.Min(m_characterStatsData.MetaphysicBuildUp.x + amount, m_characterStatsData.MetaphysicBuildUp.y)
                                                                        : Mathf.Max(m_characterStatsData.MetaphysicBuildUp.x + amount, m_characterStatsData.MetaphysicBuildUp.z);
                break;
            case AilmentType.Contamination:
                m_characterStatsData.ContaminationBuildUp.x =             Mathf.Min(m_characterStatsData.ContaminationBuildUp.x + amount, m_characterStatsData.ContaminationBuildUp.y);
                break;
            default:
                break;
        }
    }
    #endregion




}
