using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private CharacterStatus m_playerStatus;
    [SerializeField] private ShieldImpactHandler m_shieldImpactHandler;

    [SerializeField] private Image m_hp;
    [SerializeField] private Image m_hpDelay;
    [SerializeField] private Image m_ep;
    [SerializeField] private Image m_epDelay;
    [SerializeField] private Image m_sep;
    [SerializeField] private Image m_sepDelay;
    [SerializeField] private Image m_shieldBar;
    [SerializeField] private Image m_shiledImpact;

    [SerializeField] private float m_delayTime = 1;
    [SerializeField] private AnimationCurve m_delayCurve;
    [SerializeField] private float  m_delayCurveFactor = 1;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_playerStatus = GameManager.Instance.Player.GetComponent<CharacterStatus>();
        m_shieldImpactHandler = GameManager.Instance.Player.GetComponent<ShieldImpactHandler>();
    }

    private void OnEnable()
    {
        m_playerStatus.OnHPChanged += UpdateHP;
        m_hp.fillAmount = m_playerStatus.StatsData.HealthPoints.x / m_playerStatus.StatsData.HealthPoints.y;
        m_hpDelay.fillAmount = m_hp.fillAmount;

        m_playerStatus.OnEPChanged += UpdateEP;
        m_ep.fillAmount = m_playerStatus.StatsData.EnergyPoints.x / m_playerStatus.StatsData.EnergyPoints.y;
        m_epDelay.fillAmount = m_ep.fillAmount;

        m_playerStatus.OnSEPChanged += UpdateSEP;
        m_sep.fillAmount = m_playerStatus.StatsData.SpecialEnergyPoints.x / m_playerStatus.StatsData.SpecialEnergyPoints.y;
        m_sepDelay.fillAmount = m_sep.fillAmount;

        m_shieldImpactHandler.OnShieldChanged += UpdateShield;
        m_shieldBar.fillAmount = 1;
        m_shiledImpact.fillAmount = 0;
    }

    private void OnDisable()
    {
        m_playerStatus.OnHPChanged -= UpdateHP;
        m_playerStatus.OnEPChanged -= UpdateEP;
        m_playerStatus.OnSEPChanged -= UpdateSEP;
        m_shieldImpactHandler.OnShieldChanged -= UpdateShield;


    }


    private void UpdateHP(float current, float max , bool gainingPoints = false)
    {
        if (!gainingPoints) m_hpDelay.fillAmount = m_hp.fillAmount;

        m_hp.fillAmount = current / max;

        if (!gainingPoints && m_hp.fillAmount < m_hpDelay.fillAmount)
        {
            if (m_hpBarDelayCoroutine != null)
            {
                StopCoroutine(m_hpBarDelayCoroutine);
                m_hpBarDelayCoroutine = null;
            }
            m_hpBarDelayCoroutine = StartCoroutine(DelayBar(m_hp, m_hpDelay));
        }
    }

    private void UpdateEP(float current, float max, bool gainingPoints = false)
    {
        if (!gainingPoints /*|| m_ep.fillAmount > m_epDelay.fillAmount*/) m_epDelay.fillAmount = m_ep.fillAmount;

        m_ep.fillAmount = current / max;

        if (!gainingPoints && m_ep.fillAmount < m_epDelay.fillAmount)
        {
            if (m_epBarDelayCoroutine != null)
            {
                StopCoroutine(m_epBarDelayCoroutine);
                m_epBarDelayCoroutine = null;
            }
            m_epBarDelayCoroutine = StartCoroutine(DelayBar(m_ep, m_epDelay));
        }
    }

    private void UpdateSEP(float current, float max, bool gainingPoints = false)
    {
        if (!gainingPoints) m_sepDelay.fillAmount = m_sep.fillAmount;

        m_sep.fillAmount = current / max;

        if (!gainingPoints && m_sep.fillAmount < m_sepDelay.fillAmount)
        {
            if (m_sepBarDelayCoroutine != null)
            {
                StopCoroutine(m_sepBarDelayCoroutine);
                m_sepBarDelayCoroutine = null;
            }
            m_sepBarDelayCoroutine = StartCoroutine(DelayBar(m_sep, m_sepDelay));
        }
    }

    private Coroutine m_hpBarDelayCoroutine;
    private Coroutine m_epBarDelayCoroutine;
    private Coroutine m_sepBarDelayCoroutine;

    private IEnumerator DelayBar(Image bar, Image delayBar)
    {
        float delayTime = 0;

        while (delayBar.fillAmount > bar.fillAmount)
        {
            delayTime += Time.deltaTime / m_delayTime;
            //Debug.Log(delayTime / m_delayTime);
            delayBar.fillAmount -= m_delayCurve.Evaluate(delayTime/ m_delayTime) * m_delayCurveFactor;
            //Debug.Log(delayBar.fillAmount);

            yield return null;
        }

        delayBar.fillAmount = bar.fillAmount;
    }




    private void UpdateShield(float current, bool isImpact = false)
    {
        m_shieldBar.fillAmount = current;
        if (isImpact) m_shiledImpact.fillAmount = current;
        else m_shiledImpact.fillAmount = 0;
    }








}
