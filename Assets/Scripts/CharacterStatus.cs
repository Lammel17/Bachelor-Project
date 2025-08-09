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
    [Header("Set if its not the Player")]
    [SerializeField] private HitAndHurtBoxManagerOfEquipment m_activeWeaponHitBoxManager = null;
    [SerializeField] private WeaponInstanceData m_activeWeaponInstance;
    [SerializeField] private HitAndHurtBoxManagerOfEquipment m_activeShieldHitBoxManager = null;
    [SerializeField] private ShieldInstanceData m_activeShieldInstance;
    [Header("Player only")]
    [SerializeField] private ShieldImpactHandler m_shieldImpactHandler;
    [Space]
    [Space]
    [Space]
    [Header("Character Stats")]
    [SerializeField] private bool m_invulnerable = false;
    [SerializeField] private bool m_infinteStamina = false;
    [Space]
    [SerializeField] private float m_energyRecoverySpeed = 1f;
    [SerializeField] private float m_energyRecoveryPause = 1f;
    [Space]
    [SerializeField] private float m_specialEnergyRecoverySpeed = 1f;
    [Space]
    [SerializeField] private float m_PoiseRecoverySpeed = 1f;
    [SerializeField] private float m_buildUpsRecoverySpeed = 1f;
    [SerializeField] private float m_poiseRecoverPauseTime = 3f;

    [Space]
    [SerializeField] private float m_minRecoveredEnergyForAction = 40f;
    [SerializeField] private float m_minRecoveredEnergyConstantForAction = 90f;

    [SerializeField][EditorAttributes.ReadOnly] private bool m_isShielding = false;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_hp = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_ep = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_sep = 1;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_poise = 1;
    [Space]
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_thermic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_electric = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_metaphysic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_contamination = 0;

    [Space]
    [Tooltip("When below x% poise, AND when recieving poiseDamage which is more than y% of maxPoise, then get stun")]
    [SerializeField] private Vector2 m_stunThreshhold = new Vector2(0.4f, 0.05f);
    [Tooltip("When below x% energy, AND when recieving energyDamage which is more than y% of maxEnergy, then get stun")]
    [SerializeField] private Vector2 m_shieldStunThreshhold = new Vector2(0.4f, 0.1f);


    private bool m_isPauseEnergyRecoveryDueAction = false;
    private bool m_isPauseEnergyRecoveryDueEmpty = false;
    private bool m_isEnergyExhausted = false;
    private bool m_thisFrameEnergyWasConsumed = false;
    private bool m_canRecoverPoise = true;

    private float m_healthLostOfFrame = 0;
    private float m_healthGainOfFrame = 0;
    private float m_energyCostsOfFrame = 0;
    private float m_energyGainOfFrame = 0;
    private float m_specialEnergyGainOfFrame = 0;
    private float m_gainingHealthForTimeFactor = 0;
    private float m_loosingHealthForTimeFactor = 0;
    private float m_energyRecoverFactorByShielding = 1;

    private Coroutine m_pauseEnergyRecoveryCoroutine;
    private Coroutine m_pausePoiseRecoveryCoroutine;
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
    public HitAndHurtBoxManagerOfEquipment HitBoxManagerWeapon { get => m_activeWeaponHitBoxManager; set => m_activeWeaponHitBoxManager = value; }
    public HitAndHurtBoxManagerOfEquipment HitBoxManagerShield { get => m_activeShieldHitBoxManager; set => m_activeShieldHitBoxManager = value; }
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
        m_metaphysic = m_characterStatsData.MetaphysicBuildUp.x >= 0 ? m_characterStatsData.MetaphysicBuildUp.x / m_characterStatsData.MetaphysicBuildUp.y : m_characterStatsData.MetaphysicBuildUp.x / m_characterStatsData.MetaphysicBuildUp.z;
        m_contamination = m_characterStatsData.ContaminationBuildUp.x / m_characterStatsData.ContaminationBuildUp.y;

    }
    public void StopAll()
    {
        m_gainingHealthForTimeFactor = 0;
        m_loosingHealthForTimeFactor = 0;
        StopAllCoroutines();
        m_pauseEnergyRecoveryCoroutine = null;
    }


    public void IsShielding(bool isShielding)
    {
        m_isShielding = isShielding;

        m_energyRecoverFactorByShielding = !isShielding ? 1 : 0;
        if (isShielding )
        {
            if (m_shieldImpactHandler != null)          m_shieldImpactHandler.UseShielding();
            if (m_activeShieldHitBoxManager != null)    m_activeShieldHitBoxManager.ActivateBlockBox();
        }
        else
        {
            if (m_shieldImpactHandler != null)          m_shieldImpactHandler.StopUseShielding();
            if (m_activeShieldHitBoxManager != null)    m_activeShieldHitBoxManager.DeactivateBlockBox();
        }
    }

    public void TakeDamageByDamageData(DamageData damageData)
    {
        //damageData when shielding, Check for Angle
        if (m_playerMovement.IsShielding && Vector3.Angle(transform.forward, -damageData.Direction) <= m_activeShieldInstance.ShieldData.ShieldingAngle)
        {
            int energyValue = -(damageData.PhysicalSliceDamage + damageData.PhysicalBluntDamage + damageData.PhysicalPierceDamage);
            energyValue += m_shieldImpactHandler.EvaluateImpactAbsorpstion(energyValue);
            damageData = CombatUtils.CalculateNegatedDamageData(m_activeShieldInstance.ShieldData.DamageNegationTable, damageData);
            
            //apply energy
            if (energyValue > 0)
                GainEnergyPoints(energyValue);
            else 
            {
                if (!m_infinteStamina)ExpendEnergyPoints(energyValue);
                DecideStaggerShielding(energyValue, damageData.StaggerType, damageData.Direction);
            }
        }

        //calculate Damage
        int damage =    damageData.PhysicalSliceDamage * (1 - (m_characterStatsData.PhysicalSliceNegation / 100))
                      + damageData.PhysicalBluntDamage * (1 - (m_characterStatsData.PhysicalBluntNegation / 100))
                      + damageData.PhysicalPierceDamage * (1 - (m_characterStatsData.PhysicalPierceNegation / 100))
                      + damageData.ThermicDamageAndBuildUp.x * (1 - (m_characterStatsData.ThermicNegation / 100))
                      + damageData.ElectricDamageAndBuildUp.x * (1 - (m_characterStatsData.ElectricNegation / 100))
                      + damageData.MetaphysicDamageAndBuildUp.x * (1 - (m_characterStatsData.MetaphysicNegation / 100));
        LooseFixedHealthPoints(damage);

        //add buildups
        TakeAilmentBuildUpDamage(AilmentType.Thermic, damageData.ThermicDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Electric, damageData.ElectricDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Metaphysic, damageData.MetaphysicDamageAndBuildUp.y);
        TakeAilmentBuildUpDamage(AilmentType.Contamination, damageData.ContaminationBuildUpDamage);

        //calculate poiseDamage
        if (!m_playerMovement.IsShielding && m_characterStatsData.HealthPoints.x != 0) TakePoiseDamage(damageData.PoiseDamage, damageData.StaggerType, damageData.Direction);
    }


    #region PrepareDamageDataForAction
    public DamageData GetActionDamageData(WeaponData.WeaponAttack attackData, Vector3 playerDirection, CombatUtils.PhysicalDamageType Physicaltype)
    {
        CombatUtils.PhysicalDamageType physicalType = (attackData.PhysicalType != CombatUtils.PhysicalDamageType.TypeByBase) ? attackData.PhysicalType : Physicaltype;
        DamageTableData tableData = m_activeWeaponInstance.WeaponData.DamageTabel;
        float levelFactor = tableData.UpgradeCurve.Evaluate(m_activeWeaponInstance.WeaponLevelCurrentMax.x / m_activeWeaponInstance.WeaponLevelCurrentMax.y);
        DamageData baseDmgDat = CombatUtils.CalculateBaseDamageData(tableData, levelFactor, attackData.StaggerType);
        return CombatUtils.CalculateActionDamageData(baseDmgDat, attackData.actionDamageData, playerDirection, physicalType);
    }
    public DamageData GetActionDamageData(ShieldData.ShieldAction actionkData, Vector3 playerDirection, CombatUtils.PhysicalDamageType Physicaltype)
    {
        CombatUtils.PhysicalDamageType physicalType = (actionkData.PhysicalType != CombatUtils.PhysicalDamageType.TypeByBase) ? actionkData.PhysicalType : Physicaltype;
        DamageTableData tableData = m_activeShieldInstance.ShieldData.DamageTabel;
        float levelFactor = tableData.UpgradeCurve.Evaluate(m_activeShieldInstance.ShieldLevelCurrentMax.x / m_activeShieldInstance.ShieldLevelCurrentMax.y);
        DamageData baseDmgDat = CombatUtils.CalculateBaseDamageData(tableData, levelFactor, actionkData.StaggerType);
        return CombatUtils.CalculateActionDamageData(baseDmgDat, actionkData.actionDamageData, playerDirection, physicalType);
    }

    #endregion



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
        if (m_invulnerable) return;

        m_characterStatsData.HealthPoints.x = Mathf.Max(m_characterStatsData.HealthPoints.x - loosePoints, 0);

        if (m_characterStatsData.HealthPoints.x == 0)
            m_playerMovement.TriggerDie();
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


        if (!m_invulnerable) 
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
            m_characterStatsData.EnergyPoints.x = Mathf.Max(m_characterStatsData.EnergyPoints.x + (int)m_energyCostsOfFrame, 0);
            m_energyCostsOfFrame -= (int)m_energyCostsOfFrame;
        }
        else if (!m_isPauseEnergyRecoveryDueAction && !m_isPauseEnergyRecoveryDueEmpty)
        {
            m_energyGainOfFrame += m_energyRecoverySpeed * Time.deltaTime * m_energyRecoverFactorByShielding;
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

    public void ExpendEnergyPoints(float expensesNegativeValue)
    {
        m_thisFrameEnergyWasConsumed = true;
        m_energyCostsOfFrame += expensesNegativeValue;
    }

    public void GainEnergyPoints(int energyGain)
    {
        if (m_characterStatsData.EnergyPoints.x + energyGain > m_characterStatsData.EnergyPoints.y)
        {
            int overshootEnergy = energyGain - (m_characterStatsData.EnergyPoints.y - m_characterStatsData.EnergyPoints.x);
            m_characterStatsData.EnergyPoints.x += energyGain - overshootEnergy;
            GainSpecialEnergy(overshootEnergy);
        }
        else
            m_characterStatsData.EnergyPoints.x += energyGain;
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

    public void ExpendSpecialEnergyPoints(int expenses)
    {
        m_characterStatsData.SpecialEnergyPoints.x = Mathf.Max(m_characterStatsData.SpecialEnergyPoints.x - expenses, 0);
    }

    public void GainSpecialEnergy(int specialEnergyGain)
    {
        m_characterStatsData.SpecialEnergyPoints.x = Mathf.Min(m_characterStatsData.SpecialEnergyPoints.x + specialEnergyGain, m_characterStatsData.SpecialEnergyPoints.y);
    }

    #endregion

    #region POISE
    private void RecoverPoise()
    {
        if (!m_canRecoverPoise)
            return;

        if (m_characterStatsData.PoisePoints.x != m_characterStatsData.PoisePoints.y)
            m_characterStatsData.PoisePoints.x = Mathf.Min(m_characterStatsData.PoisePoints.x + m_PoiseRecoverySpeed * Time.deltaTime, m_characterStatsData.PoisePoints.y);
    }

    public void TakePoiseDamage(int poiseDamage, StaggerType staggerType, Vector3 direction)
    {
        //betrifft nur damage den man nicht blockt
        if (poiseDamage == 0)
            return;

        m_characterStatsData.PoisePoints.x = Mathf.Max(m_characterStatsData.PoisePoints.x - poiseDamage, 0);

        DecideStagger(poiseDamage, staggerType, direction);

        //if depleated, then refill
        if (m_characterStatsData.PoisePoints.x == 0) m_characterStatsData.PoisePoints.x = m_characterStatsData.PoisePoints.y;

        //pause recovery
        m_canRecoverPoise = false;
        if (m_pausePoiseRecoveryCoroutine != null) 
        { 
            StopCoroutine(m_pausePoiseRecoveryCoroutine); 
            m_pauseEnergyRecoveryCoroutine = null; 
        }
        m_pausePoiseRecoveryCoroutine = StartCoroutine(UtilityFunctions.Wait(m_poiseRecoverPauseTime, () => { m_canRecoverPoise = true; } ));
    }

    #endregion
    
    #region STAGGER
    private void DecideStagger(int poiseValue, StaggerType staggerType, Vector3 direction)
    {
        //HEAVY STAGGER
        if (m_characterStatsData.PoisePoints.x <= 0)
        {
            switch (staggerType)
            {
                default: break;
                case StaggerType.Flinch_Only: m_playerMovement.TriggerDamage(); break;
                case StaggerType.Stun: m_playerMovement.TriggerStun(); break;
                case StaggerType.Stagger: m_playerMovement.TriggerStagger(); break;
                case StaggerType.Thrown_Over: m_playerMovement.TriggerFallingOver(direction); break;
                case StaggerType.Thrown_Over_Strong: m_playerMovement.TriggerFallingOver(direction); break;
            }
        }
        //LIGHT STAGGER
        else if ((m_characterStatsData.PoisePoints.x - poiseValue) / m_characterStatsData.PoisePoints.y < m_stunThreshhold.x && Mathf.Abs(poiseValue) >= m_characterStatsData.PoisePoints.y * m_stunThreshhold.y)
        {
            switch (staggerType)
            {
                default: break;
                case StaggerType.Flinch_Only: m_playerMovement.TriggerDamage(); break;
                case StaggerType.Stun: m_playerMovement.TriggerDamage(); break;
                case StaggerType.Stagger: m_playerMovement.TriggerStun(); break;
                case StaggerType.Thrown_Over: m_playerMovement.TriggerStagger(); break;
                case StaggerType.Thrown_Over_Strong: m_playerMovement.TriggerFallingOver(direction); break;
            }
        }

        else if (staggerType != StaggerType.None)
            m_playerMovement.TriggerDamage();
    }
    private void DecideStaggerShielding(int energyLost, StaggerType staggerType, Vector3 direction)
    {
        energyLost = -Mathf.Abs(energyLost);
        //HEAVY STAGGER
        if ((m_characterStatsData.EnergyPoints.x + energyLost) <= 0)
        {
            Debug.Log("heavy");
            switch (staggerType)
            {
                default: break;
                case StaggerType.Flinch_Only: m_playerMovement.TriggerShieldDeflect(); break;
                case StaggerType.Stun: m_playerMovement.TriggerShieldStun(); break;
                case StaggerType.Stagger: m_playerMovement.TriggerShieldBreak(); break;
                case StaggerType.Thrown_Over: m_playerMovement.TriggerFallingOver(direction); break;
                case StaggerType.Thrown_Over_Strong: m_playerMovement.TriggerFallingOver(direction); break;
            }
        }
        //LIGHT STAGGER
        else if ((m_characterStatsData.EnergyPoints.x + energyLost) / m_characterStatsData.EnergyPoints.y < m_shieldStunThreshhold.x && Mathf.Abs(energyLost) >= m_characterStatsData.EnergyPoints.y * m_shieldStunThreshhold.y)
        {
            Debug.Log($"light");
            switch (staggerType)
            {
                default: break;
                case StaggerType.Flinch_Only: m_playerMovement.TriggerShieldDeflect(); break;
                case StaggerType.Stun: m_playerMovement.TriggerDamage(); break;
                case StaggerType.Stagger: m_playerMovement.TriggerShieldStun(); break;
                case StaggerType.Thrown_Over: m_playerMovement.TriggerShieldBreak(); break;
                case StaggerType.Thrown_Over_Strong: m_playerMovement.TriggerFallingOver(direction); break;
            }
        }

        else if (staggerType != StaggerType.None)
            m_playerMovement.TriggerShieldDeflect();
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
