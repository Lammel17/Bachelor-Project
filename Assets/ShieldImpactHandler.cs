using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterActionAndMovementHandler))]
public class ShieldImpactHandler : MonoBehaviour
{
    private CharacterActionAndMovementHandler m_characterActionAndMovementHandler;
    private int m_maxEnergyPointsGain = 0;
    private AnimationCurve m_curveSpeed;
    private float m_duration;

    private float m_recoveryDalay = 0;
    private float m_perfectBlockTimeFrame = 0;

    private float m_timeValue = 1;
    private float m_delayTimeValue = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_absorption = 1;

    private Coroutine m_usingShieldCoroutine;
    private Coroutine m_recoverShieldCoroutine;
    private Coroutine m_delayRecoverShieldCoroutine;

    private void Start()
    {
        m_characterActionAndMovementHandler = GetComponent<CharacterActionAndMovementHandler>();
    }

    public void SetCrystalValues(int maxGain, AnimationCurve curve, float duration)
    {
        m_maxEnergyPointsGain = maxGain;
        m_curveSpeed = curve;
        m_duration = duration;

        m_timeValue = Mathf.Min(m_timeValue, m_duration);
        m_absorption = m_curveSpeed.Evaluate(m_timeValue);
    }
    public void SetShieldValues(float dalay, float perfBlockTime)
    {
        m_recoveryDalay = dalay;
        m_perfectBlockTimeFrame = perfBlockTime;
    }

    public void UseShielding()
    {
        if (m_recoverShieldCoroutine != null)
        {
            StopCoroutine(m_recoverShieldCoroutine);
            m_recoverShieldCoroutine = null;
        }
        if (m_delayRecoverShieldCoroutine != null)
        {
            StopCoroutine(m_delayRecoverShieldCoroutine);
            m_delayRecoverShieldCoroutine = null;
        }

        m_usingShieldCoroutine = StartCoroutine(ShieldingUse());
    }

    public void StopUseShielding()
    {
        if (m_usingShieldCoroutine != null)
        {
            StopCoroutine(m_usingShieldCoroutine);
            m_usingShieldCoroutine = null;
        }

        m_delayRecoverShieldCoroutine = StartCoroutine(DelayForRecovering());
    }
    private IEnumerator DelayForRecovering()
    {
        m_delayTimeValue = m_recoveryDalay;
        while (m_delayTimeValue > 0)
        {
            yield return null; 
            m_delayTimeValue = Mathf.Max(m_delayTimeValue - Time.deltaTime, 0);
        }
        m_recoverShieldCoroutine = StartCoroutine(ShieldingRecover());
    }



    public void ImpactStop()
    {
        StopCoroutine(m_usingShieldCoroutine);
        m_usingShieldCoroutine = null;

        //calculate
    }
    private IEnumerator ShieldingUse()
    {
        while (m_timeValue != 0)
        {
            yield return null;
            m_timeValue = Mathf.Max(m_timeValue - (Time.deltaTime / m_duration), 0);
            m_absorption = m_curveSpeed.Evaluate(m_timeValue);
        }

    }

    private IEnumerator ShieldingRecover()
    {
        while (m_timeValue != m_duration)
        {
            yield return null;
            m_timeValue = Mathf.Min(m_timeValue + (Time.deltaTime / m_duration), 1);
            m_absorption = m_curveSpeed.Evaluate(m_timeValue);
        }
        StopRecover();

    }
    private void StopRecover()
    {
        StopCoroutine(m_recoverShieldCoroutine);
        m_recoverShieldCoroutine = null;
    }

}
