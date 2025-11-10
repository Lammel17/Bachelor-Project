using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterActionAndMovementHandler;
using System.Collections;

using System;


public class ActionMovementHandler : MonoBehaviour
{
    [SerializeField] private AnimationMovementData m_emptyFallbackAnimMoveData;
    [SerializeField] private CharacterController m_characterController; 
    private Coroutine m_ActionCoroutine = null;
    private bool m_disableSidewardMovement = false;
    private AnimationData m_currentActionAnimData = null;
    private float m_animationSpeed = 1;
    private bool m_isLockOn = false;
    private AnimationMovementData m_actionMovementData = null;
    private AnimationMovementData.TargetRelations m_actionTargetRelations = 0;
    private AnimationMovementData.TurningRelations m_actionTurningRelations = 0;
    public event Action OnEndAction;

    public bool IsLockOn { get => m_isLockOn; set => m_isLockOn = value; }
    public float AnimationSpeed { get => m_animationSpeed; set => m_animationSpeed = value; }
    public int ActionTurningRelation { get => (int)m_actionTurningRelations; }
    public Coroutine ActionCoroutine { get => m_ActionCoroutine; }















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










    public void StartAction(AnimationData animData, List<Action> effectList, float moveAcceleration, float turningStrenght, float maxTurningSpeed, Vector3 inputDirInWS, Vector3 characterForward)
    {
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



        foreach (var value in m_actionMovementData.variableValue)
        {
            if (value.ignore)
                continue;
            AnimationMovementData.Values.Settings valueData = value.settings;
            AnimationMovementData.Values.Settings.Influence influenceData = value.settings.customInfluenceOverInput;
            bool valueTypeIsConstant = valueData.valueType == AnimationMovementData.ValueType.ConstantValue;
            bool valueTypeIsStartEnd = valueData.valueType == AnimationMovementData.ValueType.StartEndValue;
            bool influenceValueTypeIsConstant = influenceData.influenceType == AnimationMovementData.InfluenceValueType.ConstantInfluence;
            bool influenceValueTypeIsStartEnd = influenceData.influenceType == AnimationMovementData.InfluenceValueType.StartEndInfluence;

            switch (value.valueName)
            {
                //MOVING
                case AnimationMovementData.ValueName.Move_Direction_Angle:
                    if (valueTypeIsConstant) m_directionByAction = Quaternion.Euler(0, valueData.value, 0) * m_directionByActionBaseValue;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverMoveDirection = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Move_Speed:
                    if (valueTypeIsConstant) m_speedByAction = valueData.value;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverMoveSpeed = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Move_Acceleration:
                    if (valueTypeIsConstant) m_moveAccelerationByAction = valueData.value;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverMoveAcceleration = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;

                //TURNING
                case AnimationMovementData.ValueName.Turning_Direction_Angle:

                    if (valueTypeIsConstant) m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, valueData.value, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverDesiredFacingRotationDirInWS = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Turning_Strenght:

                    if (valueTypeIsConstant) m_turningStrenghtByAction = valueData.value;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverTurningStrenght = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Max_Turning_Speed:

                    if (valueTypeIsConstant) m_maxTurningSpeedByInputByAction = valueData.value;
                    else if (valueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant) m_actionInfluenceOverMaxTurningSpeed = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd) RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;

            }
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

        DamageData actionDamageData = null;
        //List<int> activeHitBoxActiveDataList = new List<int>();
        //if (processedData.AnimationData.hitBoxActiveData.Count != 0)
        //    actionDamageData = m_currentWeaponAttackData != null ? m_characterStatus.GetActionDamageData(m_currentWeaponAttackData, transform.forward, m_movesetData.weapon.BasePhysicalType)
        //                                                         : m_characterStatus.GetActionDamageData(m_currentShieldActionData, transform.forward, m_movesetData.shield.PhysicalType);
        Action pauseMidAirAction = null;
        //if (processedData.AnimationData.IsPausingMidAir) pauseMidAirAction = () =>
        //{
        //    if (!m_isGrounded)
        //    {
        //        m_isMidAirPause = true;
        //        m_slowDownAnimSpeedCoroutine = StartCoroutine(SlowDownAnimSpeed());
        //        //m_animationSpeed = 0;
        //        //m_animator.speed = m_animationSpeed;
        //    }
        //};

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

                /*

                //INTERRUPTABILITY this is if a action is earlier interruptable than the lenght of the animation
                if (m_currentInteruptability != processedData.AnimationData.ChangedInterruptability)
                {
                    //processedData.AnimationData.InterruptabilityChangeBeforeEndTime;
                    float timetillChangeInteruptability = timeTillEnd - processedData.AnimationData.InterruptabilityChangeBeforeEndTime;
                    if (timetillChangeInteruptability <= 0)
                    {
                        m_characterStatus.ContinueEnergyRegenerationInTime();
                        m_currentInteruptability = processedData.AnimationData.ChangedInterruptability;
                        m_actionChangesInterruptabilityCoroutine = null;
                        if (m_playerInputManager.CheckRecallLatestBufferedInput())
                        {
                            EndActionReset();
                            yield break;
                        }
                    }
                    else
                        waitTime = Mathf.Min(timetillChangeInteruptability, waitTime);
                }

                //EFFECT LIST //effects like pay stamina cost or switch weapons
                if (processedData.Effects != null)
                {
                    //float timetillEffectTime = timeTillEnd - duration * (1 - processedData.AnimationData.MainActionMomentTime);
                    float timetillEffectTime = (duration * processedData.AnimationData.MainActionMomentTime) - ((duration - processedData.AnimationData.crossfadeOutBeginn) - timeTillEnd);
                    if (timetillEffectTime <= 0)
                    {
                        foreach (Action effect in processedData.Effects)
                        {
                            effect.Invoke();
                        }
                        processedData.Effects = null;
                    }
                    else
                        waitTime = Mathf.Min(timetillEffectTime, waitTime);
                }

                //PAUSE GRAVITY On and Off
                if (processedData.AnimationData.IsPausingMidAir)
                {
                    float timeTillGravityChange = waitTime;
                    if (relativeElapsedTime >= processedData.AnimationData.PauseGravityTime.x && relativeElapsedTime < processedData.AnimationData.PauseGravityTime.y && Gravity != 0)
                    {
                        Gravity = 0;
                        m_footPlacing.SetWeightActive(false);
                    }
                    else if (relativeElapsedTime > processedData.AnimationData.PauseGravityTime.y && Gravity == 0)
                    {
                        Gravity = m_originalGravity;
                    }

                    if (relativeElapsedTime < processedData.AnimationData.PauseGravityTime.x)
                        timeTillGravityChange = (processedData.AnimationData.PauseGravityTime.x - relativeElapsedTime) * duration;
                    else if (relativeElapsedTime < processedData.AnimationData.PauseGravityTime.y)
                        timeTillGravityChange = (processedData.AnimationData.PauseGravityTime.y - relativeElapsedTime) * duration;

                    waitTime = Mathf.Min(timeTillGravityChange, waitTime);

                }

                //PAUSE Animation when MidAir 
                if (processedData.AnimationData.IsPausingMidAir)
                {
                    if (pauseMidAirAction != null && relativeElapsedTime >= (processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration / 2) / duration))
                    {
                        pauseMidAirAction.Invoke();
                        pauseMidAirAction = null;
                    }

                    if (relativeElapsedTime < (processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration / 2) / duration))
                        waitTime = Mathf.Min(((processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration / 2) / duration) - relativeElapsedTime) * duration, waitTime);
                }


                //HITBOXES On and Off
                if (processedData.AnimationData.hitBoxActiveData.Count != 0)
                {
                    int activeDataIndex = 0;
                    float timetillNextHitBoxChange = timeTillEnd;
                    foreach (AnimationData.HitBoxActiveData hitActiveData in processedData.AnimationData.hitBoxActiveData)
                    {
                        if (relativeElapsedTime < hitActiveData.activeTime.x)   //before Hitbox activation
                        { timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.x - relativeElapsedTime) * duration, timetillNextHitBoxChange); }
                        else if (relativeElapsedTime < hitActiveData.activeTime.y) //while Hitbox activation
                        {
                            timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.y - relativeElapsedTime) * duration, timetillNextHitBoxChange);
                            if (!activeHitBoxActiveDataList.Contains(activeDataIndex))
                            {
                                if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) m_characterStatus.HitBoxManagerWeapon.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
                                if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
                                activeHitBoxActiveDataList.Add(activeDataIndex);
                            }
                        }
                        else if (relativeElapsedTime >= hitActiveData.activeTime.y)//after Hitbox activation
                        {
                            if (activeHitBoxActiveDataList.Contains(activeDataIndex))
                            {
                                if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) m_characterStatus.HitBoxManagerWeapon.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
                                if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
                                activeHitBoxActiveDataList.Remove(activeDataIndex);
                            }
                        }
                        activeDataIndex++;
                    }
                    waitTime = Mathf.Min(timetillNextHitBoxChange, waitTime);
                    //Debug.Log(relativeElapsedTime + timetillNextHitBoxChange / duration);
                }
                */

                //STARTEND VALUES
                foreach (var rangeData in processedData.RangeValuesList)
                {
                    float activeFactor = relativeElapsedTime >= rangeData.startEnd.x && relativeElapsedTime < rangeData.startEnd.y ? 1 : 0;
                    float valueInRange = rangeData.value * activeFactor;

                    //this calculates how long to wait for the next necessary canculation
                    float waitForTimeByRangeValues = timeTillEnd;
                    if (relativeElapsedTime < rangeData.startEnd.x) { waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime; }//wait till range start
                    else if (relativeElapsedTime < rangeData.startEnd.y) { waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime; }//wait till range end

                    waitTime = Math.Min(waitTime, waitForTimeByRangeValues);
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

            delayByMidAir += Time.deltaTime * (1 - m_animationSpeed);
            elapsedTime = Time.time - (startTime + delayByMidAir); // time must be added after the first wait

            m_actionTimeTillNextChange -= Time.deltaTime * m_animationSpeed;
            //Debug.Log(m_animationSpeed);
            //Debug.Log(elapsedTime);
        }


        EndAction();

        //HERE NOTHING MORE

    }

