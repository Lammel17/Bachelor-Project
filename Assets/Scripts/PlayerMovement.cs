using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using EditorAttributes;
using TMPro;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerCameraHolder m_playerCameraHolder; 
    private PlayerInputManager m_playerInputManager;
    [SerializeField] private CharacterMovesetData m_characterMovesetData;

    [SerializeField] private Animator m_animator;
    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    private float m_inputFactor = 1f; //should stay 1
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private float m_turningStrenghtBaseValue = 7f;
    [SerializeField] private float m_maxTurningSpeedBaseValue = 50f;
    private const int m_runningMoveStrenght = 2;

    private float m_moveStrength = 0f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_inputDirInWS = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWS = Vector3.forward;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningStrenght;
    private float m_maxTurningSpeed;
    private float m_speed = 0; //slow, walk, running


    //Values Depending on Camera
    private Quaternion m_cameraYAxisRotationInWS = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private enum Direction { Forward, Left, Right, Backward };
    [SerializeField][EditorAttributes.ReadOnly] private Direction m_facingDirectionType = Direction.Forward;

    //bools
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isStandingStill = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isLockOn = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isRunning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isFreelyMoving = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isTurning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isAction = false;

    //Actions
    Action ResetTurningAction;

    //Coroutines
    private Coroutine m_turningCoroutine;

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private Vector3 m_prevFacingRotationDir = Vector3.forward;

    private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;

    private enum FacingDirectionTypeConstrains { Free, LockedByAction}
    [SerializeField] [EditorAttributes.ReadOnly] private FacingDirectionTypeConstrains m_actionDirectionConstrain = FacingDirectionTypeConstrains.Free;


    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public float MoveStrenght { get => m_moveStrength; set { if (m_isRunning && value > 0f)  m_moveStrength = m_runningMoveStrenght; else m_moveStrength = value; Speed = m_moveStrength; } } //is already snapped by inputmanager
    public float Speed { get => m_speed; set { m_speed = value == 0 ? 0 : value == 0.5 ? m_speedValues.x : value == 1 ? m_speedValues.y : m_speedValues.z; } } //is already snapped by inputmanager
    public Quaternion CameraYAxisRotation { get => m_cameraYAxisRotationInWS; set => m_cameraYAxisRotationInWS = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target { get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get => Target.position; }
    public Vector3 PlayerToTargetXZVector { get { if (m_target == null) { Debug.Log("No target, so no Direction to Target"); return transform.forward; }; return new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z); } }
    public bool IsRunning { get => m_isRunning; set { m_isRunning = value; MoveStrenght = m_playerInputManager.LeftStickSnappedMag; m_animator.SetBool("IsRunning", value); } }

    public AnimationInterruptableType CurrentInteruptability { get => m_currentInteruptability;  }


    void Start()
    {
        m_playerInputManager = PlayerInputManager.Instance;
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;

        m_turningStrenght = m_turningStrenghtBaseValue;
        m_maxTurningSpeed = m_maxTurningSpeedBaseValue;

        ResetTurningAction = () =>
        {
            m_turningStrenght = m_turningStrenghtBaseValue;
            m_maxTurningSpeed = m_maxTurningSpeedBaseValue;
            if (m_turningCoroutine != null) { StopCoroutine(m_turningCoroutine); m_turningCoroutine = null; }
            m_isTurning = false;
            m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
            m_animator.ResetTrigger("TriggerTurning");

        };
    }

    void Update()
    {
        SetValues();
        TriggerTurning();

        RotatingPlayer();

        MovingPlayer();

        SetAnimatorMoveValues();
    }

    private void SetValues() //moveDir, threshholds, TargetDist, etc
    {
        m_isFreelyMoving = m_actionDirectionConstrain == FacingDirectionTypeConstrains.Free ? !m_isLockOn || m_isRunning || m_isStandingStill : m_isFreelyMoving; //only change, when its not mid animation
        m_isStandingStill = m_moveStrength == 0;

        if (m_isLockOn) m_targetDist = (TargetPos - transform.position).magnitude;

        if (m_isStandingStill)
        {
            //m_facingDirectionType = Direction.Forward;
            return;
        }
        if (m_isFreelyMoving)
        {
            // InputDirRelativeToCam is relative to cameraRotation, so it should not affect the InputDirRelativeToCam when for example standing still
            m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : m_inputDirInWS; 

            m_facingDirectionType = Direction.Forward;
        }
        else
        {

            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector);
            Quaternion playerToTargetAndCameraForwardSlerp = Quaternion.Slerp(m_cameraYAxisRotationInWS, playerToTargetLookRotation, 0.5f);
            m_inputDirInWS = playerToTargetAndCameraForwardSlerp * m_inputDir;

            if (m_actionDirectionConstrain == FacingDirectionTypeConstrains.Free) //if action started, the facingType should stay, since its needed for the SetAnimatorMoveValues()
                SetFacingDirectionType();
        }
    }

    void SetFacingDirectionType()
    {
        //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
        float firstThreshholdAngleMin = 15f;
        float secondThreshholdAngleMin = 110f;
        float distThreshhold = 10f;    //Ab diesem abstand werden die threshholds anfangen zu den Min Threshholds zu lerpen
        float additionalThreshhold = 5f; //Damit es keine flickerzone gibt, wo das überschreiten und unterschreiten des threshholds gleich ist

        if (m_facingDirectionType == Direction.Left || m_facingDirectionType == Direction.Right) additionalThreshhold = -additionalThreshhold;

        m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, m_targetDist / distThreshhold) + additionalThreshhold;
        m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, m_targetDist / distThreshhold) - additionalThreshhold;

        float inputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);

        if (inputAngleToForward < m_forwardSidewardThreshholdAngle)                                             m_facingDirectionType = Direction.Forward;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle && Mathf.Sign(m_inputDir.x) >= 0)      m_facingDirectionType = Direction.Right;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle)                                       m_facingDirectionType = Direction.Left;
        else                                                                                                    m_facingDirectionType = Direction.Backward; 


    }



    private void SetAnimatorMoveValues()
    {
        float animationDampTime = m_actionDirectionConstrain == FacingDirectionTypeConstrains.Free ? 0.15f : 0; //smaller is faster transition
        float MoveStrength = m_moveStrength; //is already snapped in inputmanager
        m_animator.SetFloat("MoveMag", MoveStrength, animationDampTime, Time.deltaTime);

        if (m_isStandingStill)
            return;

        if (m_isFreelyMoving)
        {
            m_animator.SetFloat("Vertical", 1, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else                                                                                                  
        {
            Vector2 horAndVerMovement = new Vector2(0, 1);

            if (m_facingDirectionType == Direction.Right)           horAndVerMovement = new Vector2(1, 0);
            else if (m_facingDirectionType == Direction.Left)       horAndVerMovement = new Vector2(-1, 0);
            else if (m_facingDirectionType == Direction.Backward)       horAndVerMovement = new Vector2(0, -1);

            m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);
        }

    }







    [SerializeField] float turningStrenght = 2f;
    [SerializeField] float maxTurningSpeed = 10f;
    void TriggerTurning()
    {
        AnimationInterruptableType turningInterruptability = AnimationInterruptableType.Easily_Interruptable;

        if ((int)m_currentInteruptability >= (int)turningInterruptability)
            return;

        // if the input differs too much, its will trigger an turn. Therefore we need the current and pevious frame latestProcessedDir
        float angleMoveDirToPrevMoveDir = Vector3.Angle(m_desiredFacingRotationDirInWS, m_prevFacingRotationDir);

        if (m_isFreelyMoving && (!m_isRunning && angleMoveDirToPrevMoveDir > 90) || (m_isRunning && angleMoveDirToPrevMoveDir > 150))
        {
            m_currentInteruptability = turningInterruptability;
            m_turningStrenght = turningStrenght;
            m_maxTurningSpeed = maxTurningSpeed;

            m_animator.SetTrigger("TriggerTurning");
            m_isTurning = true;

            m_turningCoroutine = StartCoroutine(UtilityFunctions.Wait(0.45f, ResetTurningAction));
        }
    }


    public void TriggerEvading()
    {
        AnimationInterruptableType evadeInterruptability = AnimationInterruptableType.Not_Interruptable;

        if ((int)m_currentInteruptability >= (int)evadeInterruptability)
            return;
        
        if (m_characterMovesetData == null)
        {
            Debug.Log("MISSING Moveset DATA");
            return;
        }

        AnimationMovementData animData;
        if (m_facingDirectionType == Direction.Forward)         animData = m_characterMovesetData.evadeForward.AnimationMovementData;
        else if (m_facingDirectionType == Direction.Left)       animData = m_characterMovesetData.evadeLeft.AnimationMovementData;
        else if (m_facingDirectionType == Direction.Right)      animData = m_characterMovesetData.evadeRight.AnimationMovementData;
        else                                                    animData = m_characterMovesetData.evadeBackwards.AnimationMovementData;

        if (animData == null)
        {
            Debug.Log("MISSING ANIMATION DATA");
            return;
        }

        ResetTurningAction?.Invoke();

        m_currentInteruptability = evadeInterruptability;
        m_animator.SetTrigger("TriggerEvade");
        SetValues(); //needed, because what if it jumps from one action directly into another
        m_isAction = true;
        m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        //m_desiredFacingRotationDirInWS = m_inputDirInWS;


        float animationDuration = m_characterMovesetData.evadeForward.animationClip.length / m_characterMovesetData.evadeForward.animationSpeed;
        SetActionValues(animData, animationDuration);

    }












    private void MovingPlayer()
    {
        //less movement gets applied if the character is still not turned into moveDir, but full movement is applied when facing in movement direction //not sure if this is a nice solution
        float forwardFactor = m_isTurning ? UtilityFunctions.RefitRange(Vector3.Angle(transform.forward, m_inputDirInWS), 30, 20, 0, 1) : 1f;

        //direction
        Vector3 directionByInput = !m_isFreelyMoving ? m_inputDirInWS : transform.forward;
        Vector3 directionByAction = m_directionByAction;
        Vector3 nowMoveDirection = Vector3.Lerp(directionByInput, directionByAction, m_actionInfluenceOverMoveDirection);

        //speed
        float speedByInput = m_inputFactor * m_speed * forwardFactor;
        float speedByAction = m_speedByAction;
        float nowSpeed = Mathf.Lerp(speedByInput, speedByAction, m_actionInfluenceOverMoveSpeed);

        //acceleration
        float moveAccelerationByInput = m_moveAcceleration;
        float moveAccelerationByAction = m_moveAccelerationByAction;
        float nowMoveAcceleration = Mathf.Lerp(moveAccelerationByInput, moveAccelerationByAction, m_actionInfluenceOverMoveAcceleration);


        Vector3 nowMove =  UtilityFunctions.SmartLerp(m_prevMove, nowMoveDirection * nowSpeed, Time.deltaTime * nowMoveAcceleration);
        m_characterController.Move(nowMove * Time.deltaTime);
        m_prevMove = nowMove;
        
    }


    Quaternion AdditionalFacingRotation()
    {
        if (m_facingDirectionType == Direction.Forward) return Quaternion.identity;
        if (m_facingDirectionType == Direction.Right) return Quaternion.Euler(0, -90, 0);
        else if (m_facingDirectionType == Direction.Left) return Quaternion.Euler(0, 90, 0);
        else return Quaternion.Euler(0, 180, 0);
    }


    private void RotatingPlayer()
    {
        m_prevFacingRotationDir = m_desiredFacingRotationDirInWS; //hmm, vielleicht von dem nehmen: nowdesiredFacingRotationDirInWS

        Quaternion additionalTurningRotation =  AdditionalFacingRotation();

        //if no input, then it should not recalculate the desired facing direction, because what if i stand still and then lock on something behind me, it should not affect any calculation as long as i dont move
        // also, actions like evading set their initial m_desiredFacingRotationDirInWS in their own Trigger function
        m_desiredFacingRotationDirInWS = (!m_isStandingStill) ? additionalTurningRotation * m_inputDirInWS : m_desiredFacingRotationDirInWS;

        //direction
        Vector3 desiredFacingRotationDirInWSByInput = m_desiredFacingRotationDirInWS; 
        Vector3 desiredFacingRotationDirInWSByAction = m_turningFollowsTarget && m_isLockOn ? PlayerToTargetXZVector : m_desiredFacingRotationDirInWSByAction;
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput, desiredFacingRotationDirInWSByAction, m_actionInfluenceOverDesiredFacingRotationDirInWS);
        //Debug.Log(m_desiredFacingRotationDirInWSByAction);
        //Speed
        float turningStrenghtByInput = m_turningStrenght;
        float turningStrenghtByAction = m_turningStrenghtByAction;
        float nowTurningStrenght = Mathf.Lerp(turningStrenghtByInput, turningStrenghtByAction, m_actionInfluenceOverTurningStrenght);

        //acceleration
        float maxTurningSpeedByInput = m_maxTurningSpeed;
        float maxTurningSpeedByAction = m_maxTurningSpeedByInputByAction;
        float nowMaxTurningSpeed = Mathf.Lerp(maxTurningSpeedByInput, maxTurningSpeedByAction, m_actionInfluenceOverMaxTurningSpeed) * 60 * Time.deltaTime; //60 als faktor, damit maxspeed nicht so groß sein muss

        float angle = Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up); 
        float newAngle = Mathf.Clamp(Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up) * Time.deltaTime * nowTurningStrenght, -nowMaxTurningSpeed, nowMaxTurningSpeed); //Only ever 90° steps max per seconds, the turning speed
        if (newAngle != 0) m_animator.SetFloat("TurningDir", Mathf.Sign(newAngle));

        //this makes the car rotate not around it center when walking and turning, but rotates around a pont slightly to the side
        float turnRotationPointOffsetXAxis = Mathf.Sign(newAngle) * m_prevMove.magnitude / 1.8f;
        Vector3 rotationCenterOffset = new Vector3(turnRotationPointOffsetXAxis, 0, 0);

        transform.RotateAround(transform.position + (transform.rotation * rotationCenterOffset), Vector3.up, newAngle);


    }
















    #region

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

    private bool m_turningFollowsTarget = false;

    Coroutine m_ActionCoroutine = null;

    #endregion


    private void SetActionValues(AnimationMovementData animData, float animationDuration)
    {

        int moveDirPredefinition = (int)animData.moveDirPredefinition;
        int turningDirPredefinition = (int)animData.turningDirPredefinition;
        float startMoveInfluence = animData.moveInfluence  == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;  
        float startTurningInfluence = animData.turningInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;

        m_turningFollowsTarget = animData.turningRelations == AnimationMovementData.InitialRelations.TurningDirFollowsTarget;

        //move
        m_directionByAction = m_directionByActionBaseValue = (moveDirPredefinition == 1) ? m_inputDirInWS : transform.forward;
        m_actionInfluenceOverMoveDirection = startMoveInfluence;
        m_speedByAction = 0; // is set to 0
        m_actionInfluenceOverMoveSpeed = startMoveInfluence;
        m_moveAccelerationByAction = m_moveAcceleration; // is set to current acc
        m_actionInfluenceOverMoveAcceleration = startMoveInfluence;

        //turning
        m_desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByActionBaseValue = (turningDirPredefinition == 1) ? m_desiredFacingRotationDirInWS : transform.forward;
        m_actionInfluenceOverDesiredFacingRotationDirInWS = startTurningInfluence;
        m_turningStrenghtByAction = m_turningStrenght; // is set to current speed 
        m_actionInfluenceOverTurningStrenght = startTurningInfluence;
        m_maxTurningSpeedByInputByAction = m_maxTurningSpeed; // is set to current acc
        m_actionInfluenceOverMaxTurningSpeed = startTurningInfluence;

        List<ProcessedAnimationMovementData.DataCurves> CurveValuesList = new List<ProcessedAnimationMovementData.DataCurves>(); 
        List<ProcessedAnimationMovementData.DataStartEnd> RangeValuesList = new List<ProcessedAnimationMovementData.DataStartEnd>(); 


        foreach (var value in animData.variableValue)
        {
            if (value.ignore)
                continue;
            AnimationMovementData.Values.Settings valueData = value.settings;

            bool valueTypeIsConstant = valueData.valueType == AnimationMovementData.ValueType.ConstantValue;
            bool valueTypeIsStartEnd = valueData.valueType == AnimationMovementData.ValueType.StartEndValue;

            bool influenceValueTypeIsConstant = valueData.influenceType == AnimationMovementData.InfluenceValueType.ConstantInfluence;
            bool influenceValueTypeIsStartEnd = valueData.influenceType == AnimationMovementData.InfluenceValueType.StartEndInfluence;

            switch (value.valueName)
            {
                case AnimationMovementData.ValueName.Move_Direction_Angle:
                    if (valueTypeIsConstant)                m_directionByAction = Quaternion.Euler(0, valueData.value, 0) * m_directionByActionBaseValue; 
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));
                    
                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverMoveDirection = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if ((int)animData.turningRelations == 3)   { m_desiredFacingRotationDirInWSByAction = m_directionByAction; /*m_actionInfluenceOverDesiredFacingRotationDirInWS = 1;*/ }

                    break;
                case AnimationMovementData.ValueName.Move_Speed:
                    if (valueTypeIsConstant)            m_speedByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverMoveSpeed = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Move_Acceleration:
                    if (valueTypeIsConstant)            m_moveAccelerationByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverMoveAcceleration = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Turning_Direction_Angle:

                    if (valueTypeIsConstant)                m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, valueData.value, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverDesiredFacingRotationDirInWS = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    //if ((int)animData.turningRelations == 4)   { m_directionByAction = transform.forward; /*m_actionInfluenceOverDesiredFacingRotationDirInWS = 1;*/ }

                    break;
                case AnimationMovementData.ValueName.Turning_Strenght:

                    if (valueTypeIsConstant)            m_turningStrenghtByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverTurningStrenght = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Max_Turning_Speed:

                    if (valueTypeIsConstant)            m_maxTurningSpeedByInputByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)   m_actionInfluenceOverMaxTurningSpeed = valueData.influence;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    break;

            }
        }

        ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, (int)animData.turningRelations, animData.timeStepsForCurves, animationDuration); //This could be saved somewhere in future!

        m_ActionCoroutine = StartCoroutine(PerformAction(processedData));

    }
        
    private IEnumerator PerformAction(ProcessedAnimationMovementData processedData)
    {
        float elapsedTime = 0;
        float startTime = Time.time;
        float timeToWait = processedData.timeSteps;


        float duration = processedData.animationDuration; //what about blendtrees?

        void SetValueByName(ProcessedAnimationMovementData.ValueName name, float newValue)
        {
            switch (name)
            {
                case ProcessedAnimationMovementData.ValueName.Move_Direction_Angle:                     m_directionByAction                                     = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue; 
                                                                        if (processedData.turningRelations == 3) m_desiredFacingRotationDirInWSByAction = m_directionByAction; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle:         m_actionInfluenceOverMoveDirection                      = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Speed:                               m_speedByAction                                         = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed:                   m_actionInfluenceOverMoveSpeed                          = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Acceleration:                        m_moveAccelerationByAction                              = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration:            m_actionInfluenceOverMoveAcceleration                   = newValue; break;
                
                case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:                  m_desiredFacingRotationDirInWSByAction                  = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                                                                        /*if (processedData.turningRelations == 4) m_directionByAction = transform.forward;*/ break; 
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle:      m_actionInfluenceOverDesiredFacingRotationDirInWS       = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Turning_Strenght:                         m_turningStrenghtByAction                               = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght:             m_actionInfluenceOverTurningStrenght                    = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Max_Turning_Speed:                        m_maxTurningSpeedByInputByAction                        = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed:            m_actionInfluenceOverMaxTurningSpeed                    = newValue; break;
            }
        }

        //Debug.Log($"time: {Time.time}, < start: {startTime} + elapsed: {elapsedTime}");

        while (elapsedTime <= duration)
        {
            float timeTillEnd = (duration - elapsedTime);
            float waitForTime = timeTillEnd; 
            float relativeTimeValue = elapsedTime / duration;

            //STARTEND VALUES
            foreach (var rangeData in processedData.rangeValuesList)
            {
                
                float activeFactor = relativeTimeValue >= rangeData.startEnd.x && relativeTimeValue < rangeData.startEnd.y ? 1 : 0;
                float valueInRange = rangeData.value * activeFactor;

                //this calculates how long to wait for the next necessary canculation
                float waitForTimeByRangeValues = timeTillEnd;
                if(relativeTimeValue < rangeData.startEnd.x)                waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime; //wait till range start
                else if (relativeTimeValue < rangeData.startEnd.y)          waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime; //wait till range end
                                                                        waitForTime = Math.Min(waitForTime, waitForTimeByRangeValues);
                SetValueByName(rangeData.name, valueInRange);
            }

            //CURVE VALUES
            foreach (var curveData in processedData.curveValuesList)
            {
                AnimationCurve curve = curveData.curve;

                float activeFactor = relativeTimeValue > curveData.startEnd.x && relativeTimeValue < curveData.startEnd.y ? 1 : 0;
                float curveValue = curveData.value * curve.Evaluate(Mathf.InverseLerp(curveData.startEnd.x, curveData.startEnd.y, relativeTimeValue)) * activeFactor;

                //this calculates how long to wait for the next necessary canculation
                float waitForTimeByCurveValues = timeToWait;
                if (relativeTimeValue < curveData.startEnd.x)               waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.x * duration) - elapsedTime); //wait till range start or timeToWait
                else if (relativeTimeValue < curveData.startEnd.y)          waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.y * duration) - elapsedTime); //wait till range end or timeToWait                                                                        //wait till timeToWait
                                                                        waitForTime = Mathf.Min(waitForTime, waitForTimeByCurveValues);                                                                                                   


                SetValueByName(curveData.name, curveValue);
            }

            //End of Frame
            //Debug.Log($" relativeTime: { relativeTimeValue}");
            if (elapsedTime > duration - 0.001f)
                yield return null;
            else
                yield return new WaitForSeconds(waitForTime);

            elapsedTime = Time.time - startTime; // time must be added after the first wait
        }


        //End of Action

        //reset Values
        m_actionInfluenceOverMoveDirection = 0;
        m_actionInfluenceOverMoveSpeed = 0;
        m_actionInfluenceOverMoveAcceleration = 0;
        m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;
        m_actionInfluenceOverTurningStrenght = 0;
        m_actionInfluenceOverMaxTurningSpeed = 0;
        m_turningFollowsTarget = false;
        m_actionDirectionConstrain = FacingDirectionTypeConstrains.Free;
        m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
        m_ActionCoroutine = null;
        m_isAction = false;
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;

        //Set Values
        m_prevFacingRotationDir = transform.forward;
        m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward; 
        m_desiredFacingRotationDirInWS = !m_isStandingStill ? m_inputDirInWS : transform.forward;



        m_playerInputManager.RecallLatestBufferedInput();
    }









}
