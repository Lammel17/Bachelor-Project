using System;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] private CharacterStatsData m_characterStatsData;
    [SerializeField] private bool m_infinteStamina = false;


    private Coroutine m_pauseEnergyRecoveryCoroutine;

    [SerializeField] private float m_energyRecoverySpeed = 1f;
    [SerializeField] private float m_energyRecoveryPause = 1f;
    [Space]
    [SerializeField] private float m_specialEnergyRecoverySpeed = 1f;
    [Space]
    [SerializeField] private float m_PoiseRecoverySpeed = 1f;
    [SerializeField] private float m_buildUpsRecoverySpeed = 1f;

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


    [Space]
    [SerializeField] private float m_minRecoveredEnergyForAction = 40f;
    [SerializeField] private float m_minRecoveredEnergyConstantForAction = 90f;

    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_hp = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_ep = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_sep = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_poise = 1;
    [Space]
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_thermic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_electric = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(1, -1)] private float m_metaphysic = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_contamination = 0;

    private void Start()
    {
        if (m_characterStatsData == null)
        {
            m_characterStatsData = new CharacterStatsData();
            m_infinteStamina = true;
        }
    }


    private void Update()
    {
        SetHealthEveryFrame();

        SetEnergyEveryFrame();
        SetSpecialEnergyEveryFrame();

        RecoverPoise();
        RecoverBuildUps();

        m_hp = m_characterStatsData.HealthPoints.x / m_characterStatsData.HealthPoints.y;
        m_ep = m_characterStatsData.EnergyPoints.x / m_characterStatsData.EnergyPoints.y;
        m_sep = m_characterStatsData.SpecialEnergyPoints.x / m_characterStatsData.SpecialEnergyPoints.y;
        m_poise = m_characterStatsData.PoisePoints.x / m_characterStatsData.PoisePoints.y;

        m_thermic = m_characterStatsData.ThermicBuildUp.x >= 0 ? m_characterStatsData.ThermicBuildUp.x / m_characterStatsData.ThermicBuildUp.y : m_characterStatsData.ThermicBuildUp.x / m_characterStatsData.ThermicBuildUp.z;
        m_electric = m_characterStatsData.ElectricBuildUp.x >= 0 ? m_characterStatsData.ElectricBuildUp.x / m_characterStatsData.ElectricBuildUp.y : m_characterStatsData.ElectricBuildUp.x / m_characterStatsData.ElectricBuildUp.z;
        m_metaphysic = m_characterStatsData.MetaphysicBuildUp.x >= 0 ? m_characterStatsData.MetaphysicBuildUp.y / m_characterStatsData.MetaphysicBuildUp.y : m_characterStatsData.MetaphysicBuildUp.y / m_characterStatsData.MetaphysicBuildUp.z;
        m_contamination = m_characterStatsData.ContaminationBuildUp.x / m_characterStatsData.ContaminationBuildUp.y;

    }

    private void RecoverPoise()
    {
        if (m_characterStatsData.PoisePoints.x != m_characterStatsData.PoisePoints.y)
            m_characterStatsData.PoisePoints.x = Mathf.Min(m_characterStatsData.PoisePoints.x + m_PoiseRecoverySpeed * Time.deltaTime, m_characterStatsData.PoisePoints.y);
    }

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

    public void StopAll()
    {
        m_gainingHealthForTimeFactor = 0;
        m_loosingHealthForTimeFactor = 0;
        StopAllCoroutines();
        m_pauseEnergyRecoveryCoroutine = null;
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

    public void TakeDamage(DamageData damageData)
    {
        int damage =
            damageData.PhysicalSliceDamage * (1 - (m_characterStatsData.PhysicalSliceNegation / 100))
            + damageData.PhysicalBluntDamage * (1 - (m_characterStatsData.PhysicalBluntNegation / 100))
            + damageData.PhysicalPierceDamage * (1 - (m_characterStatsData.PhysicalPierceNegation / 100))
            + damageData.ThermicDamageAndBuildUp.x * (1 - (m_characterStatsData.ThermicNegation / 100))
            + damageData.ElectricDamageAndBuildUp.x * (1 - (m_characterStatsData.ElectricNegation / 100))
            + damageData.MetaphysicDamageAndBuildUp.x * (1 - (m_characterStatsData.MetaphysicNegation / 100));
        m_characterStatsData.HealthPoints.x = Mathf.Max(m_characterStatsData.HealthPoints.x - damage, 0);


        int thermicPoints = damageData.ThermicDamageAndBuildUp.y /** (1 - (m_characterStatsData.ThermicNegation / 200))*/;
        m_characterStatsData.ThermicBuildUp.x = thermicPoints >= 0 ? Mathf.Min(m_characterStatsData.ThermicBuildUp.x + thermicPoints, m_characterStatsData.ThermicBuildUp.y)
                                                                   : Mathf.Max(m_characterStatsData.ThermicBuildUp.x + thermicPoints, m_characterStatsData.ThermicBuildUp.z);
        int electricPoints = damageData.ElectricDamageAndBuildUp.y /** (1 - (m_characterStatsData.ElectricNegation / 200))*/;
        m_characterStatsData.ElectricBuildUp.x = electricPoints >= 0 ? Mathf.Min(m_characterStatsData.ElectricBuildUp.x + electricPoints, m_characterStatsData.ElectricBuildUp.y)
                                                                     : Mathf.Max(m_characterStatsData.ElectricBuildUp.x + electricPoints, m_characterStatsData.ElectricBuildUp.z);
        int metaphysicPoints = damageData.MetaphysicDamageAndBuildUp.y /** (1 - (m_characterStatsData.MetaphysicNegation / 200))*/;
        m_characterStatsData.MetaphysicBuildUp.x = metaphysicPoints >= 0 ? Mathf.Min(m_characterStatsData.MetaphysicBuildUp.x + metaphysicPoints, m_characterStatsData.MetaphysicBuildUp.y)
                                                                         : Mathf.Max(m_characterStatsData.MetaphysicBuildUp.x + metaphysicPoints, m_characterStatsData.MetaphysicBuildUp.z);

        int contaminationPoints = damageData.ContaminationBuildUpDamage;
        m_characterStatsData.ContaminationBuildUp.x = Mathf.Min(m_characterStatsData.ContaminationBuildUp.x + contaminationPoints, m_characterStatsData.ContaminationBuildUp.y);

        int poiseDamage = damageData.PoiseDamage;
        m_characterStatsData.PoisePoints.x = Mathf.Max(m_characterStatsData.PoisePoints.x - poiseDamage, 0);
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
            m_energyGainOfFrame -= (int)m_energyCostsOfFrame;
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

    



}
