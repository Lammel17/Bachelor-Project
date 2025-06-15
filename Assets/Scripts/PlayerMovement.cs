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
using Unity.Hierarchy;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    public GameObject testMoveDirection;
    public GameObject testTurningDirection;



    private CharacterController m_characterController;
    private PlayerCameraHolder m_playerCameraHolder; 
    private PlayerInputManager m_playerInputManager;
    [SerializeField] private Animator m_animator;
    private LookAt m_lookAtScript = null;
    [SerializeField] private CharacterMovesetData m_characterMovesetData;
    private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;

    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    private float m_inputFactor = 1f; //should stay 1
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private Vector3 m_turningStrenghtBaseValues = new Vector3(15, 15, 10); //slow, walk, running
    [SerializeField] private float m_maxTurningSpeedBaseValue = 50f;
    //private const int m_runningMoveStrenght = 2;
    private Vector3 m_nowMoveDir = Vector3.forward;

    private float m_inputStrenght = 0f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_inputDirInWS = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWS = Vector3.forward;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningAngle = 0;
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
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isAdditionalRotationForbidden = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isStandingStill = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isLockOn = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isHoldRunning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isRunning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isFreelyMoving = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isTurning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isAction = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isWalkingLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isActionLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isHoldShielding = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isShielding = false;

    [SerializeField][EditorAttributes.ReadOnly] private int m_currentAnimation;
    [SerializeField][EditorAttributes.ReadOnly] private int m_currentUpperBodyAnimation;

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private float m_prevInputStrength = 0;
    private float m_prevPrevInputStrength = 0;
    bool m_isStandingPrev = true;


    private Coroutine m_actionChangesInterruptabilityCoroutine;
    private Coroutine m_ActionCoroutine = null;

    private NextPossibleWeaponActions m_nextPossibleWeaponActions = null;
    private NextPossibleShieldActions m_nextPossibleShieldActions = null;
    public class NextPossibleWeaponActions
    {
        public WeaponData.WeaponAttack light;
        public WeaponData.WeaponAttack heavy;
        public WeaponData.WeaponAttack specialLight;
        public WeaponData.WeaponAttack specialHeavy;

        public NextPossibleWeaponActions(WeaponData.WeaponAttack l, WeaponData.WeaponAttack h, WeaponData.WeaponAttack sl, WeaponData.WeaponAttack sh)
        {
            light = l;
            heavy = h;
            specialLight = sl; 
            specialHeavy = sh;
        }

    }
    public class NextPossibleShieldActions
    {
        public ShieldData.ShieldAction shieldIdle;
        public ShieldData.ShieldAction ShieldingUpperBody;
        public ShieldData.ShieldAction special12;
        public ShieldData.ShieldAction special34;

        public NextPossibleShieldActions(ShieldData.ShieldAction i, ShieldData.ShieldAction s, ShieldData.ShieldAction s12, ShieldData.ShieldAction s34)
        {
            shieldIdle = i;
            ShieldingUpperBody = s;
            special12 = s12;
            special34 = s34;
        }
    }

    //PROPERTIES
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public float InputStrenght //is already snapped by inputmanager
    { 
        get => m_inputStrenght; 
        set { m_inputStrenght = value; Speed = m_inputStrenght; } 
    } 
    public float Speed //is already snapped by inputmanager
    { 
        get => m_speed; 
        set 
        { 
            if          (value == 0)        { m_speed = 0;                  m_turningStrenght = m_turningStrenghtBaseValues.x; }
            else if     (m_isHoldRunning)   { m_speed = m_speedValues.z;    m_turningStrenght = m_turningStrenghtBaseValues.z; }
            else if     (value == 0.5f)     { m_speed = m_speedValues.x;    m_turningStrenght = m_turningStrenghtBaseValues.x; }
            else /*if   (value == 1) */     { m_speed = m_speedValues.y;    m_turningStrenght = m_turningStrenghtBaseValues.y; }
        } 
    } 
    public Quaternion CameraYAxisRotation { get => m_cameraYAxisRotationInWS; set => m_cameraYAxisRotationInWS = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target 
    { 
        get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } 
        set { m_target = value; m_isLockOn = (m_target != null); if (!m_isAction) SetLookAt(m_target); } 
    }
    public Vector3 TargetPos { get => Target.position; }
    public Vector3 PlayerToTargetXZVector 
    { 
        get { if (m_target == null) { Debug.Log("No target, so no Direction to Target"); return transform.forward; }; return new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z).normalized; } 
    }
    public bool IsHoldRunning { get => m_isHoldRunning; set { m_isHoldRunning = value; Speed = m_inputStrenght; } }
    public bool IsHoldShielding { get => m_isHoldShielding; set { m_isHoldShielding = value; } }
    public Vector3 PreviousMove { get => m_prevMove; }

    public AnimationInterruptableType CurrentInteruptability { get => m_currentInteruptability;  }




    void Start()
    {
        m_playerInputManager = PlayerInputManager.Instance;
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
        if (TryGetComponent<LookAt>(out LookAt lookAt))
            m_lookAtScript = lookAt;

        m_turningStrenght = m_turningStrenghtBaseValues[1];
        m_maxTurningSpeed = m_maxTurningSpeedBaseValue;

        ChangeMoveset.SetWeapon(m_characterMovesetData.weapon, m_characterMovesetData);
        ChangeMoveset.SetShield(m_characterMovesetData.shield, m_characterMovesetData);
        ChangeAnimation.InitializeAnimationOverrideController(m_animator, m_characterMovesetData);

        m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.LightAttack1, m_characterMovesetData.weapon.HeavyAttack1, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);
        m_nextPossibleShieldActions = new NextPossibleShieldActions(m_characterMovesetData.shield.shieldIdle, m_characterMovesetData.shield.shieldingUpperBody, m_characterMovesetData.shield.ShiledSpecial1, m_characterMovesetData.shield.ShiledSpecial3);

        m_currentAnimation = Idle_1;
        m_currentUpperBodyAnimation = Empty_UpperBody;

    }










    void Update()
    {
        SetValues();
        TriggerTurning();

        testMoveDirection.transform.localScale = new Vector3(0.07f, 0.07f, Mathf.Min(Mathf.Max(m_inputStrenght, 0.2f), 1.5f));

        if ((int)m_actionTurningRelations == 1/*TurningDirFollowsMoveDir*/)
        {
            MovingPlayer();
            RotatingPlayer();
        }
        else
        {
            RotatingPlayer();
            MovingPlayer();
        }


        SetAnimatorMoveValues();
        CheckAnimation();

        m_prevPrevInputStrength = m_prevInputStrength; 
        m_prevInputStrength = m_inputStrenght;


        testMoveDirection.transform.position = new Vector3(transform.position.x, testMoveDirection.transform.position.y, transform.position.z);
        testTurningDirection.transform.position = new Vector3(transform.position.x, testTurningDirection.transform.position.y, transform.position.z);


    }


    private Coroutine SwitchFreelyMoving;
    bool m_isAboutSwitchDirectionType = false;

    private void SetValues() //moveDir, threshholds, TargetDist, etc
    {
        //StandingStill
        m_isStandingPrev = m_isStandingStill;
        m_isStandingStill = ((m_isWalkingLocked == false && m_inputStrenght == 0) || m_isWalkingLocked == true);

        //Running
        if (m_isRunning != (m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked))
        {
            m_isRunning = !m_isRunning;
            if (m_isRunning)        { SetNextPossibleAttacks(currentAction: Running); SetLookAt(null); }
            else if (!m_isAction)   { SetNextPossibleAttacks(currentAction: Reset); SetLookAt(m_target); }
        }
        
        //FreelyMoving
        bool prevFreelyMoving = m_isFreelyMoving;
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
        if (prevFreelyMoving != m_isFreelyMoving) //Only for one thing, that when locked on and then start running, that the movement is for 0.2s set to input instead of forward
        { 
            m_isAboutSwitchDirectionType = true; 
            SwitchFreelyMoving = StartCoroutine(UtilityFunctions.Wait(0.2f, () => { m_isAboutSwitchDirectionType = false; /*Debug.Log*/ })); 
        }

        //TargetDist
        if (m_isLockOn) 
            m_targetDist = (TargetPos - transform.position).magnitude;

        //VALUES
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
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector, Vector3.up);
            Quaternion playerToTargetAndCameraForwardSlerp = Quaternion.Slerp(m_cameraYAxisRotationInWS, playerToTargetLookRotation, 0.5f);
            m_inputDirInWS = playerToTargetAndCameraForwardSlerp * m_inputDir;

            if (!m_isAction) //if action started, the facingType should stay, since its needed for the SetAnimatorMoveValues()
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








    void TriggerTurning()
    {
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType turningInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)turningInterruptability) return;    

        AnimationData animData = null;


        // if the input differs too much, its will trigger an turn. Therefore we need the current and pevious frame latestProcessedDir
        float angleMoveDirToPrevMoveDir = m_turningAngle;

        if (!m_isRunning && !m_isLockOn && m_isFreelyMoving && (m_prevInputStrength == 0 || m_prevPrevInputStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 90)
        {
            if ( Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)     animData = m_characterMovesetData.turningLeft;
            else                                                animData = m_characterMovesetData.turningRight;   
            SetTriggerTurning();
        }
        if (m_isRunning && m_isFreelyMoving &&  (m_prevInputStrength == 0 || m_prevPrevInputStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 150)
        {
            if (Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)      animData = m_characterMovesetData.turningRunningLeft;
            else                                                animData = m_characterMovesetData.turningRunningRight;
            SetTriggerTurning();
        }

        void SetTriggerTurning()
        {
            if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

            ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin
        
            m_isTurning = true;
            
            if (m_ActionCoroutine != null)
                EndActionReset();

            m_animator.SetFloat("TurningDir", Mathf.Sign(angleMoveDirToPrevMoveDir));
            m_currentInteruptability = turningInterruptability;

            InitAction(!m_isRunning ? Turning : Turning_Running, animData);
        }
    }


    public void TriggerEvading()
    {
        if (m_isActionLocked) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType evadeInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)evadeInterruptability) return;

        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_facingDirectionType = Direction.Forward; //just a reminder

        AnimationData animData;
        int animHash = 0;
        if (m_facingDirectionType == Direction.Forward)             { animData = m_characterMovesetData.evadeForward; animHash = Evade_Forward; }
        else if (m_facingDirectionType == Direction.Left)           { animData = m_characterMovesetData.evadeLeft; animHash = Evade_Left; }
        else if (m_facingDirectionType == Direction.Right)          { animData = m_characterMovesetData.evadeRight; animHash = Evade_Right; }
        else                                                        { animData = m_characterMovesetData.evadeBackwards; animHash = Evade_Backwards; }

        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();
        
        SetNextPossibleAttacks(currentAction: animHash);

        m_currentInteruptability = evadeInterruptability;

        InitAction(animHash, animData);

        }


    public void TriggerLightAttack()
    {
        if (m_isActionLocked) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        WeaponData.WeaponAttack thisAttack = m_nextPossibleWeaponActions.light; 
        if (thisAttack == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return;}
        if (thisAttack.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return; }

        AnimationInterruptableType lightAttackInterruptability = thisAttack.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAttack.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)lightAttackInterruptability) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

        SetNextPossibleAttacks(thisAttack);

        m_currentInteruptability = lightAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        InitAction(thisAttack.AttackHash, thisAttack.AnimData);
    }

    public void TriggerHeavyAttack()
    {
        if (m_isActionLocked) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        WeaponData.WeaponAttack thisAttack = m_nextPossibleWeaponActions.heavy;
        if (thisAttack == null) { Debug.Log("MISSING ATTACK DATA of a Heavy Attack"); return; }
        if (thisAttack.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Heavy Attack"); return; }

        AnimationInterruptableType heavyAttackInterruptability = thisAttack.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAttack.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)heavyAttackInterruptability) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

        SetNextPossibleAttacks(thisAttack);

        m_currentInteruptability = heavyAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        InitAction(thisAttack.AttackHash, thisAttack.AnimData);

    }

    public void TriggerShielding(bool isHoldShielding)
    {
        IsHoldShielding = isHoldShielding;

        if (!m_isHoldShielding)
        {
            m_isShielding = false;
            return;
        }

        AnimationInterruptableType shieldingInterruptabilityLimit = AnimationInterruptableType.Hardly_Interruptable;
        if ((int)m_currentInteruptability >= (int)shieldingInterruptabilityLimit) return;

        if (m_ActionCoroutine != null && !m_isTurning) //stop current animation if its not those: turning
            EndActionReset();

        m_isShielding = true;

        CheckAnimation();

    }








    //Coroutine m_resetNextPossibleActionsCoroutine;
    private void InitAction(int animationHash, AnimationData animData)
    {
        m_isAction = true;

        if (!m_isTurning) //stop upperbody animations
        {
            m_isShielding = false;
            SetUpperBodyAnimation(Empty_UpperBody, crossFadeDuration: 0.1f);
        }


        SetLookAt(null);

        SetAnimation(animationHash, true, animData.crossfadeInTime); //this sets and activates the animation with given crossfadeInTime
        m_nextCrossfadeOutTime = animData.crossfadeOutTime; //this is set and stored for end of action for the case the animation fades out normally and is not interrupted by an action with its own fadeInTime

        SetValues(); //needed, because what if it jumps from one action directly into another

        float animationDuration = animData.animationClip.length;

        Action changeInteruptabilityAction = () =>
        {
            m_currentInteruptability = animData.ChangedInterruptability;
            m_actionChangesInterruptabilityCoroutine = null;

            if (m_playerInputManager.CheckRecallLatestBufferedInput())
                EndActionReset();
            else if (m_isHoldShielding)
            {
                EndActionReset();
                m_isShielding = true;
            }
        };
        
        //this is if a action is earlier interruptable than the lenght of the animation
        m_actionChangesInterruptabilityCoroutine = StartCoroutine(UtilityFunctions.Wait(animationDuration - animData.crossfadeBeginn - animData.InterruptabilityChangeBeforeEndTime, changeInteruptabilityAction));

        SetActionValues(animData.AnimationMovementData, animationDuration, animData.crossfadeOutTime, animData.crossfadeBeginn);
    }

    private void SetNextPossibleAttacks(WeaponData.WeaponAttack currentAttackData = null, int currentAction = 0)
    {
        if (currentAction == Evade_Forward || currentAction == Evade_Left || currentAction == Evade_Right || currentAction == Evade_Backwards)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.EvadeLightAttack, m_characterMovesetData.weapon.EvadeHeavyAttack, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == Running)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.SprintLightAttack, m_characterMovesetData.weapon.SprintHeavyAttack, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == Reset)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.LightAttack1, m_characterMovesetData.weapon.HeavyAttack1, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);

        else if (currentAttackData != null)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(GetNextAttackLight(currentAttackData.nextLight), GetNextAttackHeavy(currentAttackData.nextHeavy), GetNextAttackSpecialLight(currentAttackData.nextSpecialLight), GetNextAttackSpecialHeavy(currentAttackData.nextSpecialHeavy));


        WeaponData.WeaponAttack GetNextAttackLight(WeaponData.LightAttack light)
        {
            switch (light)
            {
                case WeaponData.LightAttack.Light_Attack_1: if (m_characterMovesetData.weapon.LightAttack1.AnimData != null) return m_characterMovesetData.weapon.LightAttack1; break;
                case WeaponData.LightAttack.Light_Attack_2: if (m_characterMovesetData.weapon.LightAttack2.AnimData != null) return m_characterMovesetData.weapon.LightAttack2; break;
                case WeaponData.LightAttack.Light_Attack_3: if (m_characterMovesetData.weapon.LightAttack3.AnimData != null) return m_characterMovesetData.weapon.LightAttack3; break;
                case WeaponData.LightAttack.Light_Attack_4: if (m_characterMovesetData.weapon.LightAttack4.AnimData != null) return m_characterMovesetData.weapon.LightAttack4; break;
                case WeaponData.LightAttack.Light_Attack_5: if (m_characterMovesetData.weapon.LightAttack5.AnimData != null) return m_characterMovesetData.weapon.LightAttack5; break;
                case WeaponData.LightAttack.Light_Attack_6: if (m_characterMovesetData.weapon.LightAttack6.AnimData != null) return m_characterMovesetData.weapon.LightAttack6; break;
                case WeaponData.LightAttack.Sprint_Light_Attack: if (m_characterMovesetData.weapon.SprintLightAttack.AnimData != null) return m_characterMovesetData.weapon.SprintLightAttack; break;
                case WeaponData.LightAttack.Evade_Light_Attack: if (m_characterMovesetData.weapon.EvadeLightAttack.AnimData != null) return m_characterMovesetData.weapon.EvadeLightAttack; break;
            }
            return m_characterMovesetData.weapon.LightAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackHeavy(WeaponData.HeavyAttack heavy)
        {
            switch (heavy)
            {
                case WeaponData.HeavyAttack.Heavy_Attack_1: if (m_characterMovesetData.weapon.HeavyAttack1.AnimData != null) return m_characterMovesetData.weapon.HeavyAttack1; break;
                case WeaponData.HeavyAttack.Heavy_Attack_2: if (m_characterMovesetData.weapon.HeavyAttack2.AnimData != null) return m_characterMovesetData.weapon.HeavyAttack2; break;
                case WeaponData.HeavyAttack.Heavy_Attack_3: if (m_characterMovesetData.weapon.HeavyAttack3.AnimData != null) return m_characterMovesetData.weapon.HeavyAttack3; break;
                case WeaponData.HeavyAttack.Heavy_Attack_4: if (m_characterMovesetData.weapon.HeavyAttack4.AnimData != null) return m_characterMovesetData.weapon.HeavyAttack4; break;
                case WeaponData.HeavyAttack.Sprint_Heavy_Attack: if (m_characterMovesetData.weapon.SprintHeavyAttack.AnimData != null) return m_characterMovesetData.weapon.SprintHeavyAttack; break;
                case WeaponData.HeavyAttack.Evade_Heavy_Attack: if (m_characterMovesetData.weapon.EvadeHeavyAttack.AnimData != null) return m_characterMovesetData.weapon.EvadeHeavyAttack; break;
            }
            return m_characterMovesetData.weapon.HeavyAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialLight(WeaponData.LightAttackSpecial specialLight)
        {
            switch (specialLight)
            {
                case WeaponData.LightAttackSpecial.Special_Light_Attack_1: if (m_characterMovesetData.weapon.SpecialLightAttack1.AnimData != null) return m_characterMovesetData.weapon.SpecialLightAttack1; break;
                case WeaponData.LightAttackSpecial.Special_Light_Attack_2: if (m_characterMovesetData.weapon.SpecialLightAttack2.AnimData != null) return m_characterMovesetData.weapon.SpecialLightAttack2; break;
            }
            return m_characterMovesetData.weapon.SpecialLightAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialHeavy(WeaponData.HeavyAttackSpecial specialHeavy)
        {
            switch (specialHeavy)
            {
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_1: if (m_characterMovesetData.weapon.SpecialHeavyAttack1.AnimData != null) return m_characterMovesetData.weapon.SpecialHeavyAttack1; break;
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_2: if (m_characterMovesetData.weapon.SpecialHeavyAttack2.AnimData != null) return m_characterMovesetData.weapon.SpecialHeavyAttack2; break;
            }
            return m_characterMovesetData.weapon.SpecialHeavyAttack1;
        }


    }
    public int GetInterruptabilityLight()
    {
        if (m_nextPossibleWeaponActions.light.AnimData == null || m_nextPossibleWeaponActions.light.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleWeaponActions.light.AnimData.CustomInterruptability;
    }
    public int GetInterruptabilityHeavy()
    {
        if (m_nextPossibleWeaponActions.heavy.AnimData == null || m_nextPossibleWeaponActions.heavy.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleWeaponActions.heavy.AnimData.CustomInterruptability;
    }
    public int GetInterruptabilityLightSpecial()
    {
        if (m_nextPossibleWeaponActions.specialLight.AnimData == null || m_nextPossibleWeaponActions.specialLight.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleWeaponActions.specialLight.AnimData.CustomInterruptability;
    }
    public int GetInterruptabilityHeavySpecial()
    {
        if (m_nextPossibleWeaponActions.specialHeavy.AnimData == null || m_nextPossibleWeaponActions.specialHeavy.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleWeaponActions.specialHeavy.AnimData.CustomInterruptability;
    }















    private void SetLookAt(Transform transform)
    {
        if (m_lookAtScript != null)
            m_lookAtScript.SetTarget(transform);

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
        //if no input, then it should not recalculate the desired facing direction, because what if i stand still and then lock on something behind me, it should not affect any calculation as long as i dont move
        // also, actions like evading set their initial m_desiredFacingRotationDirInWS in their own Trigger function
        if (!m_isStandingStill)
            m_desiredFacingRotationDirInWS = AdditionalFacingRotation() * m_inputDirInWS;
        else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && !m_isRunning)
            m_desiredFacingRotationDirInWS = PlayerToTargetXZVector;
        else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && m_isRunning)
            m_desiredFacingRotationDirInWS = m_desiredFacingRotationDirInWS;


        //FacingDir
        Vector3 desiredFacingRotationDirInWSByInput = m_desiredFacingRotationDirInWS;
        Vector3 desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 1/*TurningDirFollowsTarget*/) desiredFacingRotationDirInWSByAction = Quaternion.Euler(0,Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up),0) * PlayerToTargetXZVector;
        else if ((int)m_actionTurningRelations == 1/*TurningDirFollowsMoveDir*/)        desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction, Vector3.up), 0) * m_nowMoveDir;
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput.normalized, desiredFacingRotationDirInWSByAction.normalized, m_actionInfluenceOverDesiredFacingRotationDirInWS);

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

        testTurningDirection.transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, nowdesiredFacingRotationDirInWS, Vector3.up),0);

    }

    private void MovingPlayer()
    {
        //direction
        Vector3 directionByInput = (!m_isFreelyMoving || m_isAboutSwitchDirectionType) ? m_inputDirInWS : transform.forward;
        Vector3 directionByAction = m_directionByAction;
        if (m_isLockOn && (int)m_actionTargetRelations == 2/*MoveDirFollowsTarget*/)    directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * PlayerToTargetXZVector;
        else if ((int)m_actionTurningRelations == 2/*MoveDirFollowsTurningDir*/)        directionByAction = Quaternion.Euler(0, Vector3.SignedAngle(m_directionByActionBaseValue, m_directionByAction, Vector3.up), 0) * transform.forward;
        Vector3 nowMoveDirection = Vector3.Lerp(directionByInput.normalized, directionByAction.normalized, m_actionInfluenceOverMoveDirection);
        if (nowMoveDirection != Vector3.zero) m_nowMoveDir = nowMoveDirection.normalized;

        //speed
        float speedByInput = (!m_isWalkingLocked) ? m_inputFactor * m_speed : 0;
        float speedByAction = m_speedByAction;
        float nowSpeed = Mathf.Lerp(speedByInput, speedByAction, m_actionInfluenceOverMoveSpeed);

        //acceleration
        float moveAccelerationByInput = m_moveAcceleration;
        float moveAccelerationByAction = m_moveAccelerationByAction;
        float nowMoveAcceleration = Mathf.Lerp(moveAccelerationByInput, moveAccelerationByAction, m_actionInfluenceOverMoveAcceleration);


        Vector3 nowMove =  UtilityFunctions.SmartLerp(m_prevMove, m_nowMoveDir * nowSpeed, Time.deltaTime * nowMoveAcceleration);
        m_characterController.Move(nowMove * Time.deltaTime);
        m_prevMove = nowMove;


        if (nowMove != Vector3.zero) testMoveDirection.transform.rotation = Quaternion.LookRotation(nowMove, Vector3.up);
        else testMoveDirection.transform.localScale = new Vector3(0.5f, 0.07f, Mathf.Min(Mathf.Max(nowSpeed/2, 0.2f), 1.5f));


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

    private AnimationMovementData.TargetRelations m_actionTargetRelations = 0;
    private AnimationMovementData.TurningRelations m_actionTurningRelations = 0;

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

        m_actionTargetRelations = animData.targetRelations;
        m_actionTurningRelations = animData.turningRelations;
        m_isAdditionalRotationForbidden = animData.forbidAdditinalRotation;


        //initial moveDir
        if ((int)m_actionTurningRelations == 2 /*MoveDirFollowsTurningDir*/ || (m_isLockOn && (int)m_actionTargetRelations == 2 /*MoveDirFollowsTarget*/)) 
        {
            m_directionByAction = /*Quaternion.Inverse(AdditionalFacingRotation()) **/ Vector3.forward;
            m_directionByActionBaseValue = Vector3.forward;
        } 
        else
        {
            Vector3 moveDir = Vector3.zero;
            if (moveDirPredefinition == 1 /*LatestInput*/)      moveDir = m_inputDirInWS;
            if (moveDirPredefinition == 2 /*LatestFrame)*/)     moveDir = transform.forward;
            m_directionByAction = m_directionByActionBaseValue = moveDir;
        }
        m_actionInfluenceOverMoveDirection = startMoveInfluence;
        m_speedByAction = 0; // is set to 0
        m_actionInfluenceOverMoveSpeed = startMoveInfluence;
        m_moveAccelerationByAction = m_moveAcceleration; // is set to current acc
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
            if (turningDirPredefinition == 1 /*latestInputWithAddTurning*/) turningDir = /*AdditionalFacingRotation() **/ m_inputDirInWS;
            if (turningDirPredefinition == 2 /*latestFrame)*/)              turningDir = transform.forward;
            m_desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByActionBaseValue = turningDir;
        }
        m_actionInfluenceOverDesiredFacingRotationDirInWS = startTurningInfluence;
        m_turningStrenghtByAction = m_turningStrenght; // is set to current strenght
        m_actionInfluenceOverTurningStrenght = startTurningInfluence;
        m_maxTurningSpeedByInputByAction = m_maxTurningSpeed; // is set to current maxspeed
        m_actionInfluenceOverMaxTurningSpeed = startTurningInfluence;



        //testTurningDirection.transform.localRotation = Quaternion.LookRotation(m_desiredFacingRotationDirInWSByActionBaseValue, Vector3.up);



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

            //bool beginnsWithoutInfluence = (influenceValueTypeIsConstant && influenceData.influence == 0);

            switch (value.valueName)
            {
             //MOVING
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

             //TURNING
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

        ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, (int)m_actionTurningRelations, animData.timeStepsForCurves, animationDuration, crossfadeOutTime, crossfadeStartBeforeEndTime); //This could be saved somewhere in future!

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
                    //if (processedData.turningRelations == 1 /*TurningDirFollowsMoveDir*/) 
                    //{ //the facingdirBaseValue must be updated, and then the turning also needs to be recalculated
                    //    m_desiredFacingRotationDirInWSByActionBaseValue = m_directionByAction; 

                    //    Quaternion presumableTurningValueOffsetData = Quaternion.FromToRotation(m_desiredFacingRotationDirInWSByActionBaseValue, m_desiredFacingRotationDirInWSByAction);
                    //    m_desiredFacingRotationDirInWSByAction = presumableTurningValueOffsetData * m_desiredFacingRotationDirInWSByActionBaseValue;
                    //}
                    m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
                    break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle:         m_actionInfluenceOverMoveDirection                      = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Speed:                               m_speedByAction                                         = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed:                   m_actionInfluenceOverMoveSpeed                          = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Acceleration:                        m_moveAccelerationByAction                              = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration:            m_actionInfluenceOverMoveAcceleration                   = newValue; break;
                
                case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:
                    //Debug.Log(processedData.turningRelations);
                    //if (processedData.turningRelations == 2 /*MoveDirFollowsTurningDir*/)
                    //{ //the movedirBase value must be updated and then applied
                    //    Debug.Log("EEEEEEEEEEEEEEEEEEEEEEEEEE");
                    //    m_directionByActionBaseValue = m_desiredFacingRotationDirInWSByAction;
                    //    Quaternion presumableMoveDirValueOffsetData = Quaternion.FromToRotation(m_directionByActionBaseValue, m_directionByAction);
                    //    m_directionByAction = presumableMoveDirValueOffsetData * m_directionByActionBaseValue;
                    //}
                    m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    break; 
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
        bool isRunning = m_isHoldRunning && m_inputStrenght != 0 && !m_isWalkingLocked;
        if (isRunning) SetNextPossibleAttacks(currentAction: Running);
        else SetNextPossibleAttacks(currentAction: Reset);

        EndActionReset();

        //HERE NOTHING MORE

    }

    private void EndActionReset()
    {
        //reset Values
        m_actionInfluenceOverMoveDirection = 0;
        m_actionInfluenceOverMoveSpeed = 0;
        m_actionInfluenceOverMoveAcceleration = 0;
        m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;
        m_actionInfluenceOverTurningStrenght = 0;
        m_actionInfluenceOverMaxTurningSpeed = 0;
        m_isAdditionalRotationForbidden = false;
        m_actionTargetRelations = 0;
        m_actionTurningRelations = 0;
        m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
        m_isAction = false;
        m_isRunning = m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked;
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
        m_isTurning = false;
        if (m_isHoldShielding) m_isShielding = true;

        //End Coroutines
        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }
        if (m_actionChangesInterruptabilityCoroutine != null) 
        {
            StopCoroutine(m_actionChangesInterruptabilityCoroutine);
            m_actionChangesInterruptabilityCoroutine = null;
        }

        //Set Values
        m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_facingDirectionType = Direction.Forward; //just a reminder, in the past here was a issue, but it should not anymore
        m_desiredFacingRotationDirInWS = !m_isStandingStill ? AdditionalFacingRotation() * m_inputDirInWS : transform.forward;
        if (m_isLockOn && !m_isRunning) SetLookAt(m_target);

        m_playerInputManager.RecallLatestBufferedInput();
    }
























    private void SetAnimatorMoveValues()
    {
        float animationDampTime = !m_isAction ? 0.1f : 0; //smaller is faster transition
        float MoveStrength = m_isRunning ? 2 : m_inputStrenght; //is already snapped in inputmanager
        Vector2 horAndVerMovement = new Vector2(0, 1);

        m_animator.SetFloat("MoveMag", MoveStrength, animationDampTime, Time.deltaTime);

        if (m_facingDirectionType == Direction.Forward) horAndVerMovement = new Vector2(0, 1);
        else if (m_facingDirectionType == Direction.Right) horAndVerMovement = new Vector2(1, 0);
        else if (m_facingDirectionType == Direction.Left) horAndVerMovement = new Vector2(-1, 0);
        else horAndVerMovement = new Vector2(0, -1);

        m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);
        m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);

    }

    readonly int Shielding_TorsoStabilizing = Animator.StringToHash("Shielding_TorsoStabilizing");
    readonly int Empty_TorsoStabilizer = Animator.StringToHash("Empty_TorsoStabilizer");

    readonly int Shielding_UpperBody        = Animator.StringToHash("Shielding_UpperBody");
    readonly int Empty_UpperBody            = Animator.StringToHash("Empty_UpperBody");

    readonly int Running                    = Animator.StringToHash("Running");
    readonly int Reset                      = Animator.StringToHash("Reset");

    //animation States
    #region
    readonly int Idle_1                     = Animator.StringToHash("Idle_1");
    readonly int Shield_Idle                = Animator.StringToHash("Shield_Idle");

    readonly int Locomotion                 = Animator.StringToHash("Locomotion");
    readonly int Turning                    = Animator.StringToHash("Turning");
    readonly int Turning_Running            = Animator.StringToHash("Turning_Running");

    readonly int Evade_Forward              = Animator.StringToHash("Evade_Forward");
    readonly int Evade_Left                 = Animator.StringToHash("Evade_Left");
    readonly int Evade_Right                = Animator.StringToHash("Evade_Right");
    readonly int Evade_Backwards            = Animator.StringToHash("Evade_Backwards");

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
        {
            if (m_isShielding)
            {
                SetAnimation(Shield_Idle, false, crossFadeDuration: m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeInTime);
                m_nextCrossfadeOutTime = m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeOutTime;
            }
            else
                SetAnimation(Idle_1, false, crossFadeDuration);
        }
        if (!m_isStandingStill)
        {
            SetAnimation(Locomotion, false, crossFadeDuration, 0.25f);
        }


        //UpperBody
        if (m_isShielding)
        {
            SetUpperBodyAnimation(Shielding_UpperBody, crossFadeDuration: m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeInTime); ///////// get this from a place where i save shield anims
            m_nextUpperBodyCrossfadeOutTime = m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeOutTime;
        }
        else
            SetUpperBodyAnimation(Empty_UpperBody); 

    }





    private float m_baseCrossFadeDuration = 0.15f;
    private float m_nextCrossfadeOutTime = -1f; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim
    private float m_nextUpperBodyCrossfadeOutTime = -1f; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim

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


    private void SetUpperBodyAnimation(int upperBodyAnimation, bool calledByAction = false, float crossFadeDuration = 0.1f, float timeOffset = 0)
    {
        if (!calledByAction && m_currentUpperBodyAnimation == upperBodyAnimation)
            return;

        if (m_nextUpperBodyCrossfadeOutTime >= 0)
        {
            crossFadeDuration = m_nextUpperBodyCrossfadeOutTime;
            m_nextUpperBodyCrossfadeOutTime = -1;
        }

        m_animator.CrossFade(upperBodyAnimation, crossFadeDuration, 1, timeOffset);
        m_currentUpperBodyAnimation = upperBodyAnimation;

        if (upperBodyAnimation == Shielding_UpperBody) m_animator.CrossFade(Shielding_TorsoStabilizing, crossFadeDuration, 2, timeOffset);
        else if (upperBodyAnimation == Empty_UpperBody) m_animator.CrossFade(Empty_TorsoStabilizer, crossFadeDuration, 2, timeOffset);

    }











}
