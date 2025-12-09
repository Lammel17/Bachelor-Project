using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using TMPro;
using Unity.Collections;
using Unity.Hierarchy;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
//using static UnityEditor.Experimental.GraphView.GraphView;

//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using Debug = UnityEngine.Debug;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterStatus))]
[RequireComponent(typeof(ActionMovementHandler))]

public class CharacterActionAndMovementHandler : MonoBehaviour
{
    public GameObject testMoveDirection;
    public GameObject testTurningDirection;
    [Space]

    [SerializeField][Required] private CharacterController m_characterController;
    [SerializeField][Required] private ActionMovementHandler m_actionMovementHandler;
    [SerializeField][Required] private CharacterStatus m_characterStatus;
    [SerializeField][Required] private GameObject m_chraracterMesh;
    [SerializeField][Required] private Animator m_animator;
    [SerializeField] private LayerMask m_environmentLayer;
    private FootPlacing m_footPlacing;
    private PlayerCameraHolder m_playerCameraHolder; 
    private PlayerInputManager m_playerInputManager;
    private LookAt m_lookAtScript = null;
    private CharacterMovesetData m_movesetData;
    [SerializeField][EditorAttributes.ReadOnly] private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;

    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    [SerializeField][Required] private CharacterSettingsData m_characterSettingsData;
    [SerializeField][EditorAttributes.ReadOnly] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField][EditorAttributes.ReadOnly] private float m_moveAcceleration = 20f;
    [SerializeField][EditorAttributes.ReadOnly] private Vector3 m_maxTurningSpeedBaseValues = new Vector3(12, 12, 12); //slow, walk, running
    [SerializeField][EditorAttributes.ReadOnly] private float m_turningStrenghtBaseValue = 15f;
    //private const int m_runningMoveStrenght = 2;
    private Vector3 m_nowMoveDir = Vector3.forward;
    private float m_originalGravity = -9.81f;
    [SerializeField][EditorAttributes.ReadOnly] private float m_currentGravity = -0f;
    private float m_velocityThroughGravity;
    private Vector3 m_controllerVelocity;

    private float m_inputStrenght = 0f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_inputDirInWS = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWS = Vector3.forward;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningAngle = 0;
    [SerializeField][EditorAttributes.ReadOnly] private float m_maxTurningSpeed;
    [SerializeField][EditorAttributes.ReadOnly] private float m_turningStrenght;
    private float m_currentBaseSpeed = 0; //standing, slow, walk, running
    private float m_animationSpeed = 1; //slow, walk, running

    private float m_currentMoveSpeedReference = 0;
    private float m_currentMoveAccelerationReference = 0;
    private Vector3 m_playerToTargetXZVector = Vector3.forward;

    //Values Depending on Camera
    private Quaternion m_cameraYAxisRotationInWS = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private enum Orientation { Forward, Left, Right, Backward };
    [Space]
    [SerializeField][EditorAttributes.ReadOnly] private BodyPosition m_bodyPosition = BodyPosition.Standing;
    [SerializeField][EditorAttributes.ReadOnly] private Orientation m_orientation = Orientation.Forward;
    //bools
    [SerializeField][EditorAttributes.ReadOnly] private bool m_disableSidewardMovement = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isStandingStill = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isLockOn = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isHoldRunning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isRunning = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isFreelyMoving = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isTurningApplied = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isSlidingApplied = false;
    //[SerializeField][EditorAttributes.ReadOnly] private bool m_isAction = false;
    //[SerializeField][EditorAttributes.ReadOnly] private bool m_isActionUpperBody = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isActionLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isHoldShielding = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isShielding = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isGrounded = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isMidAirPause = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_equipmentIsReady = true;
    [Space]
    [SerializeField][EditorAttributes.ReadOnly] private Vector2 m_currentActionAndLayer;
    [SerializeField][EditorAttributes.ReadOnly] private int[] m_currentAnimationStates;
    [SerializeField][EditorAttributes.ReadOnly] private Vector2 c_emptyAction = new Vector2(AnimationTypes.Empty,0);

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private float m_prevInputStrength = 0;
    private float m_prevPrevInputStrength = 0;
    private bool m_isStandingPrev = true;

    private AnimationData m_currentActionAnimData = null;
    private WeaponData.WeaponAttack m_currentWeaponAttackData = null;
    private ShieldData.ShieldAction m_currentShieldActionData = null;


    //private Coroutine m_actionChangesInterruptabilityCoroutine;
    private Coroutine m_actionPayCostCouroutine;
    private Coroutine m_actionPauseCoroutine;
    private Coroutine m_gravityPauseCoroutine;
    //private Coroutine m_ActionCoroutine = null;
    private Coroutine m_slowDownAnimSpeedCoroutine = null;

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
        public ShieldData.SimpleShieldAction shieldIdle;
        public ShieldData.SimpleShieldAction ShieldingUpperBody;
        public ShieldData.ShieldAction specialShieldLight;
        public ShieldData.ShieldAction specialShieldHeavy;

        public NextPossibleShieldActions(ShieldData.SimpleShieldAction i, ShieldData.SimpleShieldAction s, ShieldData.ShieldAction s12, ShieldData.ShieldAction s34)
        {
            shieldIdle = i;
            ShieldingUpperBody = s;

            specialShieldLight = s12;
            specialShieldHeavy = s34;
        }
    }
    public enum BodyPosition
    {
        Standing,
        Laying_Forward,
        Laying_Backwards,
        Kneeling_Sitting,
        Levitating,
    }

    #region PROPERTIES
    public Animator Animator { get => m_animator; }
    public float AnimatorSpeed
    {
        get => m_animationSpeed;
        set
        {
            if (value == m_animationSpeed)
                return;
            m_animationSpeed = value;
            m_actionMovementHandler.AnimationSpeed = value;
        }
    }
    public CharacterMovesetData MovesetData { get => m_movesetData; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public float InputStrenght //is already snapped by inputmanager to either 0, 0.5 or 1
    { 
        get => m_inputStrenght; 
        set { m_inputStrenght = value; BaseSpeed = m_inputStrenght; } 
    } 
    public float BaseSpeed //is already snapped by inputmanager
    { 
        get => m_currentBaseSpeed; 
        set 
        { 
            if          (value == 0)        { m_currentBaseSpeed = 0;                  /*m_maxTurningSpeed = m_maxTurningSpeedBaseValues.x;*/ }
            else if     (m_isRunning)       { m_currentBaseSpeed = m_speedValues.z;    /*m_maxTurningSpeed = m_maxTurningSpeedBaseValues.z;*/ }
            else if     (value == 0.5f)     { m_currentBaseSpeed = m_speedValues.x;    /*m_maxTurningSpeed = m_maxTurningSpeedBaseValues.x;*/ }
            else /*if   (value == 1) */     { m_currentBaseSpeed = m_speedValues.y;    /*m_maxTurningSpeed = m_maxTurningSpeedBaseValues.y;*/ }
        } 
    } 
    public bool IsGrounded 
    { 
        get => m_isGrounded; 
        set 
        {
            if (value == m_isGrounded)
                return;

            if (m_footPlacing != null && m_isGrounded && value == false)
                m_footPlacing.SetWeightActive(false);
            else if (m_footPlacing != null && !m_isGrounded && value == true)
                m_footPlacing.SetWeightActive(true);

            m_isGrounded = value;
            if (m_isMidAirPause && m_isGrounded)
            {
                Debug.Log("Grounded");
                m_isMidAirPause = false;
                if (m_slowDownAnimSpeedCoroutine != null)
                {
                    StopCoroutine(m_slowDownAnimSpeedCoroutine);
                    m_slowDownAnimSpeedCoroutine = null;
                }
                AnimatorSpeed = 1;
                m_animator.speed = m_animationSpeed;
            }
        } 
    }
    public float Gravity 
    { 
        get => m_currentGravity; 
        set 
        { 
            if (m_currentGravity == value) 
                return;

            //if (m_footPlacing != null && m_currentGravity != 0 && value == 0) 
            //    m_footPlacing.SetWeightActive(false); 
            //else if (m_footPlacing != null && m_currentGravity == 0 && value != 0)
            //    m_footPlacing.SetWeightActive(true);

            m_currentGravity = value; 
        } 
    }
    public Quaternion CameraYAxisRotation { get => m_cameraYAxisRotationInWS; set => m_cameraYAxisRotationInWS = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target 
    { 
        get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } 
        set 
        { 
            m_target = value; 
            m_isLockOn = (m_target != null); 
            if (m_currentActionAndLayer == c_emptyAction || (m_currentActionAnimData != null && m_currentActionAnimData.actionUsesLookAtTargetData)) 
                SetBodyLookAtTarget(m_target);

        } 
    }
    public Vector3 TargetPos { get => Target.position; }
    public bool IsHoldRunning { get => m_isHoldRunning; set { m_isHoldRunning = value; } }
    public bool IsRunning
    {
        get => m_isRunning;
        set 
        {
            if (value == m_isRunning) 
                return;

            m_isRunning = value;
            if (m_isRunning) { SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Running); SetBodyLookAtTarget(null); }
            else if (!m_isRunning && m_currentActionAndLayer == c_emptyAction) 
            { 
                SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
                if (m_inputStrenght == 0 && m_prevMove.magnitude > m_speedValues.z - 0.5f)
                {
                    m_animator.CrossFadeInFixedTime(AnimationTypes.Running_Sliding, m_baseCrossFadeDuration);
                    m_isSlidingApplied = true;
                    Debug.Log("SLIDING");
                    Debug.Log("AYA Dont forget Bug");
                }
            } 
            if (!m_isRunning && m_currentActionAndLayer == c_emptyAction && m_target != null) { SetBodyLookAtTarget(m_target); }

            BaseSpeed = m_inputStrenght;

            if (m_isRunning) m_characterStatus.PauseEnergyRegenerationByAction();
            else m_characterStatus.ContinueEnergyRegenerationInTime();
        } 
    }
    public bool IsHoldShielding { get => m_isHoldShielding; set { m_isHoldShielding = value; } }
    public bool IsShielding 
    { 
        get => m_isShielding; 
        set 
        { 
            if (value == m_isShielding) return;

            m_isShielding = value;
            if (m_isShielding)
            {
                m_characterStatus.IsShielding(true);
                SetLookAtForward(m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.useLookAtForwardData, m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.lookAtData);

                SetUpperBodyAnimation(AnimationTypes.Shielding, (int)m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.bodyParts, crossFadeDuration: m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeInTime);
                m_nextUpperBodyCrossfadeOutTime = m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeOutTime;
            }
            else
            {
                m_characterStatus.IsShielding(false);
                SetLookAtForward(false);

                SetUpperBodyAnimation(AnimationTypes.Empty, 0, m_nextUpperBodyCrossfadeOutTime);
            }
        }
    }
    public Vector3 PreviousMove { get => m_prevMove; }
    public AnimationInterruptableType CurrentInteruptability { get => m_currentInteruptability;  }
    public bool EquipmentIsReady { get => m_equipmentIsReady; }

    #endregion

    private void OnEnable()
    {
        m_actionMovementHandler.OnEndAndResetAction += EndActionResetValues;
        m_actionMovementHandler.OnEndActionBeforeReset += ResetNextAttack;
    }

    private void OnDisable()
    {
        m_actionMovementHandler.OnEndAndResetAction -= EndActionResetValues;
        m_actionMovementHandler.OnEndActionBeforeReset -= ResetNextAttack;
    }


    void Start()
    {
        SetSettingsValues();
        if (m_characterController == null) m_characterController = GetComponent<CharacterController>();
        if (m_characterStatus == null) m_characterStatus = GetComponent<CharacterStatus>();
        m_movesetData = m_characterStatus.MovesetData;



        m_chraracterMesh.transform.position = new Vector3(0, -m_characterController.skinWidth, 0);

        m_playerInputManager = PlayerInputManager.Instance;
        m_playerCameraHolder = PlayerCameraHolder.Instance;

        if (TryGetComponent<LookAt>(out LookAt lookAt))
            m_lookAtScript = lookAt;
        if (TryGetComponent<FootPlacing>(out FootPlacing footPlace))
            m_footPlacing = footPlace;

        m_maxTurningSpeed = m_maxTurningSpeedBaseValues[0];
        m_turningStrenght = m_turningStrenghtBaseValue;

        SetNextPossibleWeaponActions();
        SetNextPossibleShieldActions();

        m_currentAnimationStates = new int[m_animator.layerCount];
        for(int i = 1; i < m_currentAnimationStates.Length; i++) { m_currentAnimationStates[i] = AnimationTypes.Empty; }
        m_currentAnimationStates[0] = AnimationTypes.Idle_1;
        m_currentActionAndLayer = c_emptyAction;

        m_currentGravity = m_originalGravity;
        m_nextCrossfadeOutTime = m_baseCrossFadeDuration;
    }

    private void SetSettingsValues(CharacterSettingsData charBehaviorData = null)
    {
        if (charBehaviorData != null)
            m_characterSettingsData = charBehaviorData;

        m_speedValues = m_characterSettingsData.SpeedValues;
        m_moveAcceleration = m_characterSettingsData.MoveAcceleration;
        m_turningStrenghtBaseValue = m_characterSettingsData.TurningStrenghtBaseValues;
        m_maxTurningSpeedBaseValues = m_characterSettingsData.MaxTurningSpeedBaseValue;
        m_originalGravity = m_characterSettingsData.Gravity;
    }

    public void SetNextPossibleWeaponActions() 
    {
        if (m_movesetData == null || m_movesetData.weapon == null)
            return;
        m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_movesetData.weapon.LightAttack1, m_movesetData.weapon.HeavyAttack1, m_movesetData.weapon.SpecialLightAttack1, m_movesetData.weapon.SpecialHeavyAttack1);
    }
    public void SetNextPossibleShieldActions()
    {
        if (m_movesetData == null || m_movesetData.shield == null)
            return;
        m_nextPossibleShieldActions = new NextPossibleShieldActions(m_movesetData.shield.shieldIdle, m_movesetData.shield.shieldingUpperBody, m_movesetData.shield.ShieldSpecialLight1, m_movesetData.shield.ShieldSpecialHeavy1);
    }








    void Update()
    {

        SetValues();

        testMoveDirection.transform.localScale = new Vector3(0.07f, 0.07f, Mathf.Min(Mathf.Max(m_inputStrenght, 0.2f), 1.5f));

        if (/*(int)m_actionTurningRelations*/ m_actionMovementHandler.ActionTurningRelation != 1/*TurningDirFollowsMoveDir*/)
        {
            RotatingPlayer();
            MovingPlayer();
        }
        else
        {
            MovingPlayer();
            RotatingPlayer();
        }

        SetAnimatorMoveValues();
        CheckAnimation();

        if (!m_isTurningApplied && Mathf.Abs(m_turningAngle) >= 100 && /*m_prevMove.magnitude != 0 &&*/ m_prevInputStrength == 0 && (m_currentActionAndLayer == c_emptyAction))
        {
            //m_animator.SetTrigger("Turning");
            m_animator.SetFloat("TurningDir", Mathf.Sign(m_turningAngle));
            m_animator.CrossFadeInFixedTime(IsRunning && m_prevMove.magnitude > m_speedValues.x ? AnimationTypes.Turning_Running : AnimationTypes.Turning, m_baseCrossFadeDuration);
            m_isTurningApplied = true;
            m_isSlidingApplied = false;
        }
        if (m_isTurningApplied && Mathf.Abs(m_turningAngle) <= 20)
            m_isTurningApplied = false;
        if (m_isSlidingApplied && (m_inputStrenght != 0 || m_prevMove.magnitude < 1f))
            m_isSlidingApplied = false;

        //Debug.Log(m_prevMove.magnitude);

        m_prevPrevInputStrength = m_prevInputStrength; 
        m_prevInputStrength = m_inputStrenght;


        testMoveDirection.transform.position = new Vector3(transform.position.x, testMoveDirection.transform.position.y, transform.position.z);
        testTurningDirection.transform.position = new Vector3(transform.position.x, testTurningDirection.transform.position.y, transform.position.z);


    }


    #region INITIAL FRAME VALUES

    private Coroutine SwitchFreelyMoving;
    bool m_isAboutSwitchOrientation = false;

    private void SetValues() //moveDir, threshholds, TargetDist, etc
    {
        IsGrounded = m_characterController.isGrounded;

        m_controllerVelocity = m_characterController.velocity;

        //StandingStill
        m_isStandingPrev = m_isStandingStill;
        m_isStandingStill = m_inputStrenght == 0;

        //Running
        IsRunning = (m_isHoldRunning && m_inputStrenght != 0 && m_currentActionAndLayer == c_emptyAction) && m_characterStatus.CheckIfCanConsumeConstantEnergy();
        if (m_isRunning && m_equipmentIsReady) m_characterStatus.ExpendEnergyPoints(30 * Time.deltaTime);

        //FreelyMoving
        bool prevFreelyMoving = m_isFreelyMoving;
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
        if (prevFreelyMoving != m_isFreelyMoving) //Only for one thing, that when locked on and then start running, that the movement is for 0.2s set to input instead of forward
        {
            m_isAboutSwitchOrientation = true;
            SwitchFreelyMoving = StartCoroutine(UtilityFunctions.Wait(0.2f, () => { m_isAboutSwitchOrientation = false; /*Debug.Log*/ }));
        }

        //TargetDist
        if (m_isLockOn)
        {
            m_playerToTargetXZVector = new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z).normalized;
            m_targetDist = (TargetPos - transform.position).magnitude;
        }

        //VALUES
        if (m_isFreelyMoving)
        {
            // InputDirRelativeToCam is relative to cameraRotation, so it should not affect the InputDirRelativeToCam when for example standing still
            m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
            m_orientation = Orientation.Forward;
            return;
        }
        else
        {
            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(m_playerToTargetXZVector, Vector3.up);
            Quaternion playerToTargetAndCameraForwardSlerp = Quaternion.Slerp(m_cameraYAxisRotationInWS, playerToTargetLookRotation, 0.5f);
            m_inputDirInWS = playerToTargetAndCameraForwardSlerp * m_inputDir;

            if (m_currentActionAndLayer.y >= 1 || m_currentActionAndLayer == c_emptyAction) //if action started, the facingType should stay, since its needed for the SetAnimatorMoveValues()
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

        if (m_orientation == Orientation.Left || m_orientation == Orientation.Right) additionalThreshhold = -additionalThreshhold;

        m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, m_targetDist / distThreshhold) + additionalThreshhold; /////////////////Mathf.Min
        m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, m_targetDist / distThreshhold) - additionalThreshhold;

        float inputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);

        if (inputAngleToForward < m_forwardSidewardThreshholdAngle)                                             m_orientation = Orientation.Forward;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle && Mathf.Sign(m_inputDir.x) >= 0)      m_orientation = Orientation.Right;
        else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle)                                       m_orientation = Orientation.Left;
        else                                                                                                    m_orientation = Orientation.Backward; 


    }

    #endregion




    #region TRIGGER ACTIONS

    public void TriggerFlinchAtDamage()
    {
        //does not Affect anything
        SetDamageAnimation(AnimationTypes.Get_Hit, 5, 0);
    }
    public void TriggerStagger()
    {
        //cancels animation and stuns for less than a second, caused by amount of poise damage when below 40% poise
        Debug.Log("TriggerStagger");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType staggerInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)staggerInterruptability) return;
        AnimationData animData = m_movesetData.stagger;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = staggerInterruptability;

        InitAction(AnimationTypes.Stagger, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }
    public void TriggerStun()
    {
        //cancels animation and stuns for more than a second, cause by depleated poise
        Debug.Log("TriggerStun");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType stunInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)stunInterruptability) return;
        AnimationData animData = m_movesetData.stun;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = stunInterruptability;

        InitAction(AnimationTypes.Stun, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }
    public void TriggerFallingOver(Vector3 directionWS)
    {
        // cancels animation and throws character away
        Debug.Log("TriggerFallingOver");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType staggerInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)staggerInterruptability) return;
        AnimationData animData;
        int animHash;
        if (Vector3.Angle(directionWS, transform.forward) <= 90) { animData = m_movesetData.fallingForward; animHash = (int)AnimationTypes.Falling_Forward; }
        else                                                     { animData = m_movesetData.fallingBackwards; animHash = (int)AnimationTypes.Falling_Backward; }

        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = staggerInterruptability;

        InitAction(animHash, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }
    public void TriggerThrownUpwards(Vector3 direction)
    {
        // cancels animation and throws character away
        Debug.Log("TriggerThrownUpwards");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType ThrownUpInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)ThrownUpInterruptability) return;
        AnimationData animData = m_movesetData.thrownUpward;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = ThrownUpInterruptability;

        InitAction(AnimationTypes.Thrown_Upwards, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }

    public void TriggerShieldDeflect()
    {
        //does not Affect anything, same as flinch
        //SetDamageAnimation(AnimationTypes.Shield_Deflect, 5, 0);
        Debug.Log("TriggerShieldDeflect");
        SetDamageAnimation(AnimationTypes.Get_Hit, 5, 0);
    }
    public void TriggerShieldStun()
    {
        // cancels animation and stuns for less than a second, character is still blocking, caused by amount of energy consumption when below 40% energy
        Debug.Log("TriggerShieldStun");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType ShieldStaggerInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)ShieldStaggerInterruptability) return;
        AnimationData animData = m_movesetData.shield.ShieldStanceStagger.AnimData;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = ShieldStaggerInterruptability;

        InitAction(AnimationTypes.Shield_Stance_Stagger, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }
    public void TriggerShieldBreak()
    {
        // cancels animation and stuns for more than a second, character is not blocking, cause by depleated energy
        Debug.Log("TriggerShieldBreak");

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }
        AnimationInterruptableType ShieldBreakInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)ShieldBreakInterruptability) return;
        AnimationData animData = m_movesetData.shield.ShieldStanceBreak.AnimData;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        ///////////////////////////////////////////

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        m_currentInteruptability = ShieldBreakInterruptability;

        InitAction(AnimationTypes.Shield_Stance_Break, animData.bodyParts, animData, m_movesetData.evadeCosts);
    }
    public void TriggerDie()
    {
        Debug.Log("TriggerDie");

    }

    //void TriggerRunningSlide()
    //{
    //    if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

    //    AnimationInterruptableType slidingInterruptability = AnimationInterruptableType.Easily_Interruptable;
    //    if ((int)m_currentInteruptability >= (int)slidingInterruptability) return;

    //    AnimationData animData = m_movesetData.runningSliding;

    //    if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

    //    if (m_ActionCoroutine != null)
    //        EndActionReset();

    //    m_currentInteruptability = slidingInterruptability;

    //    InitAction(AnimationTypes.Running_Sliding, animData.bodyParts, animData);
    //}


    //void TriggerTurning()
    //{
    //    if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

    //    AnimationInterruptableType turningInterruptability = AnimationInterruptableType.Easily_Interruptable;
    //    if ((int)m_currentInteruptability > (int)turningInterruptability) return;    

    //    AnimationData animData = null;
    //    // if the input differs too much, its will trigger an turn. Therefore we need the current and pevious frame latestProcessedDir
    //    float angleMoveDirToPrevMoveDir = m_turningAngle;

    //    if (!m_isRunning && !m_isLockOn && m_isFreelyMoving && (m_prevInputStrength == 0 || m_prevPrevInputStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 90)
    //    {
    //        if ( Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)     animData = m_movesetData.turningLeft;
    //        else                                                animData = m_movesetData.turningRight;   
    //    }
    //    else if (m_isRunning && m_isFreelyMoving &&  (m_prevInputStrength == 0 || m_prevPrevInputStrength == 0) && Mathf.Abs(angleMoveDirToPrevMoveDir) > 150)
    //    { 
    //        if (Mathf.Sign(angleMoveDirToPrevMoveDir) < 0)      animData = m_movesetData.turningRunningLeft;
    //        else                                                animData = m_movesetData.turningRunningRight;
    //    }
    //    else 
    //        return;

    //    if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

    //    m_isTurning = true;

    //    if (m_ActionCoroutine != null)
    //        EndActionReset();

    //    m_animator.SetFloat("TurningDir", Mathf.Sign(angleMoveDirToPrevMoveDir));
    //    m_currentInteruptability = turningInterruptability;

    //    InitAction(!m_isRunning ? AnimationTypes.Turning : AnimationTypes.Turning_Running, animData.bodyParts, animData);
    //}


    public void TriggerReadyOrRemoveEquipment()
    {
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) return;

        int animHashWeapon = 0;
        int animHashShield = 0;
        AnimationData animDataWeapon = null;
        AnimationData animDataShield = null;
        if (m_equipmentIsReady)
        {
            animDataWeapon = m_movesetData.weapon.RemoveWeapon;
            animDataShield =  m_movesetData.shield.RemoveShield;
            animHashWeapon = AnimationTypes.Ready_Weapon;
            animHashShield = AnimationTypes.Ready_Shield;
        }
        else
        {
            animDataWeapon = m_movesetData.weapon.ReadyWeapon;
            animDataShield = m_movesetData.shield.ReadyShield;
            animHashWeapon = AnimationTypes.Remove_Weapon;
            animHashShield = AnimationTypes.Remove_Shield;
        }
        if (animDataWeapon == null) { Debug.Log("MISSING ANIMATION DATA of a ReadyOrRemoveWeapon"); return; }
        if (animDataShield == null) { Debug.Log("MISSING ANIMATION DATA of a ReadyOrRemoveShield"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action readyOrRemoveEffect = EquipmentHandler.Instance == null ? null : () => { m_equipmentIsReady = !m_equipmentIsReady; EquipmentHandler.Instance.ReadyOrRemoveEquipment(m_equipmentIsReady); };

        if(animDataWeapon.animationClip.length >= animDataShield.animationClip.length)
        {
            InitAction(animHashWeapon, animDataWeapon.bodyParts, animDataWeapon, effect: readyOrRemoveEffect);
            //SetUpperBodyAnimation(animHashShield, (int)AnimationData.BodyParts.LeftArm, animDataShield.crossfadeInTime, exeptionForMultipleLayerAnimation: true);
        }
        else
        {
            InitAction(animHashShield, animDataShield.bodyParts, animDataShield, effect: readyOrRemoveEffect);
            //SetUpperBodyAnimation(animHashWeapon, (int)AnimationData.BodyParts.RightArm, animDataWeapon.crossfadeInTime, exeptionForMultipleLayerAnimation: true);
        }

    }


    public void TriggerSwitchWeapon()
    {
        if (!m_equipmentIsReady)
        {
            EquipmentHandler.Instance.SwitchActiveWeapon();
            return;
        }

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) { return; }

        AnimationData animData = m_movesetData.weapon.SwitchWeapon;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA of SwitchWeapon"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action switchEffect = EquipmentHandler.Instance == null ? null : () => { EquipmentHandler.Instance.SwitchActiveWeapon(); Debug.Log("action"); };

        //Debug.Log("remember switch weapon spam bug");
        InitAction(AnimationTypes.Switch_Weapon, animData.bodyParts, animData, effect: switchEffect);

    }

    public void TriggerSwitchShield()
    {
        if (!m_equipmentIsReady)
        {
            EquipmentHandler.Instance.SwitchActiveShield();
            return;
        }

        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) return;

        AnimationData animData = m_movesetData.shield.SwitchShield;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA of SwitchShield"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action switchEffect = EquipmentHandler.Instance == null ? null : () => { EquipmentHandler.Instance.SwitchActiveShield(); };

        Debug.Log("remember switch weapon spam bug");
        InitAction(AnimationTypes.Switch_Shield, animData.bodyParts, animData, effect: switchEffect);

    }


    public void TriggerEvading()
    {
        if (m_isActionLocked) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        //Debug.Log(m_currentInteruptability);
        AnimationInterruptableType evadeInterruptability = AnimationInterruptableType.Not_Interruptable;
        if ((int)m_currentInteruptability >= (int)evadeInterruptability) return;

        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_orientation = Orientation.Forward; //just a reminder

        AnimationData animData;
        int animHash = 0;
        if (m_orientation == Orientation.Backward || m_inputStrenght == 0)      { animData = m_movesetData.evadeBackwards; animHash = AnimationTypes.Evade_Backwards; }
        else if (m_orientation == Orientation.Left)                             { animData = m_movesetData.evadeLeft; animHash = AnimationTypes.Evade_Left; }
        else if (m_orientation == Orientation.Right)                            { animData = m_movesetData.evadeRight; animHash = AnimationTypes.Evade_Right; }
        else                                                                    { animData = m_movesetData.evadeForward; animHash = AnimationTypes.Evade_Forward; }

        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();
        
        SetNextPossibleWeaponAttacks(currentAction: animHash);
        m_currentInteruptability = evadeInterruptability;

        InitAction(animHash, animData.bodyParts, animData, m_movesetData.evadeCosts);

        }

    public void TriggerLightAttack()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.light; 
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return;}
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return; }

        AnimationInterruptableType lightAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)lightAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(thisAction);

        m_currentInteruptability = lightAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentWeaponAttackData = thisAction;
        InitAction(thisAction.AttackHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);
    }

    public void TriggerSpecialLightAttack()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.specialLight;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialLightAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialLightAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(thisAction);

        m_currentInteruptability = specialLightAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentWeaponAttackData = thisAction;
        InitAction(thisAction.AttackHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);
    }

    public void TriggerHeavyAttack()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.heavy;
        if (thisAction == null) { Debug.Log("MISSING ATTACK DATA of a Heavy Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Heavy Attack"); return; }

        AnimationInterruptableType heavyAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)heavyAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(thisAction);

        m_currentInteruptability = heavyAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentWeaponAttackData = thisAction;
        InitAction(thisAction.AttackHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);

    }
    
    public void TriggerSpecialHeavyAttack()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.specialHeavy;
        if (thisAction == null) { Debug.Log("MISSING ATTACK DATA of a Heavy Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Heavy Attack"); return; }

        AnimationInterruptableType specialHeavyAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialHeavyAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleWeaponAttacks(thisAction);

        m_currentInteruptability = specialHeavyAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentWeaponAttackData = thisAction;
        InitAction(thisAction.AttackHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);

    }

    public void TriggerShielding(bool isHoldShielding)
    {
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }
        
        IsHoldShielding = isHoldShielding;

        if (!m_isHoldShielding)
        {
            IsShielding = false;
            return;
        }

        AnimationInterruptableType shieldingInterruptabilityLimit = AnimationInterruptableType.Hardly_Interruptable;
        if ((int)m_currentInteruptability >= (int)shieldingInterruptabilityLimit) return;

        if (m_actionMovementHandler.ActionCoroutine != null ) //stop current animation if its not those: turning
            m_actionMovementHandler.EndAction();

        IsShielding = true;

    }

    public void TriggerShieldSpecialLight()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        ShieldData.ShieldAction thisAction = m_nextPossibleShieldActions.specialShieldLight;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialShieldLightActionInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialShieldLightActionInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleShieldActions(thisAction);

        m_currentInteruptability = specialShieldLightActionInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;
        m_currentShieldActionData = thisAction;
        InitAction(thisAction.ActionkHash, thisAction.AnimData.bodyParts,thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);
    }

    public void TriggerShieldSpecialHeavy()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        ShieldData.ShieldAction thisAction = m_nextPossibleShieldActions.specialShieldHeavy;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialShieldHeavyActionInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialShieldHeavyActionInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;


        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_actionMovementHandler.ActionCoroutine != null)
            m_actionMovementHandler.EndAction();

        SetNextPossibleShieldActions(thisAction);

        m_currentInteruptability = specialShieldHeavyActionInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentShieldActionData = thisAction;
        InitAction(thisAction.ActionkHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);
    }

    public void TriggerItemUse()
    {
        if (m_isActionLocked) return;
        if (m_movesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        if (m_movesetData.item == null) { Debug.Log("MISSING Item DATA"); return; }
        ItemData.ItemAction thisAction = m_movesetData.item.ItemUse;
        if (thisAction == null) { Debug.Log("MISSING ACTION DATA of a Item Action"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Item Action"); return; }

        AnimationInterruptableType itemUseInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;

        if ((int)m_currentInteruptability >= (int)itemUseInterruptability) return;

        //if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        //if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.EnergyCost)) return;

        if (m_actionMovementHandler.ActionCoroutine != null && (thisAction.AnimData.bodyParts == AnimationData.BodyParts.WholeBody))
            m_actionMovementHandler.EndAction();

        m_currentInteruptability = itemUseInterruptability;

        InitAction(AnimationTypes.Use_Item, thisAction.AnimData.bodyParts, thisAction.AnimData);
    }

    #endregion




    #region SET ACTIONS
    private void InitAction(int animHash, AnimationData.BodyParts animLayer, AnimationData animData, int staminaCost = 0, int specialEnergyCost = 0, Action effect = null)
    {
        m_currentActionAndLayer = new Vector2(animHash, (int)animLayer);
        IsRunning = false;
        IsShielding = false;
        SetBodyLookAtTarget(animData.actionUsesLookAtTargetData ? m_target : null);

        if ((int)animLayer == 0)
        {
            SetAnimation(animHash, animData.crossfadeInTime, 0); //this sets and activates the animation with given crossfadeInTime
            m_nextCrossfadeOutTime = animData.crossfadeOutTime; //this is set and stored for end of action for the case the animation fades out normally and is not interrupted by an action with its own fadeInTime
        }
        else
        {
            if (animData.useLookAtForwardData)
                SetLookAtForward(true, animData.lookAtData);
            //SetLookAtTarget(null); //????? Depends on animation and if AddTurning
            SetUpperBodyAnimation(animHash, (int)animLayer, animData.crossfadeInTime); //this sets and activates the animation with given crossfadeInTime
            m_nextUpperBodyCrossfadeOutTime = animData.crossfadeOutTime; //this is set and stored for end of action for the case the animation fades out normally and is not interrupted by an action with its own fadeInTime
        }

        SetValues(); //needed, because what if it jumps from one action directly into another

        m_characterStatus.PauseEnergyRegenerationByAction();

        //effects like pay stamina cost at that moment
        List<EffectQueue> effectList = new List<EffectQueue>();
        if (effect != null)
            effectList.Add(new EffectQueue(effect, animData.MainActionMomentTime));
        if (staminaCost != 0)
        {
            Action payActionCostsAction = () => { m_characterStatus.ExpendEnergyPoints(staminaCost); m_characterStatus.ExpendSpecialEnergyPoints(specialEnergyCost); m_actionPayCostCouroutine = null; };
            effectList.Add(new EffectQueue(payActionCostsAction, animData.MainActionMomentTime));
        }

        if (animData.InterruptabilityChangeBeforeEndTime != 0 && animData.ChangedInterruptability != m_currentInteruptability)
        {
            Action ChangeInteruptability = () =>
            {
                m_characterStatus.ContinueEnergyRegenerationInTime();
                m_currentInteruptability = animData.ChangedInterruptability;
                //m_actionChangesInterruptabilityCoroutine = null;
                if (m_playerInputManager.CheckRecallLatestBufferedInput())
                    m_actionMovementHandler.EndAction();
            };
            effectList.Add(new EffectQueue(ChangeInteruptability, (animData.animationClip.length - animData.InterruptabilityChangeBeforeEndTime - animData.crossfadeOutBeginn) / animData.animationClip.length));
        }

        if (animData.IsPausingGravity)
        {
            Action PauseGravity = () =>
            {
                Gravity = 0;
                m_footPlacing.SetWeightActive(false);
            };
            effectList.Add(new EffectQueue(PauseGravity, animData.PauseGravityTime.x));
            Action ContinueGravity = () => 
            {
                Gravity = m_originalGravity;
            };
            effectList.Add(new EffectQueue(ContinueGravity, animData.PauseGravityTime.y));
        }

        if (animData.IsPausingMidAir)
        {
            Action PauseAnimation = () =>
            {
                if (!m_isGrounded)
                {
                    m_isMidAirPause = true;
                    m_slowDownAnimSpeedCoroutine = StartCoroutine(SlowDownAnimSpeed());
                }
            };
            effectList.Add(new EffectQueue(PauseAnimation, (animData.PauseMidAirTime - ((m_animSlowDownDuration/2) / animData.animationClip.length))));
        }


        ////HITBOXES On and Off
        if (animData.hitBoxActiveData.Count != 0)
        {
            DamageData actionDamageData = null;
            if (animData.hitBoxActiveData.Count != 0)
                actionDamageData = m_currentWeaponAttackData != null ? m_characterStatus.GetActionDamageData(m_currentWeaponAttackData, transform.forward, m_movesetData.weapon.BasePhysicalType)
                                                                     : m_characterStatus.GetActionDamageData(m_currentShieldActionData, transform.forward, m_movesetData.shield.PhysicalType);

            foreach (AnimationData.HitBoxActiveData hitActiveData in animData.hitBoxActiveData)
            {
                Action ToggleHitBoxesActive = () =>
                {
                    if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) m_characterStatus.HitBoxManagerWeapon.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
                    if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
                };
                effectList.Add(new EffectQueue(ToggleHitBoxesActive, hitActiveData.activeTime.x));

                Action ToggleHitBoxesDeactive = () =>
                {
                    if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) m_characterStatus.HitBoxManagerWeapon.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
                    if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
                };
                effectList.Add(new EffectQueue(ToggleHitBoxesDeactive, hitActiveData.activeTime.y));

            }
        }




            //HERE THE ACTION STARTS
            m_actionMovementHandler.StartAction(animData, effectList, m_moveAcceleration, m_turningStrenght, m_maxTurningSpeed, m_inputDirInWS, transform.forward);
    }



    #region Weapon Shield

    private void SetNextPossibleWeaponAttacks(WeaponData.WeaponAttack currentAttackData = null, int currentAction = 0)
    {
        if (currentAction == AnimationTypes.Evade_Forward || currentAction == AnimationTypes.Evade_Left || currentAction == AnimationTypes.Evade_Right || currentAction == AnimationTypes.Evade_Backwards)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_movesetData.weapon.EvadeLightAttack, m_movesetData.weapon.EvadeHeavyAttack, m_movesetData.weapon.SpecialLightAttack1, m_movesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == AnimationTypes.Running)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_movesetData.weapon.SprintLightAttack, m_movesetData.weapon.SprintHeavyAttack, m_movesetData.weapon.SpecialLightAttack1, m_movesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == AnimationTypes.Reset)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_movesetData.weapon.LightAttack1, m_movesetData.weapon.HeavyAttack1, m_movesetData.weapon.SpecialLightAttack1, m_movesetData.weapon.SpecialHeavyAttack1);

        else if (currentAttackData != null)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(GetNextAttackLight(currentAttackData.nextLight), GetNextAttackHeavy(currentAttackData.nextHeavy), GetNextAttackSpecialLight(currentAttackData.nextSpecialLight), GetNextAttackSpecialHeavy(currentAttackData.nextSpecialHeavy));


        WeaponData.WeaponAttack GetNextAttackLight(WeaponData.LightAttack light)
        {
            switch (light)
            {
                case WeaponData.LightAttack.Light_Attack_1: if (m_movesetData.weapon.LightAttack1.AnimData != null) return m_movesetData.weapon.LightAttack1; break;
                case WeaponData.LightAttack.Light_Attack_2: if (m_movesetData.weapon.LightAttack2.AnimData != null) return m_movesetData.weapon.LightAttack2; break;
                case WeaponData.LightAttack.Light_Attack_3: if (m_movesetData.weapon.LightAttack3.AnimData != null) return m_movesetData.weapon.LightAttack3; break;
                case WeaponData.LightAttack.Light_Attack_4: if (m_movesetData.weapon.LightAttack4.AnimData != null) return m_movesetData.weapon.LightAttack4; break;
                case WeaponData.LightAttack.Light_Attack_5: if (m_movesetData.weapon.LightAttack5.AnimData != null) return m_movesetData.weapon.LightAttack5; break;
                case WeaponData.LightAttack.Light_Attack_6: if (m_movesetData.weapon.LightAttack6.AnimData != null) return m_movesetData.weapon.LightAttack6; break;
                case WeaponData.LightAttack.Sprint_Light_Attack: if (m_movesetData.weapon.SprintLightAttack.AnimData != null) return m_movesetData.weapon.SprintLightAttack; break;
                case WeaponData.LightAttack.Evade_Light_Attack: if (m_movesetData.weapon.EvadeLightAttack.AnimData != null) return m_movesetData.weapon.EvadeLightAttack; break;
            }
            //Debug.Log("Warning: Next Possible Light Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.weapon.LightAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackHeavy(WeaponData.HeavyAttack heavy)
        {
            switch (heavy)
            {
                case WeaponData.HeavyAttack.Heavy_Attack_1: if (m_movesetData.weapon.HeavyAttack1.AnimData != null) return m_movesetData.weapon.HeavyAttack1; break;
                case WeaponData.HeavyAttack.Heavy_Attack_2: if (m_movesetData.weapon.HeavyAttack2.AnimData != null) return m_movesetData.weapon.HeavyAttack2; break;
                case WeaponData.HeavyAttack.Heavy_Attack_3: if (m_movesetData.weapon.HeavyAttack3.AnimData != null) return m_movesetData.weapon.HeavyAttack3; break;
                case WeaponData.HeavyAttack.Heavy_Attack_4: if (m_movesetData.weapon.HeavyAttack4.AnimData != null) return m_movesetData.weapon.HeavyAttack4; break;
                case WeaponData.HeavyAttack.Sprint_Heavy_Attack: if (m_movesetData.weapon.SprintHeavyAttack.AnimData != null) return m_movesetData.weapon.SprintHeavyAttack; break;
                case WeaponData.HeavyAttack.Evade_Heavy_Attack: if (m_movesetData.weapon.EvadeHeavyAttack.AnimData != null) return m_movesetData.weapon.EvadeHeavyAttack; break;
            }
            //Debug.Log("Warning: Next Possible Heavy Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.weapon.HeavyAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialLight(WeaponData.LightAttackSpecial specialLight)
        {
            switch (specialLight)
            {
                case WeaponData.LightAttackSpecial.Special_Light_Attack_1: if (m_movesetData.weapon.SpecialLightAttack1.AnimData != null) return m_movesetData.weapon.SpecialLightAttack1; break;
                case WeaponData.LightAttackSpecial.Special_Light_Attack_2: if (m_movesetData.weapon.SpecialLightAttack2.AnimData != null) return m_movesetData.weapon.SpecialLightAttack2; break;
            }
            //Debug.Log("Warning: Next Possible Special Light Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.weapon.SpecialLightAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialHeavy(WeaponData.HeavyAttackSpecial specialHeavy)
        {
            switch (specialHeavy)
            {
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_1: if (m_movesetData.weapon.SpecialHeavyAttack1.AnimData != null) return m_movesetData.weapon.SpecialHeavyAttack1; break;
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_2: if (m_movesetData.weapon.SpecialHeavyAttack2.AnimData != null) return m_movesetData.weapon.SpecialHeavyAttack2; break;
            }
            //Debug.Log("Warning: Next Possible Special Heavy Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.weapon.SpecialHeavyAttack1;
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
    private void SetNextPossibleShieldActions(ShieldData.ShieldAction currentActionData = null, int currentAction = 0)
    {
        if (currentAction == AnimationTypes.Reset)
            m_nextPossibleShieldActions = new NextPossibleShieldActions(m_movesetData.shield.shieldIdle, m_movesetData.shield.shieldingUpperBody, m_movesetData.shield.ShieldSpecialLight1, m_movesetData.shield.ShieldSpecialHeavy1);

        else if (currentActionData != null)
            m_nextPossibleShieldActions = new NextPossibleShieldActions(m_movesetData.shield.shieldIdle, m_movesetData.shield.shieldingUpperBody, GetNextShieldSpecialLight(currentActionData.nextSpecialLight), GetNextShieldSpecialHeavy(currentActionData.nextSpecialHeavy));

        ShieldData.ShieldAction GetNextShieldSpecialLight(ShieldData.ShieldSpecialLight specialLight)
        {
            switch (specialLight)
            {
                case ShieldData.ShieldSpecialLight.Shield_Special_Light_Action_1: if (m_movesetData.shield.ShieldSpecialLight1.AnimData != null) return m_movesetData.shield.ShieldSpecialLight1; break;
                case ShieldData.ShieldSpecialLight.Shield_Special_Light_Action_2: if (m_movesetData.shield.ShieldSpecialLight2.AnimData != null) return m_movesetData.shield.ShieldSpecialLight2; break;
            }
            //Debug.Log("Warning: Next Possible Special Light Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.shield.ShieldSpecialLight1;
        }
        ShieldData.ShieldAction GetNextShieldSpecialHeavy(ShieldData.ShieldSpecialHeavy specialHeavy)
        {
            switch (specialHeavy)
            {
                case ShieldData.ShieldSpecialHeavy.Shield_Special_Heavy_Action_1: if (m_movesetData.shield.ShieldSpecialHeavy1.AnimData != null) return m_movesetData.shield.ShieldSpecialHeavy1; break;
                case ShieldData.ShieldSpecialHeavy.Shield_Special_Heavy_Action_2: if (m_movesetData.shield.ShieldSpecialHeavy2.AnimData != null) return m_movesetData.shield.ShieldSpecialHeavy2; break;
            }
            //Debug.Log("Warning: Next Possible Special Heavy Attack in line has no AnimationData, so the next will be the first one again");
            return m_movesetData.shield.ShieldSpecialHeavy1;
        }
    }
    public int GetInterruptabilityShieldLightSpecial()
    {
        if (m_nextPossibleShieldActions.specialShieldLight.AnimData == null || m_nextPossibleShieldActions.specialShieldLight.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleShieldActions.specialShieldLight.AnimData.CustomInterruptability;
    }
    public int GetInterruptabilityShieldHeavySpecial()
    {
        if (m_nextPossibleShieldActions.specialShieldHeavy.AnimData == null || m_nextPossibleShieldActions.specialShieldHeavy.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton) return (int)AnimationInterruptableType.Not_Interruptable;
        else return (int)m_nextPossibleShieldActions.specialShieldHeavy.AnimData.CustomInterruptability;
    }
    #endregion
    #endregion







    #region MOVING AND ROTATING

    private void SetBodyLookAtTarget(Transform transform)
    {
        if (m_lookAtScript != null)
            m_lookAtScript.SetTarget(transform);
    }
    private void SetLookAtForward(bool active, LookAtData forwardData = null)
    {
        if (m_lookAtScript == null)
            return;
            
        if (active) m_lookAtScript.SetForwardActive(forwardData);
        else m_lookAtScript.SetForwardDeactive();

    }
    private Quaternion OrientationRotation()
    {
        if (m_disableSidewardMovement)
            return Quaternion.identity;

        if (m_orientation == Orientation.Forward) return Quaternion.identity;
        if (m_orientation == Orientation.Right) return Quaternion.Euler(0, -90, 0);
        else if (m_orientation == Orientation.Left) return Quaternion.Euler(0, 90, 0);
        else return Quaternion.Euler(0, 180, 0);
    }
    private void RotatingPlayer()
    {
        //if no input, then it should not recalculate the desired facing direction, because what if i stand still and then lock on something behind me, it should not affect any calculation as long as i dont move
        // also, actions like evading set their initial m_desiredFacingRotationDirInWS in their own Trigger function
        if (!m_isStandingStill)
            m_desiredFacingRotationDirInWS = OrientationRotation() * m_inputDirInWS;
        else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && !m_isRunning)
            m_desiredFacingRotationDirInWS = m_playerToTargetXZVector;
        else if (m_isLockOn && m_isStandingStill && !m_isStandingPrev && m_isRunning)
            m_desiredFacingRotationDirInWS = m_desiredFacingRotationDirInWS;

        //turningSpeedBy by movementSpeed
        m_maxTurningSpeed = ((m_currentBaseSpeed <= m_speedValues.y) ?
            UtilityFunctions.RefitToNewRange(m_prevMove.magnitude, m_speedValues.x, m_speedValues.y, m_maxTurningSpeedBaseValues.x, m_maxTurningSpeedBaseValues.y) :
            !m_isTurningApplied ? UtilityFunctions.RefitToNewRange(m_prevMove.magnitude, m_speedValues.y, m_speedValues.z, m_maxTurningSpeedBaseValues.y, m_maxTurningSpeedBaseValues.z) : m_maxTurningSpeedBaseValues.z);

        float thisAngle = m_actionMovementHandler.GetRotation(ref m_desiredFacingRotationDirInWS, ref m_maxTurningSpeed, ref m_turningStrenght, ref m_playerToTargetXZVector);
        m_turningAngle = m_actionMovementHandler.DesiredTurningAngle;
        //this makes the char rotate not around it center when walking and turning, but rotates around a pont slightly to the side
        //float turnRotationPointOffsetXAxis = !m_isAction && !m_isIgnoreTurningOffset ? (Mathf.Sign(newAngle) * m_prevMove.magnitude / 1.8f) : 0;
        //Vector3 rotationCenterOffset = new Vector3(turnRotationPointOffsetXAxis, 0, 0);

        //RotateAround() isnt actually working when using Move()
        transform.RotateAround(transform.position/* + (transform.rotation * rotationCenterOffset)*/, Vector3.up, thisAngle);
    }

    private void MovingPlayer()
    {

        ////this is for the case of uneven ground, the player will walk slower when walking on a hill/stairs
        //RaycastHit groundHitDir;
        //RaycastHit groundHit;
        //if (m_isGrounded && Physics.Raycast(transform.position + (m_nowMoveDir * (m_currentBaseSpeed + 0.2f) * 0.2f) + (Vector3.up * 0.5f), Vector3.down, out groundHitDir, 1, m_environmentLayer))
        //{
        //    Vector3 playerPos = (Physics.Raycast(m_chraracterMesh.transform.position + (Vector3.up * 0.5f), Vector3.down, out groundHit, 1, m_environmentLayer) ? groundHit.point : m_chraracterMesh.transform.position);
        //    m_nowMoveDir = Quaternion.AngleAxis(-Mathf.Min(45, 90 - Vector3.Angle(Vector3.up, (groundHitDir.point - playerPos).normalized)), Vector3.Cross(Vector3.up, groundHitDir.point - playerPos)) * m_nowMoveDir;
        //    //Debug.Log(m_nowMoveDir.magnitude);
        //    //Debug.Log(UtilityFunctions.VectorXZ(m_nowMoveDir).magnitude);
        //    //Debug.DrawLine(playerPos + Vector3.up * 0.5f, (playerPos + Vector3.up * 0.5f) + m_nowMoveDir.normalized * 5, Color.green);
        //    //Debug.DrawLine(playerPos + m_nowMoveDir * nowSpeed * 0.2f + Vector3.up * 0.5f, (playerPos + m_nowMoveDir * nowSpeed * 0.2f + Vector3.up * 0.5f) + Vector3.up * 5, Color.blue);
        //}


        //this is for the case of uneven ground, the player will walk slower when walking on a hill/stairs
        RaycastHit groundHitDir;
        RaycastHit groundHit;
        float terrainFactor = 1; // this is alternately the same as above, but only as a factor instead of rotationg the direction
        if (m_isGrounded && Physics.Raycast(m_chraracterMesh.transform.position + (m_nowMoveDir * (m_currentBaseSpeed + 0.2f) * 0.2f) + (Vector3.up * 0.5f), Vector3.down, out groundHitDir, 1, m_environmentLayer))
            terrainFactor = Mathf.Cos(Mathf.Deg2Rad * Mathf.Abs(90 - Vector3.Angle(Vector3.up, (Physics.Raycast(m_chraracterMesh.transform.position + (Vector3.up * 0.5f), Vector3.down, out groundHit, 1, m_environmentLayer) ? groundHitDir.point - groundHit.point : m_chraracterMesh.transform.position).normalized)));

        //m_inputDirInWS = (m_isAboutSwitchOrientation && m_isFreelyMoving) ?  transform.forward : m_inputDirInWS;
        //speedFactor by turningangle
        float speedFactorByAngle = ((m_isTurningApplied) ? Mathf.Lerp(1, 0, (Mathf.Max(Mathf.Abs(m_turningAngle) - 20, 0) / 50)) : 1);
        //speedFactor by turningangle
        float accelerationFactorByTurning = ((m_currentBaseSpeed > m_speedValues.y && m_isTurningApplied) || m_isSlidingApplied ? 0.2f : 1f);
        //Gravity
        if (m_currentGravity == 0)
            m_velocityThroughGravity = 0;
        else if (m_characterController.isGrounded && m_velocityThroughGravity < 0)
            m_velocityThroughGravity = -2f; // small downward force to keep grounded
        m_velocityThroughGravity += m_currentGravity * Time.deltaTime;
        Vector3 gravity = new Vector3(0, m_velocityThroughGravity, 0);

        Debug.Log(m_currentMoveSpeedReference);
        m_currentMoveSpeedReference = m_currentBaseSpeed * speedFactorByAngle * terrainFactor;
        m_currentMoveAccelerationReference = m_moveAcceleration * accelerationFactorByTurning;

        Vector3 nowMove = m_actionMovementHandler.GetMove(ref m_inputDirInWS, ref m_currentMoveSpeedReference, ref m_currentMoveAccelerationReference, ref m_playerToTargetXZVector);

        m_characterController.Move((nowMove + gravity) * Time.deltaTime);
        m_prevMove = nowMove;
    }

    #endregion















    //#region ACTION CALCULATIONS

    ////Action Influence Values
    //private Vector3 m_directionByAction = Vector3.forward;
    //private float m_actionInfluenceOverMoveDirection = 0;

    //private float m_speedByAction = 0;
    //private float m_actionInfluenceOverMoveSpeed = 0;

    //private float m_moveAccelerationByAction = 0;
    //private float m_actionInfluenceOverMoveAcceleration = 0;

    //private Vector3 m_desiredFacingRotationDirInWSByAction = Vector3.forward;
    //private float m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;

    //private float m_turningStrenghtByAction = 0;
    //private float m_actionInfluenceOverTurningStrenght = 0;

    //private float m_maxTurningSpeedByInputByAction = 0;
    //private float m_actionInfluenceOverMaxTurningSpeed = 0;

    //private Vector3 m_directionByActionBaseValue = Vector3.forward;
    //private Vector3 m_desiredFacingRotationDirInWSByActionBaseValue = Vector3.forward;

    //private AnimationMovementData.TargetRelations m_actionTargetRelations = 0;
    //private AnimationMovementData.TurningRelations m_actionTurningRelations = 0;

    //private float m_actionTimeTillNextChange = 0f;

    //private void StartAction(AnimationData animData, List<Action> effectList = null)
    //{
    //    AnimationMovementData animMoveData = animData.AnimationMovementData;
    //    float animationDuration = animData.animationClip.length;
    //    float crossfadeOutTime = animData.crossfadeOutTime;
    //    float crossfadeStartBeforeEndTime = Mathf.Max(0, 1f, animData.crossfadeOutBeginn);

    //    if (m_ActionCoroutine != null)
    //    {
    //        StopCoroutine(m_ActionCoroutine);
    //        m_ActionCoroutine = null;
    //    }

    //    List<ProcessedAnimationMovementData.DataCurves> CurveValuesList = new List<ProcessedAnimationMovementData.DataCurves>(); 
    //    List<ProcessedAnimationMovementData.DataStartEnd> RangeValuesList = new List<ProcessedAnimationMovementData.DataStartEnd>(); 

    //    if (animMoveData == null)
    //    {
    //        Debug.Log("ANIMATION_Moveset_DATA IS NULL");
    //        animMoveData = m_movesetData.emptyFallbackAnimation.AnimationMovementData;
    //    }


    //    int moveDirPredefinition = (int)animMoveData.moveDirPredefinition;
    //    int turningDirPredefinition = (int)animMoveData.turningDirPredefinition;
    //    float startMoveInfluence = animMoveData.moveInfluence  == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;  
    //    float startTurningInfluence = animMoveData.turningInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;

    //    m_actionTargetRelations = animMoveData.targetRelations;
    //    m_actionTurningRelations = animMoveData.turningRelations;
    //    m_disableSidewardMovement = animMoveData.m_disableSidewardMovement;


    //    //initial moveDir
    //    if ((int)m_actionTurningRelations == 2 /*MoveDirFollowsTurningDir*/ || (m_isLockOn && (int)m_actionTargetRelations == 2 /*MoveDirFollowsTarget*/)) 
    //    {
    //        m_directionByAction = /*Quaternion.Inverse(AdditionalFacingRotation()) **/ Vector3.forward;
    //        m_directionByActionBaseValue = Vector3.forward;
    //    } 
    //    else
    //    {
    //        Vector3 moveDir = Vector3.zero;
    //        if (moveDirPredefinition == 1 /*LatestInput*/)      moveDir = m_inputDirInWS;
    //        if (moveDirPredefinition == 2 /*LatestFrame)*/)     moveDir = transform.forward;
    //        m_directionByAction = m_directionByActionBaseValue = moveDir;
    //    }
    //    m_actionInfluenceOverMoveDirection = startMoveInfluence;
    //    m_speedByAction = 0; // is set to 0
    //    m_actionInfluenceOverMoveSpeed = startMoveInfluence;
    //    m_moveAccelerationByAction = m_moveAcceleration; // is set to current acc
    //    m_actionInfluenceOverMoveAcceleration = startMoveInfluence;


    //    //initial turningDir
    //    if ((int)m_actionTurningRelations == 1 /*TurningDirFollowsMoveDir*/ || (m_isLockOn && (int)m_actionTargetRelations == 1 /*TurningDirFollowsTarget*/)) 
    //    {
    //        m_desiredFacingRotationDirInWSByAction = /*AdditionalFacingRotation() **/ Vector3.forward;
    //        m_desiredFacingRotationDirInWSByActionBaseValue = Vector3.forward;
    //    }  
    //    else
    //    {
    //        Vector3 turningDir = Vector3.zero;
    //        if (turningDirPredefinition == 1 /*latestInputWithAddTurning*/) turningDir = /*AdditionalFacingRotation() **/ m_inputDirInWS;
    //        if (turningDirPredefinition == 2 /*latestFrame)*/)              turningDir = transform.forward;
    //        m_desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByActionBaseValue = turningDir;
    //    }
    //    m_actionInfluenceOverDesiredFacingRotationDirInWS = startTurningInfluence;
    //    m_turningStrenghtByAction = m_turningStrenght; // is set to current strenght
    //    m_actionInfluenceOverTurningStrenght = startTurningInfluence;
    //    m_maxTurningSpeedByInputByAction = m_maxTurningSpeed; // is set to current maxspeed
    //    m_actionInfluenceOverMaxTurningSpeed = startTurningInfluence;



    //    //testTurningDirection.transform.localRotation = Quaternion.LookRotation(m_desiredFacingRotationDirInWSByActionBaseValue, Vector3.up);



    //    foreach (var value in animMoveData.variableValue)
    //    {
    //        if (value.ignore)
    //            continue;
    //        AnimationMovementData.Values.Settings valueData = value.settings;
    //        AnimationMovementData.Values.Settings.Influence influenceData = value.settings.customInfluenceOverInput;
    //        bool valueTypeIsConstant = valueData.valueType == AnimationMovementData.ValueType.ConstantValue;
    //        bool valueTypeIsStartEnd = valueData.valueType == AnimationMovementData.ValueType.StartEndValue;
    //        bool influenceValueTypeIsConstant = influenceData.influenceType == AnimationMovementData.InfluenceValueType.ConstantInfluence;
    //        bool influenceValueTypeIsStartEnd = influenceData.influenceType == AnimationMovementData.InfluenceValueType.StartEndInfluence;

    //        switch (value.valueName)
    //        {
    //         //MOVING
    //            case AnimationMovementData.ValueName.Move_Direction_Angle:
    //                if (valueTypeIsConstant)            m_directionByAction = Quaternion.Euler(0, valueData.value, 0) * m_directionByActionBaseValue; 
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveDirection = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd));
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;
    //            case AnimationMovementData.ValueName.Move_Speed:
    //                if (valueTypeIsConstant)            m_speedByAction = valueData.value;
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveSpeed = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;
    //            case AnimationMovementData.ValueName.Move_Acceleration:
    //                if (valueTypeIsConstant)            m_moveAccelerationByAction = valueData.value;
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Move_Acceleration, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverMoveAcceleration = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd));
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;

    //         //TURNING
    //            case AnimationMovementData.ValueName.Turning_Direction_Angle:

    //                if (valueTypeIsConstant)                m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, valueData.value, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverDesiredFacingRotationDirInWS = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd)); 
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;
    //            case AnimationMovementData.ValueName.Turning_Strenght:

    //                if (valueTypeIsConstant)            m_turningStrenghtByAction = valueData.value;
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Turning_Strenght, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverTurningStrenght = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd));
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;
    //            case AnimationMovementData.ValueName.Max_Turning_Speed:

    //                if (valueTypeIsConstant)            m_maxTurningSpeedByInputByAction = valueData.value;
    //                else if (valueTypeIsStartEnd)       RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd));
    //                else                                CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.Max_Turning_Speed, valueData.value, valueData.valueSettings.startEnd, valueData.valueSettings.curveValue));

    //                if (influenceValueTypeIsConstant)           m_actionInfluenceOverMaxTurningSpeed = influenceData.influence;
    //                else if (influenceValueTypeIsStartEnd)      RangeValuesList.Add(new ProcessedAnimationMovementData.DataStartEnd(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd));
    //                else                                        CurveValuesList.Add(new ProcessedAnimationMovementData.DataCurves(ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed, influenceData.influence, influenceData.influenceSettings.startEnd, influenceData.influenceSettings.curveValue));

    //                break;

    //        }
    //    }

    //    m_currentActionAnimData = animData;
    //    ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, animData, effectList); //This could be saved somewhere in future!

    //    m_ActionCoroutine = StartCoroutine(PerformAction(processedData));
    //}


    //private IEnumerator PerformAction(ProcessedAnimationMovementData processedData)
    //{
    //    float elapsedTime = 0;
    //    float startTime = Time.time;
    //    float timeSteps = processedData.AnimationData.AnimationMovementData == null ? 0.05f : processedData.AnimationData.AnimationMovementData.timeStepsForCurves;
    //    float delayByMidAir = 0;

    //    float duration = processedData.AnimationData.animationClip.length; //what about blendtrees, do they affect it?

    //    DamageData actionDamageData = null;
    //    List<int> activeHitBoxActiveDataList = new List<int>();
    //    if (processedData.AnimationData.hitBoxActiveData.Count != 0)
    //        actionDamageData = m_currentWeaponAttackData != null ? m_characterStatus.GetActionDamageData(m_currentWeaponAttackData, transform.forward, m_movesetData.weapon.BasePhysicalType) 
    //                                                             : m_characterStatus.GetActionDamageData(m_currentShieldActionData, transform.forward, m_movesetData.shield.PhysicalType);
    //    Action pauseMidAirAction = null;
    //    if (processedData.AnimationData.IsPausingMidAir) pauseMidAirAction = () => 
    //    {
    //        if (!m_isGrounded)
    //        {
    //            m_isMidAirPause = true;
    //            m_slowDownAnimSpeedCoroutine = StartCoroutine(SlowDownAnimSpeed());
    //            //m_animationSpeed = 0;
    //            //m_animator.speed = m_animationSpeed;
    //        }
    //    };

    //    void SetValueByName(ProcessedAnimationMovementData.ValueName name, float newValue)
    //    {
    //        switch (name)
    //        {
    //            case ProcessedAnimationMovementData.ValueName.Move_Direction_Angle:                     
    //                m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
    //                break;
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle:         m_actionInfluenceOverMoveDirection                      = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.Move_Speed:                               m_speedByAction                                         = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed:                   m_actionInfluenceOverMoveSpeed                          = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.Move_Acceleration:                        m_moveAccelerationByAction                              = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration:            m_actionInfluenceOverMoveAcceleration                   = newValue; break;

    //            case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:
    //                m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
    //                break; 
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle:      m_actionInfluenceOverDesiredFacingRotationDirInWS       = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.Turning_Strenght:                         m_turningStrenghtByAction                               = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght:             m_actionInfluenceOverTurningStrenght                    = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.Max_Turning_Speed:                        m_maxTurningSpeedByInputByAction                        = newValue; break;
    //            case ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed:            m_actionInfluenceOverMaxTurningSpeed                    = newValue; break;
    //        }
    //    }

    //    while (elapsedTime <= duration - processedData.AnimationData.crossfadeOutBeginn)
    //    {

    //        if (m_actionTimeTillNextChange <= 0)
    //        {
    //            float timeTillEnd = ((duration - processedData.AnimationData.crossfadeOutBeginn) - elapsedTime);
    //            float waitTime = timeTillEnd;
    //            float relativeElapsedTime = elapsedTime / duration;



    //            //INTERRUPTABILITY this is if a action is earlier interruptable than the lenght of the animation
    //            if (m_currentInteruptability != processedData.AnimationData.ChangedInterruptability)
    //            {
    //                //processedData.AnimationData.InterruptabilityChangeBeforeEndTime;
    //                float timetillChangeInteruptability = timeTillEnd - processedData.AnimationData.InterruptabilityChangeBeforeEndTime;
    //                if (timetillChangeInteruptability <= 0)
    //                {
    //                    m_characterStatus.ContinueEnergyRegenerationInTime();
    //                    m_currentInteruptability = processedData.AnimationData.ChangedInterruptability;
    //                    m_actionChangesInterruptabilityCoroutine = null;
    //                    if (m_playerInputManager.CheckRecallLatestBufferedInput())
    //                    {
    //                        EndActionReset();
    //                        yield break;
    //                    }
    //                }
    //                else
    //                    waitTime = Mathf.Min(timetillChangeInteruptability, waitTime);
    //            }

    //            //EFFECT LIST //effects like pay stamina cost or switch weapons
    //            if (processedData.Effects != null)
    //            {
    //                //float timetillEffectTime = timeTillEnd - duration * (1 - processedData.AnimationData.MainActionMomentTime);
    //                float timetillEffectTime = (duration * processedData.AnimationData.MainActionMomentTime) - ((duration - processedData.AnimationData.crossfadeOutBeginn) - timeTillEnd)  ;
    //                if (timetillEffectTime <= 0)
    //                {
    //                    foreach (Action effect in processedData.Effects)
    //                    {
    //                        effect.Invoke();
    //                    }
    //                    processedData.Effects = null;
    //                }
    //                else
    //                    waitTime = Mathf.Min(timetillEffectTime, waitTime);
    //            }

    //            //PAUSE GRAVITY On and Off
    //            if (processedData.AnimationData.IsPausingMidAir)
    //            {
    //                float timeTillGravityChange = waitTime;
    //                if (relativeElapsedTime >= processedData.AnimationData.PauseGravityTime.x && relativeElapsedTime < processedData.AnimationData.PauseGravityTime.y &&  Gravity != 0)
    //                {
    //                    Gravity = 0;
    //                    m_footPlacing.SetWeightActive(false);
    //                }
    //                else if ( relativeElapsedTime > processedData.AnimationData.PauseGravityTime.y && Gravity == 0)
    //                {
    //                    Gravity = m_originalGravity;
    //                }

    //                if (relativeElapsedTime < processedData.AnimationData.PauseGravityTime.x)
    //                    timeTillGravityChange = (processedData.AnimationData.PauseGravityTime.x - relativeElapsedTime) * duration;
    //                else if (relativeElapsedTime < processedData.AnimationData.PauseGravityTime.y)
    //                    timeTillGravityChange = (processedData.AnimationData.PauseGravityTime.y - relativeElapsedTime) * duration;

    //                waitTime = Mathf.Min(timeTillGravityChange, waitTime);

    //            }

    //            //PAUSE Animation when MidAir 
    //            if (processedData.AnimationData.IsPausingMidAir)
    //            {
    //                if (pauseMidAirAction != null && relativeElapsedTime >= (processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration/2) / duration))
    //                {
    //                    pauseMidAirAction.Invoke();
    //                    pauseMidAirAction = null;
    //                }

    //                if (relativeElapsedTime < (processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration / 2) / duration))
    //                    waitTime = Mathf.Min(((processedData.AnimationData.PauseMidAirTime - (m_animSlowDownDuration / 2) / duration) - relativeElapsedTime) * duration, waitTime);
    //            }


    //            //HITBOXES On and Off
    //            if (processedData.AnimationData.hitBoxActiveData.Count != 0)
    //            {
    //                int activeDataIndex = 0;
    //                float timetillNextHitBoxChange = timeTillEnd;
    //                foreach (AnimationData.HitBoxActiveData hitActiveData in processedData.AnimationData.hitBoxActiveData)
    //                {
    //                    if (relativeElapsedTime < hitActiveData.activeTime.x)   //before Hitbox activation
    //                    { timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.x - relativeElapsedTime) * duration, timetillNextHitBoxChange);}
    //                    else if (relativeElapsedTime < hitActiveData.activeTime.y) //while Hitbox activation
    //                    {
    //                        timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.y - relativeElapsedTime) * duration, timetillNextHitBoxChange);
    //                        if (!activeHitBoxActiveDataList.Contains(activeDataIndex))
    //                        {
    //                            if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null)  m_characterStatus.HitBoxManagerWeapon.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
    //                            if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
    //                            activeHitBoxActiveDataList.Add(activeDataIndex);
    //                        }
    //                    }
    //                    else if (relativeElapsedTime >= hitActiveData.activeTime.y)//after Hitbox activation
    //                    {
    //                        if (activeHitBoxActiveDataList.Contains(activeDataIndex))
    //                        {
    //                            if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) m_characterStatus.HitBoxManagerWeapon.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
    //                            if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) m_characterStatus.HitBoxManagerShield.DeactivateHitboxCollection(hitActiveData.CollectionRefNumber);
    //                            activeHitBoxActiveDataList.Remove(activeDataIndex);
    //                        }
    //                    }
    //                    activeDataIndex++;
    //                }
    //                waitTime = Mathf.Min(timetillNextHitBoxChange, waitTime);
    //                //Debug.Log(relativeElapsedTime + timetillNextHitBoxChange / duration);
    //            }


    //            //STARTEND VALUES
    //            foreach (var rangeData in processedData.RangeValuesList)
    //            {
    //                float activeFactor = relativeElapsedTime >= rangeData.startEnd.x && relativeElapsedTime < rangeData.startEnd.y ? 1 : 0;
    //                float valueInRange = rangeData.value * activeFactor;

    //                //this calculates how long to wait for the next necessary canculation
    //                float waitForTimeByRangeValues = timeTillEnd;
    //                if (relativeElapsedTime < rangeData.startEnd.x) { waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime;}//wait till range start
    //                else if (relativeElapsedTime < rangeData.startEnd.y) { waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime;  }//wait till range end

    //                waitTime = Math.Min(waitTime, waitForTimeByRangeValues);
    //                SetValueByName(rangeData.name, valueInRange);
    //            }

    //            //CURVE VALUES
    //            foreach (var curveData in processedData.CurveValuesList)
    //            {
    //                float activeFactor = relativeElapsedTime > curveData.startEnd.x && relativeElapsedTime < curveData.startEnd.y ? 1 : 0;
    //                float curveValue = curveData.value * curveData.curve.Evaluate(Mathf.InverseLerp(curveData.startEnd.x, curveData.startEnd.y, relativeElapsedTime)) * activeFactor;

    //                //this calculates how long to wait for the next necessary canculation
    //                float waitForTimeByCurveValues = timeSteps;
    //                if (relativeElapsedTime < curveData.startEnd.x) waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.x * duration) - elapsedTime); //wait till range start or timeToWait
    //                else if (relativeElapsedTime < curveData.startEnd.y) waitForTimeByCurveValues = Mathf.Min(waitForTimeByCurveValues, (curveData.startEnd.y * duration) - elapsedTime); //wait till range end or timeToWait                                                                        //wait till timeToWait

    //                waitTime = Mathf.Min(waitTime, waitForTimeByCurveValues);
    //                SetValueByName(curveData.name, curveValue);
    //            }

    //            m_actionTimeTillNextChange = waitTime;
    //        }
    //        //if (processedData.AnimationData.AnimationMovementData.ActionDescription == "long") Debug.Log("U");

    //        yield return null;

    //        delayByMidAir += Time.deltaTime * (1 - m_animationSpeed);
    //        elapsedTime = Time.time - (startTime + delayByMidAir); // time must be added after the first wait

    //        m_actionTimeTillNextChange -= Time.deltaTime * m_animationSpeed;
    //        //Debug.Log(m_animationSpeed);
    //        //Debug.Log(elapsedTime);
    //    }

    //    //End of Action
    //    //bool isRunning = m_isHoldRunning && m_inputStrenght != 0 && !m_isWalkingLocked;
    //    //if (isRunning) SetNextPossibleAttacks(currentAction: Running);
    //    SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
    //    SetNextPossibleShieldActions(currentAction: AnimationTypes.Reset);

    //    EndAction();

    //    //HERE NOTHING MORE

    //}

    //private void EndAction()
    //{
    //    m_animator.SetTrigger("EndActionTrigger");

    //    //reset influence of Action
    //    m_actionInfluenceOverMoveDirection = 0;
    //    m_actionInfluenceOverMoveSpeed = 0;
    //    m_actionInfluenceOverMoveAcceleration = 0;
    //    m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;
    //    m_actionInfluenceOverTurningStrenght = 0;
    //    m_actionInfluenceOverMaxTurningSpeed = 0;

    //    //reset values
    //    m_disableSidewardMovement = false;
    //    m_actionTargetRelations = 0;
    //    m_actionTurningRelations = 0;
    //    m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
    //    if (m_currentActionAndLayer.y >= 1) SetUpperBodyAnimation(AnimationTypes.Empty, 0, m_nextUpperBodyCrossfadeOutTime); 
    //    m_currentActionAndLayer = c_emptyAction;
    //    //IsRunning = (m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked);  //this is funny, many stab attacks haha
    //    m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
    //    m_isTurningApplied = false;
    //    m_characterStatus.ContinueEnergyRegenerationInTime();
    //    m_currentActionAnimData = null;
    //    if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) { m_currentWeaponAttackData = null; m_characterStatus.HitBoxManagerWeapon.DeactivateAllHitboxCollections();}
    //    if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) { m_currentShieldActionData = null; m_characterStatus.HitBoxManagerShield.DeactivateAllHitboxCollections();}


    //    if (m_isHoldShielding) IsShielding = true;
    //    SetBodyLookAtTarget(m_target);
    //    SetLookAtForward(!m_isShielding ? false : m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.useLookAtForwardData, m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.lookAtData);


    //    //End Coroutines
    //    if (m_ActionCoroutine != null)
    //    {
    //        StopCoroutine(m_ActionCoroutine);
    //        m_ActionCoroutine = null;
    //    }
    //    if (m_actionChangesInterruptabilityCoroutine != null) 
    //    {
    //        StopCoroutine(m_actionChangesInterruptabilityCoroutine);
    //        m_actionChangesInterruptabilityCoroutine = null;
    //    }
    //    if (m_actionPayCostCouroutine != null)
    //    {
    //        StopCoroutine(m_actionPayCostCouroutine);
    //        m_actionPayCostCouroutine = null;
    //    }
    //    if (m_isMidAirPause)
    //    {
    //        m_isMidAirPause = false;
    //        m_animationSpeed = 1;
    //        m_animator.speed = m_animationSpeed;
    //    }
    //    if (m_actionPauseCoroutine != null)
    //    {
    //        StopCoroutine(m_actionPauseCoroutine);
    //        m_actionPauseCoroutine = null;
    //    }
    //    if (m_gravityPauseCoroutine != null)
    //    {
    //        StopCoroutine(m_gravityPauseCoroutine);
    //        Gravity = m_originalGravity;
    //        m_gravityPauseCoroutine = null;
    //    }

    //    //Set Values
    //    m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
    //    if (!m_isFreelyMoving) SetFacingDirectionType(); else m_orientation = Orientation.Forward; //just a reminder, in the past here was a issue, but it should not anymore
    //    m_desiredFacingRotationDirInWS = !m_isStandingStill ? OrientationRotation() * m_inputDirInWS : transform.forward;

    //    m_playerInputManager.RecallLatestBufferedInput();
    //}
    //#endregion

    private void ResetNextAttack()
    {
        //End of Action
        //bool isRunning = m_isHoldRunning && m_inputStrenght != 0 && !m_isWalkingLocked;
        //if (isRunning) SetNextPossibleAttacks(currentAction: Running);
        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        SetNextPossibleShieldActions(currentAction: AnimationTypes.Reset);
    }

    private void EndActionResetValues()
    {
        m_animator.SetTrigger("EndActionTrigger");

        m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
        if (m_currentActionAndLayer.y >= 1) SetUpperBodyAnimation(AnimationTypes.Empty, 0, m_nextUpperBodyCrossfadeOutTime);
        m_currentActionAndLayer = c_emptyAction;
        //IsRunning = (m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked);  //this is funny, many stab attacks haha
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
        m_isTurningApplied = false;
        m_characterStatus.ContinueEnergyRegenerationInTime();
        m_currentActionAnimData = null;
        if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) { m_currentWeaponAttackData = null; m_characterStatus.HitBoxManagerWeapon.DeactivateAllHitboxCollections(); }
        if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) { m_currentShieldActionData = null; m_characterStatus.HitBoxManagerShield.DeactivateAllHitboxCollections(); }


        if (m_isHoldShielding) IsShielding = true;
        SetBodyLookAtTarget(m_target);
        SetLookAtForward(!m_isShielding ? false : m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.useLookAtForwardData, m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.lookAtData);


        //if (m_actionChangesInterruptabilityCoroutine != null)
        //{
        //    StopCoroutine(m_actionChangesInterruptabilityCoroutine);
        //    m_actionChangesInterruptabilityCoroutine = null;
        //}
        if (m_actionPayCostCouroutine != null)
        {
            StopCoroutine(m_actionPayCostCouroutine);
            m_actionPayCostCouroutine = null;
        }
        if (m_isMidAirPause)
        {
            m_isMidAirPause = false;
            AnimatorSpeed = 1;
            m_animator.speed = m_animationSpeed;
        }
        if (m_actionPauseCoroutine != null)
        {
            StopCoroutine(m_actionPauseCoroutine);
            m_actionPauseCoroutine = null;
        }
        if (m_gravityPauseCoroutine != null)
        {
            StopCoroutine(m_gravityPauseCoroutine);
            Gravity = m_originalGravity;
            m_gravityPauseCoroutine = null;
        }

        //Set Values
        m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_orientation = Orientation.Forward; //just a reminder, in the past here was a issue, but it should not anymore
        m_desiredFacingRotationDirInWS = !m_isStandingStill ? OrientationRotation() * m_inputDirInWS : transform.forward;

        m_playerInputManager.RecallLatestBufferedInput();
    }





























    #region ANIMATION

    public float m_animSlowDownDuration = 0.2f;
    private IEnumerator SlowDownAnimSpeed()
    {
        float t = m_animSlowDownDuration;

        while ((t > 0 && m_slowDownAnimSpeedCoroutine != null) || t == m_animSlowDownDuration)
        {
            t = Mathf.Max( t - Time.deltaTime, 0);
            //Debug.Log(t);
            AnimatorSpeed = t/ m_animSlowDownDuration;
            m_animator.speed = m_animationSpeed;
            yield return null;
        }
        m_slowDownAnimSpeedCoroutine = null;
    }



    private void SetAnimatorMoveValues()
    {
        float animationDampTime = m_currentActionAndLayer == c_emptyAction ? 0.12f : 0.12f; //smaller is faster transition
        float MoveStrength = m_isRunning ? 2 : m_inputStrenght; //is already snapped in inputmanager
        Vector2 horAndVerMovement = new Vector2(0, 1);

        m_animator.SetFloat("MoveMag", MoveStrength, animationDampTime, Time.deltaTime);

        if (m_orientation == Orientation.Forward) horAndVerMovement = new Vector2(0, 1);
        else if (m_orientation == Orientation.Right) horAndVerMovement = new Vector2(1, 0);
        else if (m_orientation == Orientation.Left) horAndVerMovement = new Vector2(-1, 0);
        else horAndVerMovement = new Vector2(0, -1);

        m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);
        m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);

    }




    private void CheckAnimation()
    {
        if (m_currentActionAndLayer.x != AnimationTypes.Empty && m_currentActionAndLayer.y == 0)
            return;
        
        if (m_isStandingStill )
        {
            if (m_isShielding && m_currentAnimationStates[0] != AnimationTypes.Shielding)
            {
                SetAnimation(AnimationTypes.Shielding, crossFadeDuration: m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeInTime, 0);
                m_nextCrossfadeOutTime = m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeOutTime;
            }
            else if (!m_isShielding && m_currentAnimationStates[0] != AnimationTypes.Idle_1 && !m_isTurningApplied && !m_isSlidingApplied)
                SetAnimation(AnimationTypes.Idle_1, m_nextCrossfadeOutTime, 0);
        }
        else if (!m_isStandingStill && m_currentAnimationStates[0] != AnimationTypes.Locomotion)
            SetAnimation(AnimationTypes.Locomotion, m_nextCrossfadeOutTime, 0, 0.25f);

    }





    private float m_baseCrossFadeDuration = 0.15f;
    private float m_nextCrossfadeOutTime = 0; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim
    private float m_nextUpperBodyCrossfadeOutTime = 0; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim

    private void SetAnimation(int animation,  float crossFadeDuration, int layer, float timeOffset = 0)
    {
        m_animator.CrossFadeInFixedTime(animation, crossFadeDuration, layer, timeOffset);
        m_currentAnimationStates[layer] = animation;
        m_nextCrossfadeOutTime = m_baseCrossFadeDuration; 
        
    }


    private void SetUpperBodyAnimation(int upperBodyAnimation,  int layer, float crossFadeDuration, float timeOffset = 0)
    {
        if (layer == 0 && upperBodyAnimation != AnimationTypes.Empty)
        { Debug.Log("This animationData should have a different animation layer, choose a bodypart beside wholeBody!"); return; }

        for (int i = 1; i < m_currentAnimationStates.Length; i++)
        {
            if (layer == 0 || (i != layer && m_currentAnimationStates[i] != AnimationTypes.Empty))
                SetAnimation(AnimationTypes.Empty, crossFadeDuration, i, timeOffset);
            else if (layer != 0 && i == layer)
                SetAnimation(upperBodyAnimation, crossFadeDuration, i, timeOffset);
        }

        m_nextUpperBodyCrossfadeOutTime = m_baseCrossFadeDuration;
    }

    private void SetDamageAnimation(int upperBodyAnimation, int layer, float crossFadeDuration = 0f)
    {
        m_animator.CrossFadeInFixedTime(upperBodyAnimation, crossFadeDuration, layer);
    }

    #endregion










}
