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
    [SerializeField] private Animator m_animator;
    [SerializeField] private CharacterMovesetData m_characterMovesetData;
    private WeaponData.WeaponActionCount m_currentWeaponMoveCount = new WeaponData.WeaponActionCount();
    private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;

    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    private float m_inputFactor = 1f; //should stay 1
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private Vector3 m_turningStrenghtBaseValues = new Vector3(15, 15, 10); //slow, walk, running
    [SerializeField] private float m_maxTurningSpeedBaseValue = 50f;
    private const int m_runningMoveStrenght = 2;

    private float m_moveStrength = 0f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_inputDirInWS = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWS = Vector3.forward;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningAngle = 0;
    private float m_turningStrenght;
    private float m_maxTurningSpeed;
    private float m_speed = 0; //slow, walk, running
    bool m_isAdditionalRotationForbidden = false;

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
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isAction = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isWalkingLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isActionLocked = false;

    [SerializeField][EditorAttributes.ReadOnly] private int m_currentAnimation;

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private float m_prevMoveStrength = 0;
    private float m_prevPrevMoveStrength = 0;
    private Vector3 m_prevFacingRotationDir = Vector3.forward; //unused currently


    private enum FacingDirectionTypeConstrains { Free, LockedByAction}
    [SerializeField] [EditorAttributes.ReadOnly] private FacingDirectionTypeConstrains m_actionDirectionConstrain = FacingDirectionTypeConstrains.Free;

    private NextPossibleActions m_nextPossibleActions = new NextPossibleActions();
    public class NextPossibleActions
    {
        public AnimationData light;
        public AnimationData heavy;
        public AnimationData specialLight;
        public AnimationData specialHeavy;
    }



    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public float MoveStrenght { get => m_moveStrength; set { m_prevPrevMoveStrength = m_prevMoveStrength; m_prevMoveStrength = m_moveStrength; if (m_isRunning && value > 0f)  m_moveStrength = m_runningMoveStrenght; else m_moveStrength = value; Speed = m_moveStrength; } } //is already snapped by inputmanager
    public float Speed 
    { 
        get => m_speed; 
        set 
        { 
            if          (value == 0)        { m_speed = 0;                  m_turningStrenght = 0; }
            else if     (value == 0.5f)     { m_speed = m_speedValues.x;    m_turningStrenght = m_turningStrenghtBaseValues.x; }
            else if     (value == 1)        { m_speed = m_speedValues.y;    m_turningStrenght = m_turningStrenghtBaseValues.y; }
            else                            { m_speed = m_speedValues.z;    m_turningStrenght = m_turningStrenghtBaseValues.z; }
        } 
    } //is already snapped by inputmanager
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

        m_turningStrenght = m_turningStrenghtBaseValues[1];
        m_maxTurningSpeed = m_maxTurningSpeedBaseValue;

        ChangeMoveset.SetWeapon(m_characterMovesetData.weapon, m_characterMovesetData);
        ChangeAnimation.InitializeAnimationOverrideController(m_animator, m_characterMovesetData);

        m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack1.AnimData;
        m_nextPossibleActions.heavy = m_characterMovesetData.weapon.HeavyAttack1.AnimData;
        m_nextPossibleActions.specialLight = m_characterMovesetData.weapon.SpecialLightAttack1.AnimData;
        m_nextPossibleActions.specialHeavy = m_characterMovesetData.weapon.SpecialHeavyAttack1.AnimData;

        m_currentAnimation = Idle_1;

    }

    void Update()
    {
        SetValues();
        TriggerTurning();

        RotatingPlayer();
        MovingPlayer();

        SetAnimatorMoveValues();
        CheckAnimation();
        
    }

    private Coroutine SwitchFreelyMoving;
    bool m_isAboutSwitchDirectionType = false;

    private void SetValues() //moveDir, threshholds, TargetDist, etc
    {
        m_isStandingStill = ((m_isWalkingLocked == false && m_moveStrength == 0) || m_isWalkingLocked == true);
        bool prevFreelyMoving = m_isFreelyMoving;
        m_isFreelyMoving = m_actionDirectionConstrain == FacingDirectionTypeConstrains.Free ? !m_isLockOn || m_isRunning || m_isStandingStill : m_isFreelyMoving; //only change, when its not mid animation
        if (prevFreelyMoving != m_isFreelyMoving) 
        { 
            m_isAboutSwitchDirectionType = true; 
            SwitchFreelyMoving = StartCoroutine(UtilityFunctions.Wait(0.2f, () => { m_isAboutSwitchDirectionType = false; /*Debug.Log*/ })); 
        }

        if (m_isLockOn) 
            m_targetDist = (TargetPos - transform.position).magnitude;

        if (m_isFreelyMoving)
        {
            // InputDirRelativeToCam is relative to cameraRotation, so it should not affect the InputDirRelativeToCam when for example standing still
            m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward; 
            m_facingDirectionType = Direction.Forward;
            return;
        }
        else
        {
            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector);
            Quaternion playerToTargetAndCameraForwardSlerp = Quaternion.Slerp(m_cameraYAxisRotationInWS, playerToTargetLookRotation, 0.5f);
            m_inputDirInWS = playerToTargetAndCameraForwardSlerp * m_inputDir;

            if (m_actionDirectionConstrain == FacingDirectionTypeConstrains.Free) //if action started, the facingType should stay, since its needed for the SetAnimatorMoveValues()
                SetFacingDirectionType();
            return;
        }
    }

    void SetFacingDirectionType()
    {
        //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
        float firstThreshholdAngleMin = 10f;
        float secondThreshholdAngleMin = 110f;
        float distThreshhold = 10f;    //Ab diesem abstand werden die threshholds anfangen zu den Min Threshholds zu lerpen
        float additionalThreshhold = 5f; //Damit es keine flickerzone gibt, wo das überschreiten und unterschreiten des threshholds gleich ist

        if (m_facingDirectionType == Direction.Left || m_facingDirectionType == Direction.Right) additionalThreshhold = -additionalThreshhold;

        m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, m_targetDist / distThreshhold) + additionalThreshhold; /////////////////Mathf.Min
        m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, m_targetDist / distThreshhold) - additionalThreshhold;

        float inputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);

        if (inputAngleToForward < m_forwardSidewardThreshholdAngle)                                             m_facingDirectionType = Direction.Forward;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle && Mathf.Sign(m_inputDir.x) >= 0)      m_facingDirectionType = Direction.Right;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle)                                       m_facingDirectionType = Direction.Left;
        else                                                                                                    m_facingDirectionType = Direction.Backward; 


    }



    private void SetAnimatorMoveValues()
    {
        float animationDampTime = !m_isAction ? 0.1f : 0; //smaller is faster transition
        float MoveStrength = m_moveStrength; //is already snapped in inputmanager
        Vector2 horAndVerMovement = new Vector2(0, 1);
        
        m_animator.SetFloat("MoveMag", MoveStrength, animationDampTime, Time.deltaTime);

        if (m_facingDirectionType == Direction.Forward)             horAndVerMovement = new Vector2(0, 1);
        else if (m_facingDirectionType == Direction.Right)          horAndVerMovement = new Vector2(1, 0);
        else if (m_facingDirectionType == Direction.Left)           horAndVerMovement = new Vector2(-1, 0);
        else                                                        horAndVerMovement = new Vector2(0, -1);

        m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);    
        m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);

    }








    void TriggerTurning()
    {
        AnimationInterruptableType turningInterruptability = AnimationInterruptableType.Easily_Interruptable;

        if ((int)m_currentInteruptability >= (int)turningInterruptability)
            return;

        // if the input differs too much, its will trigger an turn. Therefore we need the current and pevious frame latestProcessedDir
        float angleMoveDirToPrevMoveDir = m_turningAngle;
        if (m_isFreelyMoving && ((!m_isRunning && (m_prevMoveStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 90) || (m_isRunning && (m_prevMoveStrength == 0 || m_prevPrevMoveStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 150)))
        {
            //Debug.Log(angleMoveDirToPrevMoveDir);
            AnimationData animData = null;
            if      (!m_isRunning && Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)                         animData = m_characterMovesetData.turningLeft;
            else if (!m_isRunning && Mathf.Sign(angleMoveDirToPrevMoveDir) >= 0)                        animData = m_characterMovesetData.turningRight;
            else if (Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)                                         animData = m_characterMovesetData.turningRunningLeft;
            else                                                                                        animData = m_characterMovesetData.turningRunningRight;

            if (animData == null)
            {
                Debug.Log("MISSING ANIMATION DATA");
                return;
            }

            ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin


            m_animator.SetFloat("TurningDir", Mathf.Sign(angleMoveDirToPrevMoveDir));
            m_currentInteruptability = turningInterruptability;
            m_isAction = true;
            //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction; //?????????????

            SetAnimation(!m_isRunning ? Turning : Turning_Running, true, animData.crossfadeInTime);
            m_nextCrossfadeOutTime = animData.crossfadeOutTime;
            SetValues(); //needed, because what if it jumps from one action directly into another, then some new values would be skipped without this

            float animationDuration = animData.animationClip.length;
            SetActionValues(animData.AnimationMovementData, animationDuration, animData.crossfadeOutTime, animData.crossfadeBeginn);

        }
    }


    public void TriggerEvading()
    {
        AnimationInterruptableType evadeInterruptability = AnimationInterruptableType.Not_Interruptable;

        if (m_isActionLocked)
            return;

        if ((int)m_currentInteruptability >= (int)evadeInterruptability)
            return;
        
        if (m_characterMovesetData == null)
        {
            Debug.Log("MISSING Moveset DATA");
            return;
        }

        AnimationData animData;
        if (m_facingDirectionType == Direction.Forward)         animData = m_characterMovesetData.evadeForward;
        else if (m_facingDirectionType == Direction.Left)       animData = m_characterMovesetData.evadeLeft;
        else if (m_facingDirectionType == Direction.Right)      animData = m_characterMovesetData.evadeRight;
        else                                                    animData = m_characterMovesetData.evadeBackwards;

        if (animData == null)
        {
            Debug.Log("MISSING ANIMATION DATA");
            return;
        }

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        m_nextPossibleActions.light = m_characterMovesetData.weapon.EvadeLightAttack.AnimData; //////////

        m_currentInteruptability = evadeInterruptability;
        m_isAction = true;
        m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        SetAnimation(Evade, true, animData.crossfadeInTime);
        m_nextCrossfadeOutTime = animData.crossfadeOutTime;
        SetValues(); //needed, because what if it jumps from one action directly into another

        float animationDuration = animData.animationClip.length;
        SetActionValues(animData.AnimationMovementData, animationDuration, animData.crossfadeOutTime, animData.crossfadeBeginn);

    }


    public void TriggerLightAttack()
    {
        AnimationInterruptableType lightAttackInterruptability = AnimationInterruptableType.Not_Interruptable;

        if (m_isActionLocked)
            return;

        if ((int)m_currentInteruptability >= (int)lightAttackInterruptability)
            return;

        if (m_characterMovesetData == null)
        {
            Debug.Log("MISSING Moveset DATA");
            return;
        }

        AnimationData animData = m_nextPossibleActions.light; 

        if (animData == null)
        {
            Debug.Log("MISSING ANIMATION DATA");
            return;
        }

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        int whichLightAttack = 0;
        switch (m_currentWeaponMoveCount.LightAttacks)
        {
            case 0: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack1.AnimData; whichLightAttack = Light_Attack_1; break;
            case 1: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack2.AnimData; whichLightAttack = Light_Attack_2; break;
            case 2: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack3.AnimData; whichLightAttack = Light_Attack_3; break;
            case 3: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack4.AnimData; whichLightAttack = Light_Attack_4; break;
            case 4: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack5.AnimData; whichLightAttack = Light_Attack_5; break;
            case 5: m_nextPossibleActions.light = m_characterMovesetData.weapon.LightAttack6.AnimData; whichLightAttack = Light_Attack_6; break;
        }
        m_currentWeaponMoveCount.LightAttacks = (m_currentWeaponMoveCount.LightAttacks + 1) % m_characterMovesetData.weapon.weaponActionCount.LightAttacks ; ///// Kinda many xx.xx.x.x.

        m_currentInteruptability = lightAttackInterruptability;
        m_isAction = true;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        SetAnimation(whichLightAttack, true, animData.crossfadeInTime);
        m_nextCrossfadeOutTime = animData.crossfadeOutTime;
        SetValues(); //needed, because what if it jumps from one action directly into another

        float animationDuration = animData.animationClip.length;
        SetActionValues(animData.AnimationMovementData, animationDuration, animData.crossfadeOutTime, animData.crossfadeBeginn);

    }












    private void MovingPlayer()
    {
        //direction
        Vector3 directionByInput = !m_isFreelyMoving || m_isAboutSwitchDirectionType ? m_inputDirInWS : transform.forward;
        Vector3 directionByAction = m_directionByAction;
        Vector3 nowMoveDirection = Vector3.Lerp(directionByInput, directionByAction, m_actionInfluenceOverMoveDirection);

        //speed
        float speedByInput = (!m_isWalkingLocked) ? m_inputFactor * m_speed : 0;
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


    private Quaternion AdditionalFacingRotation()
    {
        if (m_isAdditionalRotationForbidden)
            return Quaternion.identity;

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
        Vector3 desiredFacingRotationDirInWSByAction = (m_isTurningFollowsTarget && m_isLockOn) ? Vector3.Lerp(PlayerToTargetXZVector, m_desiredFacingRotationDirInWSByAction, 0f) : m_desiredFacingRotationDirInWSByAction; //Maybe in future something for the lerp factor
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput, desiredFacingRotationDirInWSByAction, m_actionInfluenceOverDesiredFacingRotationDirInWS);

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
        m_turningAngle = angle;


        //this makes the car rotate not around it center when walking and turning, but rotates around a pont slightly to the side
        //float turnRotationPointOffsetXAxis = !m_isAction && !m_isIgnoreTurningOffset ? (Mathf.Sign(newAngle) * m_prevMove.magnitude / 1.8f) : 0;
        //Vector3 rotationCenterOffset = new Vector3(turnRotationPointOffsetXAxis, 0, 0);

        //RotateAround() isnt actually working when using Move()
        transform.RotateAround(transform.position/* + (transform.rotation * rotationCenterOffset)*/, Vector3.up, newAngle);


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

    private bool m_isTurningFollowsTarget = false;

    Coroutine m_ActionCoroutine = null;

    #endregion


    private void SetActionValues(AnimationMovementData animData, float animationDuration, float crossfadeOutTime, float crossfadeStartBeforeEndTime = 0.1f)
    {
        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        List<ProcessedAnimationMovementData.DataCurves> CurveValuesList = new List<ProcessedAnimationMovementData.DataCurves>(); 
        List<ProcessedAnimationMovementData.DataStartEnd> RangeValuesList = new List<ProcessedAnimationMovementData.DataStartEnd>(); 

        if (animData == null)
        {
            Debug.Log("ANIMATION_DATA IS NULL");
            ProcessedAnimationMovementData emptyProcessedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, 0, 0.01f, animationDuration, crossfadeOutTime, crossfadeStartBeforeEndTime); //This could be saved somewhere in future!
            m_ActionCoroutine = StartCoroutine(PerformAction(emptyProcessedData));
            return;
        }


        int moveDirPredefinition = (int)animData.moveDirPredefinition;
        int turningDirPredefinition = (int)animData.turningDirPredefinition;
        float startMoveInfluence = animData.moveInfluence  == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;  
        float startTurningInfluence = animData.turningInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;

        m_isTurningFollowsTarget = animData.turningRelations == AnimationMovementData.TurningRelations.TurningDirFollowsTarget;
        m_isAdditionalRotationForbidden = animData.forbidAdditinalRotation;


        //move
        m_directionByAction = m_directionByActionBaseValue = (moveDirPredefinition == 1) ? m_inputDirInWS : transform.forward;
        m_actionInfluenceOverMoveDirection = startMoveInfluence;
        m_speedByAction = 0; // is set to 0
        m_actionInfluenceOverMoveSpeed = startMoveInfluence;
        m_moveAccelerationByAction = m_moveAcceleration; // is set to current acc
        m_actionInfluenceOverMoveAcceleration = startMoveInfluence;

        //turning
        Vector3 turningDir = transform.forward;
        if (turningDirPredefinition == 1) turningDir = (((int)animData.turningRelations != 2) ? Quaternion.Inverse(AdditionalFacingRotation()) * m_desiredFacingRotationDirInWS : m_inputDirInWS);
        else if (turningDirPredefinition == 2) turningDir =  (((int)animData.turningRelations != 2) ? m_desiredFacingRotationDirInWS : m_inputDirInWS);
        //else if (turningDirPredefinition == 3) turningDir = transform.forward;
        m_desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByActionBaseValue = turningDir;
        m_actionInfluenceOverDesiredFacingRotationDirInWS = startTurningInfluence;
        m_turningStrenghtByAction = m_turningStrenght; // is set to current strenght
        m_actionInfluenceOverTurningStrenght = startTurningInfluence;
        m_maxTurningSpeedByInputByAction = m_maxTurningSpeed; // is set to current maxspeed
        m_actionInfluenceOverMaxTurningSpeed = startTurningInfluence;



        foreach (var value in animData.variableValue)
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
                case AnimationMovementData.ValueName.Move_Direction_Angle:
                    if (valueTypeIsConstant)                m_directionByAction = Quaternion.Euler(0, valueData.value, 0) * m_directionByActionBaseValue; 
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));
                    
                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveDirection = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Move_Speed:
                    if (valueTypeIsConstant)            m_speedByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveSpeed = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Move_Acceleration:
                    if (valueTypeIsConstant)            m_moveAccelerationByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveAcceleration = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Turning_Direction_Angle:

                    if (valueTypeIsConstant)                m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, valueData.value, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverDesiredFacingRotationDirInWS = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd)); 
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Turning_Strenght:

                    if (valueTypeIsConstant)            m_turningStrenghtByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverTurningStrenght = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;
                case AnimationMovementData.ValueName.Max_Turning_Speed:

                    if (valueTypeIsConstant)            m_maxTurningSpeedByInputByAction = valueData.value;
                    else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd));
                    else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

                    if (influenceValueTypeIsConstant)           m_actionInfluenceOverMaxTurningSpeed = influenceData.influence;
                    else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
                    else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

                    break;

            }
        }

        ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, (int)animData.turningRelations, animData.timeStepsForCurves, animationDuration, crossfadeOutTime, crossfadeStartBeforeEndTime); //This could be saved somewhere in future!

        m_ActionCoroutine = StartCoroutine(PerformAction(processedData));

    }
        
    private IEnumerator PerformAction(ProcessedAnimationMovementData processedData)
    {
        float elapsedTime = 0;
        float startTime = Time.time;
        float timeToWait = processedData.timeSteps;

        float duration = processedData.animationDuration; //what about blendtrees, do they affect it?

        void SetValueByName(ProcessedAnimationMovementData.ValueName name, float newValue)
        {
            switch (name)
            {
                case ProcessedAnimationMovementData.ValueName.Move_Direction_Angle:                     
                    if (processedData.turningRelations == 2) 
                    { //the facingdirBase value must be updated, and then the turning also needs to be recalculated
                        m_desiredFacingRotationDirInWSByActionBaseValue = m_directionByAction; 
                        Quaternion presumableTurningValueOfData = Quaternion.FromToRotation(m_directionByAction, m_desiredFacingRotationDirInWSByAction);
                        m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
                        m_desiredFacingRotationDirInWSByAction = presumableTurningValueOfData * m_desiredFacingRotationDirInWSByActionBaseValue;
                    }
                    else 
                        m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
                    break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle:         m_actionInfluenceOverMoveDirection                      = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Speed:                               m_speedByAction                                         = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed:                   m_actionInfluenceOverMoveSpeed                          = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Acceleration:                        m_moveAccelerationByAction                              = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration:            m_actionInfluenceOverMoveAcceleration                   = newValue; break;
                
                case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:                  m_desiredFacingRotationDirInWSByAction                  = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue; break; 
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle:      m_actionInfluenceOverDesiredFacingRotationDirInWS       = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Turning_Strenght:                         m_turningStrenghtByAction                               = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght:             m_actionInfluenceOverTurningStrenght                    = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Max_Turning_Speed:                        m_maxTurningSpeedByInputByAction                        = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed:            m_actionInfluenceOverMaxTurningSpeed                    = newValue; break;
            }
        }


        while (elapsedTime <= duration - processedData.crossfadeStartBeforeEndTime)
        {
            float timeTillEnd = ((duration - processedData.crossfadeStartBeforeEndTime) - elapsedTime);
            float waitForTime = timeTillEnd; 
            float relativeElapsedTime = elapsedTime / duration;

            //STARTEND VALUES
            foreach (var rangeData in processedData.rangeValuesList)
            {
                
                float activeFactor = relativeElapsedTime >= rangeData.startEnd.x && relativeElapsedTime < rangeData.startEnd.y ? 1 : 0;
                float valueInRange = rangeData.value * activeFactor;

                //this calculates how long to wait for the next necessary canculation
                float waitForTimeByRangeValues = timeTillEnd;
                if(relativeElapsedTime < rangeData.startEnd.x)                waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime; //wait till range start
                else if (relativeElapsedTime < rangeData.startEnd.y)          waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime; //wait till range end
                                                                        waitForTime = Math.Min(waitForTime, waitForTimeByRangeValues);
                SetValueByName(rangeData.name, valueInRange);
            }

            //CURVE VALUES
            foreach (var curveData in processedData.curveValuesList)
            {
                AnimationCurve curve = curveData.curve;

                float activeFactor = relativeElapsedTime > curveData.startEnd.x && relativeElapsedTime < curveData.startEnd.y ? 1 : 0;
                float curveValue = curveData.value * curve.Evaluate(Mathf.InverseLerp(curveData.startEnd.x, curveData.startEnd.y, relativeElapsedTime)) * activeFactor;

                //this calculates how long to wait for the next necessary canculation
                float waitForTimeByCurveValues = timeToWait;
                if (relativeElapsedTime < curveData.startEnd.x)               waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.x * duration) - elapsedTime); //wait till range start or timeToWait
                else if (relativeElapsedTime < curveData.startEnd.y)          waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.y * duration) - elapsedTime); //wait till range end or timeToWait                                                                        //wait till timeToWait
                                                                        waitForTime = Mathf.Min(waitForTime, waitForTimeByCurveValues);                                                                                                   


                SetValueByName(curveData.name, curveValue);
            }

            //End of Frame
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
        m_isAdditionalRotationForbidden = false;
        m_isTurningFollowsTarget = false;
        m_actionDirectionConstrain = FacingDirectionTypeConstrains.Free;
        m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
        m_ActionCoroutine = null;
        m_isAction = false;
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;

        //Set Values
        m_prevFacingRotationDir = transform.forward;
        m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_facingDirectionType = Direction.Forward; //just a reminder, in the past here was a issue, but it should not anymore
        m_desiredFacingRotationDirInWS = !m_isStandingStill ? AdditionalFacingRotation() * m_inputDirInWS : transform.forward;

        //CheckAnimation(true);
        m_playerInputManager.RecallLatestBufferedInput();

    }









    //animation States
    #region
    readonly int Idle_1                     = Animator.StringToHash("Idle_1");
    readonly int Shield_Idle                = Animator.StringToHash("Shield_Idle");

    readonly int Locomotion                 = Animator.StringToHash("Locomotion");
    readonly int Turning                    = Animator.StringToHash("Turning");
    readonly int Turning_Running            = Animator.StringToHash("Turning_Running");
    readonly int Evade                      = Animator.StringToHash("Evade");

    readonly int Use_Item                   = Animator.StringToHash("Use_Item");
    readonly int Healing                    = Animator.StringToHash("Healing");
    readonly int Environment_Interaction    = Animator.StringToHash("Environment_Interaction");
    readonly int Pick_Up_Item_Low           = Animator.StringToHash("Pick_Up_Item_Low");
    readonly int Pick_Up_Item_Up            = Animator.StringToHash("Pick_Up_Item_Up");

    readonly int Switch_Weapon              = Animator.StringToHash("Switch_Weapon");
    readonly int Switch_Shield              = Animator.StringToHash("Switch_Shield");
    readonly int Put_Away_Weapon            = Animator.StringToHash("Put_Away_Weapon");
    readonly int Take_Out_Weapon            = Animator.StringToHash("Take_Out_Weapon");

    readonly int Light_Attack_1             = Animator.StringToHash("Light_Attack_1");
    readonly int Light_Attack_2             = Animator.StringToHash("Light_Attack_2");
    readonly int Light_Attack_3             = Animator.StringToHash("Light_Attack_3");
    readonly int Light_Attack_4             = Animator.StringToHash("Light_Attack_4");
    readonly int Light_Attack_5             = Animator.StringToHash("Light_Attack_5");
    readonly int Light_Attack_6             = Animator.StringToHash("Light_Attack_6");
    readonly int Sprint_Light_Attack        = Animator.StringToHash("Sprint_Light_Attack");
    readonly int Evade_Light_Attack         = Animator.StringToHash("Evade_Light_Attack");
    readonly int Special_Light_Attack_1     = Animator.StringToHash("Special_Light_Attack_1");
    readonly int Special_Light_Attack_2     = Animator.StringToHash("Special_Light_Attack_2");

    readonly int Heavy_Attack_1             = Animator.StringToHash("Heavy_Attack_1");
    readonly int Heavy_Attack_2             = Animator.StringToHash("Heavy_Attack_2");
    readonly int Heavy_Attack_3             = Animator.StringToHash("Heavy_Attack_3");
    readonly int Heavy_Attack_4             = Animator.StringToHash("Heavy_Attack_4");
    readonly int Sprint_Heavy_Attack        = Animator.StringToHash("Sprint_Heavy_Attack");
    readonly int Evade_Heavy_Attack         = Animator.StringToHash("Evade_Heavy_Attack");
    readonly int Special_Heavy_Attack_1     = Animator.StringToHash("Special_Heavy_Attack_1");
    readonly int Special_Heavy_Attack_2     = Animator.StringToHash("Special_Heavy_Attack_2");

    readonly int Special_Shield_1           = Animator.StringToHash("Special_Shield_1");
    readonly int Special_Shield_2           = Animator.StringToHash("Special_Shield_2");
    readonly int Special_Shield_3           = Animator.StringToHash("Special_Shield_3");
    readonly int Special_Shield_4           = Animator.StringToHash("Special_Shield_4");

    readonly int Almost_Stance_Break        = Animator.StringToHash("Almost_Stance_Break");
    readonly int Stance_Break               = Animator.StringToHash("Stance_Break");
    readonly int Falling_Forward            = Animator.StringToHash("Falling_Forward");
    readonly int Standing_Up_Forward        = Animator.StringToHash("Standing_Up_Forward");
    readonly int Falling_Backward           = Animator.StringToHash("Falling_Backward");
    readonly int Standing_Up_Backward       = Animator.StringToHash("Standing_Up_Backward");

    readonly int Falling_Mid_Air            = Animator.StringToHash("Falling_Mid_Air");
    readonly int Landing                    = Animator.StringToHash("Landing");
    #endregion 


    private void CheckAnimation(bool forceNewAnim = false, float crossFadeDuration = -1)
    {
        if ((m_isAction))
            if (!forceNewAnim) return;

        if (m_isStandingStill)
            SetAnimation(Idle_1, false, crossFadeDuration);
        if (!m_isStandingStill)
            SetAnimation(Locomotion, false, crossFadeDuration, 0.25f);

    }





    private float m_baseCrossFadeDuration = 0.15f;
    private float m_nextCrossfadeOutTime = -1f; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim

    private void SetAnimation(int animation, bool calledByAction = false, float crossFadeDuration = -1, float timeOffset = 0)
    {
        if (!calledByAction && m_currentAnimation == animation)
            return;


        if (crossFadeDuration < 0)
        {
            if (m_nextCrossfadeOutTime >= 0)
            { 
                crossFadeDuration = m_nextCrossfadeOutTime; 
                m_nextCrossfadeOutTime = -1; 
            }
            else
                crossFadeDuration = m_baseCrossFadeDuration;
        }

        m_animator.CrossFade(animation, crossFadeDuration, 0, timeOffset);
        m_currentAnimation = animation;
        
    }












}
