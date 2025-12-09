using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterActionAndMovementHandler;
using System.Collections;

using System;
using Unity.Collections;


public class ActionMovementHandler : MonoBehaviour
{
    [SerializeField] private AnimationMovementData m_emptyFallbackAnimMoveData;
    private Coroutine m_ActionCoroutine = null;
    private bool m_disableSidewardMovement = false;
    private AnimationData m_currentActionAnimData = null;
    private float m_animationSpeed = 1;
    private bool m_isLockOn = false;
    private AnimationMovementData m_actionMovementData = null;
    private AnimationMovementData.TargetRelations m_actionTargetRelations = 0;
    private AnimationMovementData.TurningRelations m_actionTurningRelations = 0;
    public event Action OnEndAndResetAction;
    public event Action OnEndActionBeforeReset;
    //public List<EffectQueue> m_effectQueue = new List<EffectQueue>();
    [SerializeField] [EditorAttributes.ReadOnly] private State m_state = State.Stop;

    public bool IsLockOn { get => m_isLockOn; set => m_isLockOn = value; }
    public float AnimationSpeed { get => m_animationSpeed; set => m_animationSpeed = value; }
    public int ActionTurningRelation { get => (int)m_actionTurningRelations; }
    public Coroutine ActionCoroutine { get => m_ActionCoroutine; }
    public float DesiredTurningAngle { get => m_turningAngle; }

    private enum State
    {
        Stop,
        Start,
        InAction,
    }









    //Action Influence Values
    private Vector3 m_directionByAction = Vector3.forward;
    private float m_actionInfluenceOverMoveDirection = 0;

    private float m_speedByAction = 0;
    private float m_actionInfluenceOverMoveSpeed = 0;

    private float m_moveAccelerationByAction = 0;
    private float m_actionInfluenceOverMoveAcceleration = 0;

    private Vector3 m_desiredFacingRotationDirInWSByAction = Vector3.forward;
    private float m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;

    private float m_turningStrenghtByAction = 0;
    private float m_actionInfluenceOverTurningStrenght = 0;

    private float m_maxTurningSpeedByInputByAction = 0;
    private float m_actionInfluenceOverMaxTurningSpeed = 0;

    private Vector3 m_directionByActionBaseValue = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWSByActionBaseValue = Vector3.forward;


    private float m_actionTimeTillNextChange = 0f;










