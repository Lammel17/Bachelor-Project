using System;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField] private CharacterStatsData m_characterStatsData;
    [SerializeField] private bool m_infinteStamina = false;


    private Coroutine m_pauseEnergyRecoveryCoroutine;
    //private Coroutine m_energyIsEmptyCoroutine;

    [SerializeField] private float m_energyRecoverySpeed = 1f;
    [SerializeField] private float m_energyRecoveryPause = 1f;
    private bool m_isPauseEnergyRecoveryDueAction = false;
    private bool m_isPauseEnergyRecoveryDueDelay = false;
    private bool m_isPauseEnergyRecoveryDueEmpty = false;
    private bool m_isEnergyExhausted = false;

    private float m_energyCostsOfFrame = 0;
    private bool m_thisFrameEnergyWasConsumed = false;

    [Space]
    [SerializeField] private float m_minRecoveredEnergy = 40f;
    [SerializeField] private float m_minRecoveredEnergyConstant = 90f;

    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_hp = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_ep = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_sep = 1;
    [SerializeField][EditorAttributes.ReadOnly] [Range(0, 1)] private float m_poise = 1;

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
        if (m_thisFrameEnergyWasConsumed)
        {
            m_thisFrameEnergyWasConsumed = false;
            m_characterStatsData.EnergyPoints.x = Mathf.Max(m_characterStatsData.EnergyPoints.x - (int)m_energyCostsOfFrame, 0);
            m_energyCostsOfFrame -= (int)m_energyCostsOfFrame;
        }
        else if (!m_isPauseEnergyRecoveryDueAction && !m_isPauseEnergyRecoveryDueEmpty)
            m_characterStatsData.EnergyPoints.x = Mathf.Min(m_characterStatsData.EnergyPoints.x + m_energyRecoverySpeed * Time.deltaTime, m_characterStatsData.EnergyPoints.y);
        

        if (m_characterStatsData.EnergyPoints.x == 0 && !m_isEnergyExhausted)
        {
            m_isEnergyExhausted = true;
            m_isPauseEnergyRecoveryDueEmpty = true;

            //if currently is action, the coroutine will be called at the end of the action anyways
            if (!m_isPauseEnergyRecoveryDueAction) 
                ContinueEnergyRegenerationInTime();
        }
        

        m_hp = m_characterStatsData.HealthPoints.x / m_characterStatsData.HealthPoints.y;
        m_ep = m_characterStatsData.EnergyPoints.x / m_characterStatsData.EnergyPoints.y;
        m_sep = m_characterStatsData.SpecialEnergyPoints.x / m_characterStatsData.SpecialEnergyPoints.y;
        m_poise = m_characterStatsData.PoisePoints.x / m_characterStatsData.PoisePoints.y;
    }



    public void ConsumeEnergyPoints(float actionCost)
    {
        m_thisFrameEnergyWasConsumed = true;
        m_energyCostsOfFrame += actionCost;
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
        else if (m_isEnergyExhausted && m_characterStatsData.EnergyPoints.x >= m_minRecoveredEnergy)
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
        else if (m_isEnergyExhausted && m_characterStatsData.EnergyPoints.x >= m_minRecoveredEnergyConstant)
        {
            m_isEnergyExhausted = false;
            return true;
        }
        else
            return false;
    }
    public bool CheckIfCanConsumeSpecialEnergy(int cost)
    {
        if (m_characterStatsData.SpecialEnergyPoints.x >= cost)
            return true;
        else 
            return false;
    }


    //public void LooseFixedHealthPoints(int loosePoints)
    //{
    //    m_characterStatsData.HealthPoints.x = Mathf.Max(m_characterStatsData.HealthPoints.x - loosePoints, 0);

    //}

    //public void TakeDamageOnHealthPoints(int damageData)
    //{

    //    m_characterStatsData.HealthPoints.x = Mathf.Max(m_characterStatsData.HealthPoints.x , 0);

    //}





}
