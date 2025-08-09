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

    private float m_timeValueInPercent = 1;
    private float m_delayTimeValue = 0;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_absorption = 1;
    [SerializeField][EditorAttributes.ReadOnly] ImpactState m_state = ImpactState.WaitingFull;

    private Coroutine m_usingShieldCoroutine;
    private Coroutine m_recoverShieldCoroutine;
    private Coroutine m_delayRecoverShieldCoroutine;

    private enum ImpactState
    {
        WaitingFull,
        Recovering,
        Depleating,
        ImpactHalt,
        RecoverPause,
        WaitingEmpty,

    }

    private void Start()
    {
        m_characterActionAndMovementHandler = GetComponent<CharacterActionAndMovementHandler>();
    }

    public void SetCrystalValues(int maxGain, AnimationCurve curve, float duration)
    {
        m_maxEnergyPointsGain = maxGain;
        m_curveSpeed = curve;
        m_duration = duration;

        //m_timeValueInPercent = Mathf.Min(m_timeValueInPercent, m_duration);
        m_absorption = m_curveSpeed.Evaluate(m_timeValueInPercent);
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

        m_state = ImpactState.Depleating;
        m_usingShieldCoroutine = StartCoroutine(ShieldingUse());
    }

    public void StopUseShielding()
    {
        if (m_usingShieldCoroutine != null)
        {
            StopCoroutine(m_usingShieldCoroutine);
            m_usingShieldCoroutine = null;
        }
        m_state = ImpactState.RecoverPause;
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

        m_state = ImpactState.Recovering;
        m_recoverShieldCoroutine = StartCoroutine(ShieldingRecover());
    }



    public int EvaluateImpactAbsorpstion(int energyPoints)
    {
        int impactEnergy = 0;

        if (m_usingShieldCoroutine != null)
        {
            StopCoroutine(m_usingShieldCoroutine);
            m_usingShieldCoroutine = null;

            m_state = ImpactState.ImpactHalt;

            //calculate impact energy
            impactEnergy = Mathf.CeilToInt((Mathf.Abs(energyPoints) + Mathf.Min(Mathf.Abs(energyPoints), 30)) * m_absorption);
            return impactEnergy;
        }

        return impactEnergy;

    }



    private IEnumerator ShieldingUse()
    {
        while (m_timeValueInPercent != 0)
        {
            yield return null;
            m_timeValueInPercent = Mathf.Max(m_timeValueInPercent - (Time.deltaTime / m_duration), 0);
            m_absorption = m_curveSpeed.Evaluate(m_timeValueInPercent);
        }

        m_state = ImpactState.WaitingEmpty;
    }
    private IEnumerator ShieldingRecover()
    {
        while (m_timeValueInPercent != 1)
        {
            yield return null;
            m_timeValueInPercent = Mathf.Min(m_timeValueInPercent + (Time.deltaTime / m_duration), 1);
            m_absorption = m_curveSpeed.Evaluate(m_timeValueInPercent);
        }
        StopRecover();

    }
    private void StopRecover()
    {
        m_state = ImpactState.WaitingFull;

        if (m_recoverShieldCoroutine != null)
        {
            StopCoroutine(m_recoverShieldCoroutine);
            m_recoverShieldCoroutine = null;
        }
    }

}