    public void EndAction()
    {

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

        //End Coroutines
        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        OnEndAction.Invoke();

    }







    public GameObject testMoveDirection;
    public GameObject testTurningDirection;

    private float m_turningAngle = 0;
    private Vector3 m_nowMoveDir = Vector3.forward;
    private Vector3 m_prevMove = Vector3.zero;

    public void RotatingPlayer(Vector3 m_desiredFacingRotationDirInWS, float m_maxTurningSpeed, float m_turningStrenght, Vector3 vectorToTarget)
    {
        //if no input, then it should not recalculate the desired facing direction, because what if i stand still and then lock on something behind me, it should not affect any calculation as long as i dont move
        // also, actions like evading set their initial m_desiredFacingRotationDirInWS in their own Trigger function
        //if (!m_isStandingStill)
        //    m_desiredFacingRotationDirInWS = OrientationRotation() * m_inputDirInWS;
        //else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && !m_isRunning)
        //    m_desiredFacingRotationDirInWS = PlayerToTargetXZVector;
        //else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && m_isRunning)
        //    m_desiredFacingRotationDirInWS = m_desiredFacingRotationDirInWS;


        //FacingDir
        Vector3 desiredFacingRotationDirInWSByInput = m_desiredFacingRotationDirInWS;
        Vector3 desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 1/*TurningDirFollowsTarget*/) desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up), 0) * UtilityFunctions.VectorXZ(vectorToTarget);
        else if ((int)m_actionTurningRelations == 1/*TurningDirFollowsMoveDir*/) desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up), 0) * m_nowMoveDir;
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput.normalized, desiredFacingRotationDirInWSByAction.normalized, m_actionInfluenceOverDesiredFacingRotationDirInWS);

        ////turningSpeedBy by movementSpeed
        //float m_maxTurningSpeed = ((m_currentBaseSpeed <= m_speedValues.y) ?
        //    UtilityFunctions.RefitToNewRange(m_prevMove.magnitude, m_speedValues.x, m_speedValues.y, m_maxTurningSpeedBaseValues.x, m_maxTurningSpeedBaseValues.y) :
        //    !m_isTurningApplied ? UtilityFunctions.RefitToNewRange(m_prevMove.magnitude, m_speedValues.y, m_speedValues.z, m_maxTurningSpeedBaseValues.y, m_maxTurningSpeedBaseValues.z) : m_maxTurningSpeedBaseValues.z);


        //speed
        float maxTurningSpeedByInput = m_maxTurningSpeed;
        float maxTurningSpeedByAction = m_maxTurningSpeedByInputByAction;
        float nowMaxTurningSpeed = Mathf.Lerp(maxTurningSpeedByInput, maxTurningSpeedByAction, m_actionInfluenceOverMaxTurningSpeed) * 60 * Time.deltaTime; //60 als faktor, damit maxspeed nicht so groß sein muss

        //acceleration
        float turningStrenghtByInput = m_turningStrenght;
        float turningStrenghtByAction = m_turningStrenghtByAction;
        float nowTurningStrenght = Mathf.Lerp(turningStrenghtByInput, turningStrenghtByAction, m_actionInfluenceOverTurningStrenght);

        float angle = Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up);
        float newAngle = Mathf.Clamp(Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up) * Time.deltaTime * nowTurningStrenght, -nowMaxTurningSpeed, nowMaxTurningSpeed); //Only ever 90° steps max per seconds, the turning speed
        m_turningAngle = angle;


        //this makes the char rotate not around it center when walking and turning, but rotates around a pont slightly to the side
        //float turnRotationPointOffsetXAxis = !m_isAction && !m_isIgnoreTurningOffset ? (Mathf.Sign(newAngle) * m_prevMove.magnitude / 1.8f) : 0;
        //Vector3 rotationCenterOffset = new Vector3(turnRotationPointOffsetXAxis, 0, 0);

        //RotateAround() isnt actually working when using Move()
        transform.RotateAround(transform.position/* + (transform.rotation * rotationCenterOffset)*/, Vector3.up, newAngle);

        testTurningDirection.transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, nowdesiredFacingRotationDirInWS, Vector3.up), 0);

    }

    public void MovingPlayer(Vector3 inputDirection, Vector3 vectorToTarget, float m_currentBaseSpeed, float m_moveAcceleration, Vector3 gravity)
    {
        //direction
        Vector3 directionByInput = inputDirection /*(!m_isFreelyMoving || m_isAboutSwitchDirectionType) ? m_inputDirInWS : transform.forward*/;
        Vector3 directionByAction = m_directionByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 2/*MoveDirFollowsTarget*/) directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * UtilityFunctions.VectorXZ(vectorToTarget);
        else if ((int)m_actionTurningRelations == 2/*MoveDirFollowsTurningDir*/) directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * transform.forward;
        Vector3 nowMoveDirection = Vector3.Lerp(directionByInput.normalized, directionByAction.normalized, m_actionInfluenceOverMoveDirection);
        if (nowMoveDirection != Vector3.zero) m_nowMoveDir = nowMoveDirection.normalized;

        ////speedFactor by turningangle
        //float speedFactorByAngle = ((m_isTurningApplied) ? Mathf.Lerp(1, 0, (Mathf.Max(Mathf.Abs(m_turningAngle) - 20, 0) / 50)) : 1);
        ////speedFactor by turningangle
        //float accelerationFactorByTurning = ((m_currentBaseSpeed > m_speedValues.y && m_isTurningApplied) || m_isSlidingApplied ? 0.2f : 1f);


        //speed
        float speedByInput = m_currentBaseSpeed /** speedFactorByAngle*/;
        float speedByAction = m_speedByAction;
        float nowSpeed = Mathf.Lerp(speedByInput, speedByAction, m_actionInfluenceOverMoveSpeed);

        //acceleration
        float moveAccelerationByInput = m_moveAcceleration /** accelerationFactorByTurning*/;
        float moveAccelerationByAction = m_moveAccelerationByAction;
        float nowMoveAcceleration = Mathf.Lerp(moveAccelerationByInput, moveAccelerationByAction, m_actionInfluenceOverMoveAcceleration);

        //this is for the case of uneven ground, the player will walk slower when walking on a hill/stairs
        //RaycastHit groundHitDir;
        //RaycastHit groundHit;
        //if (m_isGrounded && Physics.Raycast(transform.position + (m_nowMoveDir * (nowSpeed + 0.2f) * 0.2f) + (Vector3.up * 0.5f), Vector3.down, out groundHitDir, 1, m_environmentLayer) /*&& Vector3.Angle(Vector3.up, groundHit.normal) >= 10*/)
        //{
        //    Vector3 playerPos = (Physics.Raycast(m_chraracterMesh.transform.position + (Vector3.up * 0.5f), Vector3.down, out groundHit, 1, m_environmentLayer) ? groundHit.point : m_chraracterMesh.transform.position);
        //    m_nowMoveDir = Quaternion.AngleAxis(-Mathf.Min(45, 90 - Vector3.Angle(Vector3.up, (groundHitDir.point - playerPos).normalized)), Vector3.Cross(Vector3.up, groundHitDir.point - playerPos)) * m_nowMoveDir;
            //Debug.Log(m_nowMoveDir.magnitude);
            //Debug.Log(UtilityFunctions.VectorXZ(m_nowMoveDir).magnitude);
            //Debug.DrawLine(playerPos + Vector3.up * 0.5f, (playerPos + Vector3.up * 0.5f) + m_nowMoveDir.normalized * 5, Color.green);
            //Debug.DrawLine(playerPos + m_nowMoveDir * nowSpeed * 0.2f + Vector3.up * 0.5f, (playerPos + m_nowMoveDir * nowSpeed * 0.2f + Vector3.up * 0.5f) + Vector3.up * 5, Color.blue);
        //}
        //float terrainFactor = 1; // this is alternately the same as above, but only as a factor instead of rotationg the direction
        //if (m_isGrounded && Physics.Raycast(m_chraracterMesh.transform.position + (m_nowMoveDir * (nowSpeed + 0.2f) * 0.2f) + (Vector3.up * 0.5f), Vector3.down, out groundHitDir, 1, m_environmentLayer) /*&& Vector3.Angle(Vector3.up, groundHit.normal) >= 10*/)
        //    terrainFactor = Mathf.Cos(Mathf.Deg2Rad * Mathf.Abs(90 - Vector3.Angle(Vector3.up, (Physics.Raycast(m_chraracterMesh.transform.position + (Vector3.up * 0.5f), Vector3.down, out groundHit, 1, m_environmentLayer) ? groundHitDir.point - groundHit.point : m_chraracterMesh.transform.position).normalized)));

        Vector3 nowMove = UtilityFunctions.SmartLerp(m_prevMove, m_nowMoveDir * nowSpeed, Time.deltaTime * nowMoveAcceleration);

        //Gravity
        //if (m_currentGravity == 0)
        //    m_velocityThroughGravity = 0;
        //else if (m_characterController.isGrounded && m_velocityThroughGravity < 0)
        //    m_velocityThroughGravity = -2f; // small downward force to keep grounded
        //m_velocityThroughGravity += m_currentGravity * Time.deltaTime;
        //Vector3 gravity = new Vector3(0, m_velocityThroughGravity, 0);

        //Apply
        m_characterController.Move((nowMove + gravity) * Time.deltaTime);
        m_prevMove = nowMove;


        //Display
        if (nowMove != Vector3.zero) testMoveDirection.transform.rotation = Quaternion.LookRotation(nowMove, Vector3.up);
        else testMoveDirection.transform.localScale = new Vector3(0.5f, 0.07f, Mathf.Min(Mathf.Max(nowSpeed / 2, 0.2f), 1.5f));


    }



}