    public void StartAction(AnimationData animData, List<EffectQueue> effectList, float moveAcceleration, float turningStrenght, float maxTurningSpeed, Vector3 inputDirInWS, Vector3 characterForward)
    {
        m_state = State.Start;
        m_actionMovementData = animData.AnimationMovementData;
        float animationDuration = animData.animationClip.length;
        float crossfadeOutTime = animData.crossfadeOutTime;
        float crossfadeStartBeforeEndTime = Mathf.Max(0, 1f, animData.crossfadeOutBeginn);

        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        List<ProcessedAnimationMovementData.DataCurves> CurveValuesList = new List<ProcessedAnimationMovementData.DataCurves>();
        List<ProcessedAnimationMovementData.DataStartEnd> RangeValuesList = new List<ProcessedAnimationMovementData.DataStartEnd>();

        if (m_actionMovementData == null)
        {
            Debug.Log("ANIMATION_Moveset_DATA IS NULL");
            m_actionMovementData = m_emptyFallbackAnimMoveData;
        }


        int moveDirPredefinition = (int)m_actionMovementData.moveDirPredefinition;
        int turningDirPredefinition = (int)m_actionMovementData.turningDirPredefinition;
        float startMoveInfluence = m_actionMovementData.moveInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;
        float startTurningInfluence = m_actionMovementData.turningInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;

        m_actionTargetRelations = m_actionMovementData.targetRelations;
        m_actionTurningRelations = m_actionMovementData.turningRelations;
        m_disableSidewardMovement = m_actionMovementData.m_disableSidewardMovement;


        //initial moveDir
        if ((int)m_actionTurningRelations == 2 /*MoveDirFollowsTurningDir*/ || (m_isLockOn && (int)m_actionTargetRelations == 2 /*MoveDirFollowsTarget*/))
        {
            m_directionByAction = /*Quaternion.Inverse(AdditionalFacingRotation()) * */ Vector3.forward;
            m_directionByActionBaseValue = Vector3.forward;
        }
        else
        {
            Vector3 moveDir = Vector3.zero;
            if (moveDirPredefinition == 1 /*LatestInput*/) moveDir = inputDirInWS;
            if (moveDirPredefinition == 2 /*LatestFrame)*/) moveDir = characterForward;
            m_directionByAction = m_directionByActionBaseValue = moveDir;
        }
        m_actionInfluenceOverMoveDirection = startMoveInfluence;
        m_speedByAction = 0; // is set to 0
        m_actionInfluenceOverMoveSpeed = startMoveInfluence;
        m_moveAccelerationByAction = moveAcceleration; // is set to current acc
        m_actionInfluenceOverMoveAcceleration = startMoveInfluence;


        //initial turningDir
        if ((int)m_actionTurningRelations == 1 /*TurningDirFollowsMoveDir*/ || (m_isLockOn && (int)m_actionTargetRelations == 1 /*TurningDirFollowsTarget*/))
        {
            m_desiredFacingRotationDirInWSByAction = /*AdditionalFacingRotation() **/ Vector3.forward;
            m_desiredFacingRotationDirInWSByActionBaseValue = Vector3.forward;
        }
        else
        {
            Vector3 turningDir = Vector3.zero;
            if (turningDirPredefinition == 1 /*latestInputWithAddTurning*/) turningDir = /*AdditionalFacingRotation() **/ inputDirInWS;
            if (turningDirPredefinition == 2 /*latestFrame)*/) turningDir = transform.forward;
            m_desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByActionBaseValue = turningDir;
        }
        m_actionInfluenceOverDesiredFacingRotationDirInWS = startTurningInfluence;
        m_turningStrenghtByAction = turningStrenght; // is set to current strenght
        m_actionInfluenceOverTurningStrenght = startTurningInfluence;
        m_maxTurningSpeedByInputByAction = maxTurningSpeed; // is set to current maxspeed
        m_actionInfluenceOverMaxTurningSpeed = startTurningInfluence;

        if (m_actionMovementData.moveDirection.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.moveDirection;
            if (IsApplyValueForFirstFrame(valueData, false))    m_directionByAction = Quaternion.Euler(0, valueData.value, 0) * m_directionByActionBaseValue;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverMoveDirection = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Move_Direction_Angle);
        }
        if (m_actionMovementData.moveSpeed.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.moveSpeed;
            if (IsApplyValueForFirstFrame(valueData, false))    m_speedByAction = valueData.value;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverMoveSpeed = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Move_Speed);
        }
        if (m_actionMovementData.moveAcceleration.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.moveAcceleration;
            if (IsApplyValueForFirstFrame(valueData, false))    m_moveAccelerationByAction = valueData.value;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverMoveAcceleration = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Move_Acceleration);
        }

        if (m_actionMovementData.turningDirection.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.turningDirection;
            if (IsApplyValueForFirstFrame(valueData, false))    m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, valueData.value, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverDesiredFacingRotationDirInWS = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle);
        }
        if (m_actionMovementData.turningMaxSpeed.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.turningMaxSpeed;
            if (IsApplyValueForFirstFrame(valueData, false))    m_maxTurningSpeedByInputByAction = valueData.value;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverMaxTurningSpeed = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Max_Turning_Speed);
        }
        if (m_actionMovementData.turningStrenght.isInUse)
        {
            MovementValuesData valueData = m_actionMovementData.turningStrenght;
            if (IsApplyValueForFirstFrame(valueData, false))    m_turningStrenghtByAction = valueData.value;
            if (IsApplyValueForFirstFrame(valueData, true))     m_actionInfluenceOverTurningStrenght = valueData.influence;
            ProcessData(valueData, ProcessedAnimationMovementData.ValueName.Turning_Strenght);
        }
        
        void ProcessData(MovementValuesData valueData, ProcessedAnimationMovementData.ValueName valueName)
        {
            if (valueData.valueType == ValueType.StartEndValue)                     RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(valueName, valueData.value, valueData.startEnd));
            else if (valueData.valueType == ValueType.CurvedValue)                  CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(valueName, valueData.value, valueData.startEnd, valueData.curve));

            if (valueData.influenceType == InfluenceValueType.StartEndInfluence)    RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(valueName + 1, valueData.influence, valueData.influenceStartEnd));
            else if (valueData.influenceType == InfluenceValueType.CurvedInfluence) CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(valueName + 1, valueData.influence, valueData.influenceStartEnd, valueData.influenceCurve));
        }
        bool IsApplyValueForFirstFrame(MovementValuesData valueData, bool influence)
        {
            if (!influence)     return !(valueData.valueType == ValueType.CurvedValue || valueData.startEnd.x != 0);
            else                return !(valueData.influenceType == InfluenceValueType.CurvedInfluence || valueData.influenceStartEnd.x != 0);
        }


        m_currentActionAnimData = animData;
        ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, animData, effectList); //This could be saved somewhere in future!

        m_ActionCoroutine = StartCoroutine(PerformAction(processedData));
    }


    private IEnumerator PerformAction(ProcessedAnimationMovementData processedData)
    {
        float elapsedTime = 0;
        float startTime = Time.time;
        float timeSteps = processedData.AnimationData.AnimationMovementData == null ? 0.05f : processedData.AnimationData.AnimationMovementData.timeStepsForCurves;
        float delayByMidAir = 0;

        float duration = processedData.AnimationData.animationClip.length; //what about blendtrees, do they affect it?

        void SetValueByName(ProcessedAnimationMovementData.ValueName name, float newValue)
        {
            switch (name)
            {
                case ProcessedAnimationMovementData.ValueName.Move_Direction_Angle:
                    m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
                    break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle: m_actionInfluenceOverMoveDirection = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Speed: m_speedByAction = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed: m_actionInfluenceOverMoveSpeed = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Acceleration: m_moveAccelerationByAction = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration: m_actionInfluenceOverMoveAcceleration = newValue; break;

                case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:
                    m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle: m_actionInfluenceOverDesiredFacingRotationDirInWS = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Turning_Strenght: m_turningStrenghtByAction = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght: m_actionInfluenceOverTurningStrenght = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Max_Turning_Speed: m_maxTurningSpeedByInputByAction = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed: m_actionInfluenceOverMaxTurningSpeed = newValue; break;
            }
        }

        while (elapsedTime <= duration - processedData.AnimationData.crossfadeOutBeginn)
        {

            if (m_actionTimeTillNextChange <= 0)
            {
                float timeTillEnd = ((duration - processedData.AnimationData.crossfadeOutBeginn) - elapsedTime);
                float waitTime = timeTillEnd;
                float relativeElapsedTime = elapsedTime / duration;

                foreach (EffectQueue effectQueue in processedData.Effects)
                {
                    if (effectQueue.wasApplied)
                        continue;
                    else if (relativeElapsedTime >= effectQueue.relativeEffectTime)
                    {
                        effectQueue.effect?.Invoke();
                        effectQueue.wasApplied = true;
                        if (m_state != State.InAction && elapsedTime != 0) 
                            yield break;
                    }
                    else
                    {
                        float timeUntilNextChange = (effectQueue.relativeEffectTime - relativeElapsedTime) * duration;
                        waitTime = Mathf.Min(waitTime, timeUntilNextChange);
                    }
                }

                //STARTEND VALUES
                foreach (var rangeData in processedData.RangeValuesList)
                {
                    float activeFactor = relativeElapsedTime >= rangeData.startEnd.x && relativeElapsedTime < rangeData.startEnd.y ? 1 : 0;
                    float valueInRange = rangeData.value * activeFactor;

                    //this calculates how long to wait for the next necessary canculation
                    float waitForTimeByRangeValues = timeTillEnd;
                    if (relativeElapsedTime < rangeData.startEnd.x) { waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime; }//wait till range start
                    else if (relativeElapsedTime < rangeData.startEnd.y) { waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime; }//wait till range end

                    waitTime = Mathf.Min(waitTime, waitForTimeByRangeValues);
                    SetValueByName(rangeData.name, valueInRange);
                }

                //CURVE VALUES
                foreach (var curveData in processedData.CurveValuesList)
                {
                    float activeFactor = relativeElapsedTime > curveData.startEnd.x && relativeElapsedTime < curveData.startEnd.y ? 1 : 0;
                    float curveValue = curveData.value * curveData.curve.Evaluate(Mathf.InverseLerp(curveData.startEnd.x, curveData.startEnd.y, relativeElapsedTime)) * activeFactor;

                    //this calculates how long to wait for the next necessary canculation
                    float waitForTimeByCurveValues = timeSteps;
                    if (relativeElapsedTime < curveData.startEnd.x) waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.x * duration) - elapsedTime); //wait till range start or timeToWait
                    else if (relativeElapsedTime < curveData.startEnd.y) waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.y * duration) - elapsedTime); //wait till range end or timeToWait                                                                        //wait till timeToWait

                    waitTime = Mathf.Min(waitTime, waitForTimeByCurveValues);
                    SetValueByName(curveData.name, curveValue);
                }

                m_actionTimeTillNextChange = waitTime;
            }
            //if (processedData.AnimationData.AnimationMovementData.ActionDescription == "long") Debug.Log("U");

            yield return null;
            m_state = State.InAction;

            delayByMidAir += Time.deltaTime * (1 - m_animationSpeed);
            elapsedTime = Time.time - (startTime + delayByMidAir); // time must be added after the first wait

            m_actionTimeTillNextChange -= Time.deltaTime * m_animationSpeed;
            //Debug.Log(m_animationSpeed);
            //Debug.Log(elapsedTime);
        }

        OnEndActionBeforeReset?.Invoke();

        EndAction();

        //HERE NOTHING MORE

    }

    public void EndAction()
    {
        m_state = State.Stop;

        //reset influence of Action
        m_actionInfluenceOverMoveDirection = 0;
        m_actionInfluenceOverMoveSpeed = 0;
        m_actionInfluenceOverMoveAcceleration = 0;
        m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;
        m_actionInfluenceOverTurningStrenght = 0;
        m_actionInfluenceOverMaxTurningSpeed = 0;

        //reset values
        m_disableSidewardMovement = false;
        m_actionTargetRelations = 0;
        m_actionTurningRelations = 0;
        //m_effectQueue = new List<EffectQueue>();

        //End Coroutines
        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        OnEndAndResetAction?.Invoke();

    }







    public GameObject testMoveDirection;
    public GameObject testTurningDirection;

    private float m_turningAngle = 0;
    private Vector3 m_nowMoveDir = Vector3.forward;
    private Vector3 m_prevMove = Vector3.zero;

    public float GetRotation(ref Vector3 m_desiredFacingRotationDirInWS, ref float m_maxTurningSpeed, ref float m_turningStrenght, ref Vector3 vectorToTarget)
    {
        //FacingDir
        Vector3 desiredFacingRotationDirInWSByInput = m_desiredFacingRotationDirInWS;
        Vector3 desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 1/*TurningDirFollowsTarget*/) desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up), 0) * UtilityFunctions.VectorXZ(vectorToTarget);
        else if ((int)m_actionTurningRelations == 1/*TurningDirFollowsMoveDir*/) desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up), 0) * m_nowMoveDir;
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput.normalized, desiredFacingRotationDirInWSByAction.normalized, m_actionInfluenceOverDesiredFacingRotationDirInWS);
        
        //speed
        float maxTurningSpeedByInput = m_maxTurningSpeed;
        float maxTurningSpeedByAction = m_maxTurningSpeedByInputByAction;
        float nowMaxTurningSpeed = Mathf.Lerp(maxTurningSpeedByInput, maxTurningSpeedByAction, m_actionInfluenceOverMaxTurningSpeed) * 60 * Time.deltaTime; //60 als faktor, damit maxspeed nicht so groﬂ sein muss

        //acceleration
        float turningStrenghtByInput = m_turningStrenght;
        float turningStrenghtByAction = m_turningStrenghtByAction;
        float nowTurningStrenght = Mathf.Lerp(turningStrenghtByInput, turningStrenghtByAction, m_actionInfluenceOverTurningStrenght);

        float angle = Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up);
        float newAngle = Mathf.Clamp(Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up) * Time.deltaTime * nowTurningStrenght, -nowMaxTurningSpeed, nowMaxTurningSpeed); //Only ever 90∞ steps max per seconds, the turning speed
        
        m_turningAngle = angle;


        //this makes the char rotate not around it center when walking and turning, but rotates around a pont slightly to the side
        //float turnRotationPointOffsetXAxis = !m_isAction && !m_isIgnoreTurningOffset ? (Mathf.Sign(newAngle) * m_prevMove.magnitude / 1.8f) : 0;
        //Vector3 rotationCenterOffset = new Vector3(turnRotationPointOffsetXAxis, 0, 0);

        //RotateAround() isnt actually working when using Move()
        //transform.RotateAround(transform.position/* + (transform.rotation * rotationCenterOffset)*/, Vector3.up, newAngle);

        testTurningDirection.transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, nowdesiredFacingRotationDirInWS, Vector3.up), 0);

        return newAngle;

    }

    public Vector3 GetMove(ref Vector3 inputDirection, ref float m_currentBaseSpeed, ref float m_moveAcceleration, ref Vector3 vectorToTarget)
    {
        //direction
        Vector3 directionByInput = inputDirection /*(!m_isFreelyMoving || m_isAboutSwitchDirectionType) ? m_inputDirInWS : transform.forward*/;
        Vector3 directionByAction = m_directionByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 2/*MoveDirFollowsTarget*/) directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * UtilityFunctions.VectorXZ(vectorToTarget);
        else if ((int)m_actionTurningRelations == 2/*MoveDirFollowsTurningDir*/) directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * transform.forward;
        Vector3 nowMoveDirection = Vector3.Lerp(directionByInput.normalized, directionByAction.normalized, m_actionInfluenceOverMoveDirection);
        if (nowMoveDirection != Vector3.zero) m_nowMoveDir = nowMoveDirection.normalized;
 
        //speed
        float speedByInput = m_currentBaseSpeed /** speedFactorByAngle*/;
        float speedByAction = m_speedByAction;
        float nowSpeed = Mathf.Lerp(speedByInput, speedByAction, m_actionInfluenceOverMoveSpeed);

        //acceleration
        float moveAccelerationByInput = m_moveAcceleration /** accelerationFactorByTurning*/;
        float moveAccelerationByAction = m_moveAccelerationByAction;
        float nowMoveAcceleration = Mathf.Lerp(moveAccelerationByInput, moveAccelerationByAction, m_actionInfluenceOverMoveAcceleration);

        Vector3 nowMove = UtilityFunctions.SmartLerp(m_prevMove, m_nowMoveDir * nowSpeed, Time.deltaTime * nowMoveAcceleration);

        m_prevMove = nowMove;

        //Display
        if (nowMove != Vector3.zero) testMoveDirection.transform.rotation = Quaternion.LookRotation(nowMove, Vector3.up);
        else testMoveDirection.transform.localScale = new Vector3(0.5f, 0.07f, Mathf.Min(Mathf.Max(nowSpeed / 2, 0.2f), 1.5f));

        return nowMove;

    }



}
