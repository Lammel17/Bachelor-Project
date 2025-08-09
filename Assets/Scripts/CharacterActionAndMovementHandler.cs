using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using Unity.Collections;
using Unity.Hierarchy;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using Debug = UnityEngine.Debug;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterStatus))]

public class CharacterActionAndMovementHandler : MonoBehaviour
{
    public GameObject testMoveDirection;
    public GameObject testTurningDirection;



    [SerializeField] private CharacterController m_characterController;
    [SerializeField] private CharacterStatus m_characterStatus;
    [SerializeField] private GameObject m_chraracterMesh;
    private FootPlacing m_footPlacing;
    private PlayerCameraHolder m_playerCameraHolder; 
    private PlayerInputManager m_playerInputManager;
    [SerializeField] private Animator m_animator;
    private LookAt m_lookAtScript = null;
    private CharacterMovesetData m_characterMovesetData;
    private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;

    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    private float m_inputFactor = 1f; //should stay 1
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private Vector3 m_turningStrenghtBaseValues = new Vector3(15, 15, 10); //slow, walk, running
    [SerializeField] private float m_maxTurningSpeedBaseValue = 50f;
    [SerializeField] private int m_evadeCosts = 30;
    //private const int m_runningMoveStrenght = 2;
    private Vector3 m_nowMoveDir = Vector3.forward;
    [SerializeField] private float c_gravity = -9.81f;
    [SerializeField][EditorAttributes.ReadOnly] private float m_gravity = -9.81f;
    private float m_velocityThroughGravity;
    private Vector3 m_controllerVelocity;

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
    private float m_animationSpeed = 1; //slow, walk, running

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
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isActionUpperBody = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isWalkingLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isActionLocked = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isHoldShielding = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isShielding = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isGrounded = true;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_isMidAirPause = false;
    [SerializeField][EditorAttributes.ReadOnly] private bool m_equipmentIsReady = true;
    [Space]
    [SerializeField][EditorAttributes.ReadOnly] private int m_currentBaseLayerAnimation;
    [SerializeField][EditorAttributes.ReadOnly] private Vector2 m_currentUpperBodyAnimation; // (AnimHash, Layer)

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private float m_prevInputStrength = 0;
    private float m_prevPrevInputStrength = 0;
    private bool m_isStandingPrev = true;

    private AnimationData m_currentActionAnimData = null;
    private WeaponData.WeaponAttack m_currentWeaponAttackData = null;
    private ShieldData.ShieldAction m_currentShieldActionData = null;


    private Coroutine m_actionChangesInterruptabilityCoroutine;
    private Coroutine m_actionPayCostCouroutine;
    private Coroutine m_actionPauseCoroutine;
    private Coroutine m_gravityPauseCoroutine;
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

    #region PROPERTIES
    public Animator Animator { get => m_animator; }
    public CharacterMovesetData MovesetData { get => m_characterMovesetData; }
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
            else if     (m_isRunning)       { m_speed = m_speedValues.z;    m_turningStrenght = m_turningStrenghtBaseValues.z; }
            else if     (value == 0.5f)     { m_speed = m_speedValues.x;    m_turningStrenght = m_turningStrenghtBaseValues.x; }
            else /*if   (value == 1) */     { m_speed = m_speedValues.y;    m_turningStrenght = m_turningStrenghtBaseValues.y; }
        } 
    } 
    public bool IsGrounded 
    { 
        get => m_isGrounded; 
        set 
        {
            if (value == m_isGrounded)
                return;

            m_isGrounded = value; 
            if (m_isMidAirPause && m_isGrounded)
            {
                m_isMidAirPause = false;
                m_animationSpeed = 1;
                m_animator.speed = m_animationSpeed;
            }
        } 
    }
    public float Gravity 
    { 
        get => m_gravity; 
        set 
        { 
            if (m_gravity == value) 
                return;

            if (m_footPlacing != null && m_gravity != 0 && value == 0) 
                m_footPlacing.SetWeightActive(false); 
            else if (m_footPlacing != null && m_gravity == 0 && value != 0)
                m_footPlacing.SetWeightActive(true);

            m_gravity = value; 
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
            if (!m_isAction || (m_currentActionAnimData != null && m_currentActionAnimData.actionUsesLookAtTargetData)) 
                SetLookAtTarget(m_target);

        } 
    }
    public Vector3 TargetPos { get => Target.position; }
    public Vector3 PlayerToTargetXZVector 
    { 
        get { if (m_target == null) { Debug.Log("No target, so no Direction to Target"); return transform.forward; }; return new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z).normalized; } 
    }
    public bool IsHoldRunning { get => m_isHoldRunning; set { m_isHoldRunning = value; } }
    public bool IsRunning
    {
        get => m_isRunning;
        set 
        {
            if (value == m_isRunning) 
                return;

            m_isRunning = value;
            if (m_isRunning) { SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Running); SetLookAtTarget(null); }
            else if (!m_isRunning && !m_isAction) { SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset); } 
            if (!m_isRunning && !m_isAction && m_target != null) { SetLookAtTarget(m_target); }

            Speed = m_inputStrenght;

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
            }
            else
            {
                m_characterStatus.IsShielding(false);
                SetLookAtForward(false);
            }
        } 
    }
    public Vector3 PreviousMove { get => m_prevMove; }
    public AnimationInterruptableType CurrentInteruptability { get => m_currentInteruptability;  }
    public bool EquipmentIsReady { get => m_equipmentIsReady; }

    #endregion


    void Start()
    {

        if (m_characterController == null) m_characterController = GetComponent<CharacterController>();
        if (m_characterStatus == null) m_characterStatus = GetComponent<CharacterStatus>();
        m_characterMovesetData = m_characterStatus.MovesetData;



        m_chraracterMesh.transform.position = new Vector3(0, -m_characterController.skinWidth, 0);

        m_playerInputManager = PlayerInputManager.Instance;
        m_playerCameraHolder = PlayerCameraHolder.Instance;

        if (TryGetComponent<LookAt>(out LookAt lookAt))
            m_lookAtScript = lookAt;
        if (TryGetComponent<FootPlacing>(out FootPlacing footPlace))
            m_footPlacing = footPlace;

        m_turningStrenght = m_turningStrenghtBaseValues[1];
        m_maxTurningSpeed = m_maxTurningSpeedBaseValue;

        SetNextPossibleWeaponActions();
        SetNextPossibleShieldActions();

        m_currentBaseLayerAnimation = AnimationTypes.Idle_1;
        m_currentUpperBodyAnimation = new Vector2(AnimationTypes.Empty_UpperBody, 0);

        m_gravity = c_gravity;

    }


    public void SetNextPossibleWeaponActions() 
    {
        if (m_characterMovesetData == null || m_characterMovesetData.weapon == null)
            return;
        m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.LightAttack1, m_characterMovesetData.weapon.HeavyAttack1, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);
    }
    public void SetNextPossibleShieldActions()
    {
        if (m_characterMovesetData == null || m_characterMovesetData.shield == null)
            return;
        m_nextPossibleShieldActions = new NextPossibleShieldActions(m_characterMovesetData.shield.shieldIdle, m_characterMovesetData.shield.shieldingUpperBody, m_characterMovesetData.shield.ShiledSpecialLight1, m_characterMovesetData.shield.ShiledSpecialHeavy1);
    }








    void Update()
    {

        SetValues();
        TriggerTurning();

        testMoveDirection.transform.localScale = new Vector3(0.07f, 0.07f, Mathf.Min(Mathf.Max(m_inputStrenght, 0.2f), 1.5f));

        if ((int)m_actionTurningRelations != 1/*TurningDirFollowsMoveDir*/)
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

        m_prevPrevInputStrength = m_prevInputStrength; 
        m_prevInputStrength = m_inputStrenght;


        testMoveDirection.transform.position = new Vector3(transform.position.x, testMoveDirection.transform.position.y, transform.position.z);
        testTurningDirection.transform.position = new Vector3(transform.position.x, testTurningDirection.transform.position.y, transform.position.z);


    }


    #region INITIAL FRAME VALUES

    private Coroutine SwitchFreelyMoving;
    bool m_isAboutSwitchDirectionType = false;

    private void SetValues() //moveDir, threshholds, TargetDist, etc
    {
        IsGrounded = m_characterController.isGrounded;

        m_controllerVelocity = m_characterController.velocity;

        //StandingStill
        m_isStandingPrev = m_isStandingStill;
        m_isStandingStill = ((m_isWalkingLocked == false && m_inputStrenght == 0) || m_isWalkingLocked == true);

        //Running
        IsRunning = (m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked) && m_characterStatus.CheckIfCanConsumeConstantEnergy();
        if (m_isRunning) m_characterStatus.ExpendEnergyPoints(30 * Time.deltaTime);

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

            if (!(m_isAction && !m_isActionUpperBody)) //if action started, the facingType should stay, since its needed for the SetAnimatorMoveValues()
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

    #endregion




    #region TRIGGER ACTIONS

    public void TriggerDamage()
    {
        //does not Affect anything
        SetDamageAnimation(AnimationTypes.Get_Hit, 5, 0);
    }
    public void TriggerStun()
    {
        //cancels animation and stuns for less than a second, caused by amount of poise damage when below 40% poise
        Debug.Log("TriggerStun");
    }
    public void TriggerStagger()
    {
        //cancels animation and stuns for more than a second, cause by depleated poise
        Debug.Log("TriggerStagger");

    }
    public void TriggerFallingOver(Vector3 direction)
    {
        // cancels animation and throws character away
        Debug.Log("TriggerFallingOver");

    }
    public void TriggerShieldDeflect()
    {
        //does not Affect anything
        //SetDamageAnimation(AnimationTypes.Shield_Deflect, 5, 0);
        Debug.Log("TriggerShieldDeflect");

    }
    public void TriggerShieldStun()
    {
        // cancels animation and stuns for less than a second, character is still blocking, caused by amount of energy consumption when below 40% energy
        Debug.Log("TriggerShieldStun");

    }
    public void TriggerShieldBreak()
    {
        // cancels animation and stuns for more than a second, character is not blocking, cause by depleated energy
        Debug.Log("TriggerShieldBreak");

    }
    public void TriggerDie()
    {
        Debug.Log("TriggerDie");

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
        
            m_isTurning = true;
            
            if (m_ActionCoroutine != null)
                EndActionReset();

            m_animator.SetFloat("TurningDir", Mathf.Sign(angleMoveDirToPrevMoveDir));
            m_currentInteruptability = turningInterruptability;

            InitAction(!m_isRunning ? AnimationTypes.Turning : AnimationTypes.Turning_Running, animData.bodyParts, animData);
        }
    }


    public void TriggerReadyOrRemoveEquipment()
    {
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) return;

        int animHashWeapon = 0;
        int animHashShield = 0;
        AnimationData animDataWeapon = null;
        AnimationData animDataShield = null;
        if (m_equipmentIsReady)
        {
            animDataWeapon = m_characterMovesetData.weapon.RemoveWeapon;
            animDataShield =  m_characterMovesetData.shield.RemoveShield;
            animHashWeapon = AnimationTypes.Ready_Weapon;
            animHashShield = AnimationTypes.Ready_Shield;
        }
        else
        {
            animDataWeapon = m_characterMovesetData.weapon.ReadyWeapon;
            animDataShield = m_characterMovesetData.shield.ReadyShield;
            animHashWeapon = AnimationTypes.Remove_Weapon;
            animHashShield = AnimationTypes.Remove_Shield;
        }
        if (animDataWeapon == null) { Debug.Log("MISSING ANIMATION DATA of a ReadyOrRemoveWeapon"); return; }
        if (animDataShield == null) { Debug.Log("MISSING ANIMATION DATA of a ReadyOrRemoveShield"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action readyOrRemoveEffect = EquipmentHandler.Instance == null ? null : () => { m_equipmentIsReady = !m_equipmentIsReady; EquipmentHandler.Instance.ReadyOrRemoveEquipment(m_equipmentIsReady); };

        if(animDataWeapon.animationClip.length >= animDataShield.animationClip.length)
        {
            InitAction(animHashWeapon, animDataWeapon.bodyParts, animDataWeapon, Effect: readyOrRemoveEffect);
            SetUpperBodyAnimation(animHashShield, (int)AnimationData.BodyParts.LeftArm, animDataShield.crossfadeInTime, exeptionForMultipleLayerAnimation: true);
        }
        else
        {
            InitAction(animHashShield, animDataShield.bodyParts, animDataShield, Effect: readyOrRemoveEffect);
            SetUpperBodyAnimation(animHashWeapon, (int)AnimationData.BodyParts.RightArm, animDataWeapon.crossfadeInTime, exeptionForMultipleLayerAnimation: true);
        }

    }


    public void TriggerSwitchWeapon()
    {
        if (!m_equipmentIsReady)
        {
            EquipmentHandler.Instance.SwitchActiveWeapon();
            return;
        }

        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) return;

        AnimationData animData = m_characterMovesetData.weapon.SwitchWeapon;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA of SwitchWeapon"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action switchEffect = EquipmentHandler.Instance == null ? null : () => { EquipmentHandler.Instance.SwitchActiveWeapon(); };

        InitAction(AnimationTypes.Switch_Weapon, animData.bodyParts, animData, Effect: switchEffect);

    }

    public void TriggerSwitchShield()
    {
        if (!m_equipmentIsReady)
        {
            EquipmentHandler.Instance.SwitchActiveShield();
            return;
        }

        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA"); return; }

        AnimationInterruptableType switchEquipmentInterruptability = AnimationInterruptableType.Easily_Interruptable;
        if ((int)m_currentInteruptability >= (int)switchEquipmentInterruptability) return;

        AnimationData animData = m_characterMovesetData.shield.SwitchShield;
        if (animData == null) { Debug.Log("MISSING ANIMATION DATA of SwitchShield"); return; }

        m_currentInteruptability = switchEquipmentInterruptability;

        Action switchEffect = EquipmentHandler.Instance == null ? null : () => { EquipmentHandler.Instance.SwitchActiveShield(); };

        InitAction(AnimationTypes.Switch_Shield, animData.bodyParts, animData, Effect: switchEffect);

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
        if (m_facingDirectionType == Direction.Forward)             { animData = m_characterMovesetData.evadeForward; animHash = AnimationTypes.Evade_Forward; }
        else if (m_facingDirectionType == Direction.Left)           { animData = m_characterMovesetData.evadeLeft; animHash = AnimationTypes.Evade_Left; }
        else if (m_facingDirectionType == Direction.Right)          { animData = m_characterMovesetData.evadeRight; animHash = AnimationTypes.Evade_Right; }
        else                                                        { animData = m_characterMovesetData.evadeBackwards; animHash = AnimationTypes.Evade_Backwards; }

        if (animData == null) { Debug.Log("MISSING ANIMATION DATA"); return; }

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();
        
        SetNextPossibleWeaponAttacks(currentAction: animHash);

        m_currentInteruptability = evadeInterruptability;

        InitAction(animHash, animData.bodyParts, animData, m_evadeCosts);

        }

    public void TriggerLightAttack()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.light; 
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return;}
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Light Attack"); return; }

        AnimationInterruptableType lightAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)lightAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

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
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.specialLight;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialLightAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialLightAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

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
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.heavy;
        if (thisAction == null) { Debug.Log("MISSING ATTACK DATA of a Heavy Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Heavy Attack"); return; }

        AnimationInterruptableType heavyAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)heavyAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

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
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        WeaponData.WeaponAttack thisAction = m_nextPossibleWeaponActions.specialHeavy;
        if (thisAction == null) { Debug.Log("MISSING ATTACK DATA of a Heavy Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Heavy Attack"); return; }

        AnimationInterruptableType specialHeavyAttackInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialHeavyAttackInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

        SetNextPossibleWeaponAttacks(thisAction);

        m_currentInteruptability = specialHeavyAttackInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentWeaponAttackData = thisAction;
        InitAction(thisAction.AttackHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);

    }

    public void TriggerShielding(bool isHoldShielding)
    {
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }
        
        IsHoldShielding = isHoldShielding;

        if (!m_isHoldShielding)
        {
            IsShielding = false;
            return;
        }

        AnimationInterruptableType shieldingInterruptabilityLimit = AnimationInterruptableType.Hardly_Interruptable;
        if ((int)m_currentInteruptability >= (int)shieldingInterruptabilityLimit) return;

        if (m_ActionCoroutine != null && !m_isTurning) //stop current animation if its not those: turning
            EndActionReset();

        IsShielding = true;

    }

    public void TriggerShieldSpecialLight()
    {
        if (m_isActionLocked) return;
        if (!m_equipmentIsReady) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        ShieldData.ShieldAction thisAction = m_nextPossibleShieldActions.specialShieldLight;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialShieldLightActionInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialShieldLightActionInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;

        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

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
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Light Attack"); return; }

        ShieldData.ShieldAction thisAction = m_nextPossibleShieldActions.specialShieldHeavy;
        if (thisAction == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Special Light Attack"); return; }

        AnimationInterruptableType specialShieldHeavyActionInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;
        if ((int)m_currentInteruptability >= (int)specialShieldHeavyActionInterruptability) return;

        if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.SpecialEnergyCost)) return;


        ///////////////////////////////////////////////////// starting here the aniamtion is definitely set to begin

        if (m_ActionCoroutine != null)
            EndActionReset();

        SetNextPossibleShieldActions(thisAction);

        m_currentInteruptability = specialShieldHeavyActionInterruptability;
        //m_actionDirectionConstrain = FacingDirectionTypeConstrains.LockedByAction;

        m_currentShieldActionData = thisAction;
        InitAction(thisAction.ActionkHash, thisAction.AnimData.bodyParts, thisAction.AnimData, thisAction.EnergyCost, thisAction.SpecialEnergyCost);
    }

    public void TriggerItemUse()
    {
        if (m_isActionLocked) return;
        if (m_characterMovesetData == null) { Debug.Log("MISSING Moveset DATA of a Heavy Attack"); return; }

        if (m_characterMovesetData.item == null) { Debug.Log("MISSING Item DATA"); return; }
        ItemData.ItemAction thisAction = m_characterMovesetData.item.ItemUse;
        if (thisAction == null) { Debug.Log("MISSING ACTION DATA of a Item Action"); return; }
        if (thisAction.AnimData == null) { Debug.Log("MISSING ANIMATION DATA of a Item Action"); return; }

        AnimationInterruptableType itemUseInterruptability = thisAction.AnimData.CustomInterruptability == AnimationInterruptableType.SetByButton ? AnimationInterruptableType.Not_Interruptable : thisAction.AnimData.CustomInterruptability;

        if ((int)m_currentInteruptability >= (int)itemUseInterruptability) return;

        //if (!m_characterStatus.CheckIfCanExpendEnergy()) return;
        //if (!m_characterStatus.CheckIfCanExpendSpecialEnergy(thisAction.EnergyCost)) return;

        if (m_ActionCoroutine != null && (!m_isTurning || thisAction.AnimData.bodyParts == AnimationData.BodyParts.WholeBody))
            EndActionReset();

        m_currentInteruptability = itemUseInterruptability;

        InitAction(AnimationTypes.Use_Item, thisAction.AnimData.bodyParts, thisAction.AnimData);
    }

    #endregion




    #region SET ACTIONS
    private void InitAction(int animHash, AnimationData.BodyParts animLayer, AnimationData animData, int staminaCost = 0, int specialEnergyCost = 0, Action Effect = null)
    {
        if (animLayer == AnimationData.BodyParts.WholeBody)
            InitBaseAction(animHash, animData, staminaCost, specialEnergyCost, effect: Effect);
        else if (animLayer == AnimationData.BodyParts.UpperBody)
            InitActionUpperBody(animHash, animData, 1, effect: Effect);
        else if (animLayer == AnimationData.BodyParts.Arms)
            InitActionUpperBody(animHash, animData, 2, effect: Effect);
    }

    private void InitBaseAction(int animationHash, AnimationData animData, int staminaCost = 0, int specialEnergyCost = 0, Action effect = null)
    {
        m_isAction = true;
        IsRunning = false;

        if (!m_isTurning) //stop upperbody animations
        {
            IsShielding = false;
            SetUpperBodyAnimation(AnimationTypes.Empty_UpperBody, 0, crossFadeDuration: 0.1f);
        }

        SetLookAtTarget(animData.actionUsesLookAtTargetData ? m_target : null);

        SetAnimation(animationHash, animData.crossfadeInTime); //this sets and activates the animation with given crossfadeInTime
        m_nextCrossfadeOutTime = animData.crossfadeOutTime; //this is set and stored for end of action for the case the animation fades out normally and is not interrupted by an action with its own fadeInTime

        SetValues(); //needed, because what if it jumps from one action directly into another

        float animationDuration = animData.animationClip.length;

        m_characterStatus.PauseEnergyRegenerationByAction();
        
        //effects like pay stamina cost at that moment
        List<Action> actionList = new List<Action>();
        if (staminaCost != 0)
        {
            Action payActionCostsAction = () => { m_characterStatus.ExpendEnergyPoints(staminaCost); m_characterStatus.ExpendSpecialEnergyPoints(specialEnergyCost); m_actionPayCostCouroutine = null; };
            actionList.Add(payActionCostsAction);
        }
        if (effect != null)
            actionList.Add(effect);


        if (animData.IsPausingGravity && animData.PauseGravityTime != Vector2.zero)
        {
            Action noGravity = () =>
            {
                Gravity = 0;
                m_gravityPauseCoroutine = null;
                m_footPlacing.SetWeightActive(false);

                Action yesGravity = () => 
                {
                    Gravity = c_gravity;
                    m_gravityPauseCoroutine = null;
                };
                m_gravityPauseCoroutine = StartCoroutine(UtilityFunctions.Wait(animationDuration * animData.PauseGravityTime.y, yesGravity));
            };
            m_gravityPauseCoroutine = StartCoroutine(UtilityFunctions.Wait(animationDuration * animData.PauseGravityTime.x, noGravity));
        }
        if (animData.IsPausingMidAir)
        {
            Action pauseMidAir = () => 
            { 
                if (m_isGrounded) return;
                //Contin m_actionChangesInterruptabilityCoroutine
                m_isMidAirPause = true;
                m_animationSpeed = 0;
                m_animator.speed = m_animationSpeed;
                m_actionPauseCoroutine = null;
            };
            m_actionPauseCoroutine = StartCoroutine(UtilityFunctions.Wait(animationDuration * animData.PauseMidAirTime, pauseMidAir));
        }

        SetActionValues(animData, actionList.Count == 0 ? null : actionList);
    }

    private void InitActionUpperBody(int animationHash, AnimationData animData, int layer, int staminaCost = 0, int specialEnergyCost = 0, Action effect = null)
    {
        m_isAction = true;
        m_isActionUpperBody = true;
        IsRunning = false;

        SetLookAtTarget(animData.actionUsesLookAtTargetData ? m_target : null);

        if (animData.useLookAtForwardData)
            SetLookAtForward(true, animData.lookAtData);

        //SetLookAtTarget(null); //????? Depends on animation and if AddTurning

        SetUpperBodyAnimation(animationHash, layer, animData.crossfadeInTime); //this sets and activates the animation with given crossfadeInTime
        m_nextUpperBodyCrossfadeOutTime = animData.crossfadeOutTime; //this is set and stored for end of action for the case the animation fades out normally and is not interrupted by an action with its own fadeInTime

        SetValues(); //needed, because what if it jumps from one action directly into another

        m_characterStatus.PauseEnergyRegenerationByAction();

        //effects like pay stamina cost at that moment
        List<Action> actionList = new List<Action>();
        if (staminaCost != 0)
        {
            Action payActionCostsAction = () => { m_characterStatus.ExpendEnergyPoints(staminaCost); m_characterStatus.ExpendSpecialEnergyPoints(specialEnergyCost); m_actionPayCostCouroutine = null; };
            actionList.Add(payActionCostsAction);
        }
        if (effect != null)
            actionList.Add(effect);

        SetActionValues(animData, actionList.Count == 0 ? null : actionList);
    }

    private void SetNextPossibleWeaponAttacks(WeaponData.WeaponAttack currentAttackData = null, int currentAction = 0)
    {
        if (currentAction == AnimationTypes.Evade_Forward || currentAction == AnimationTypes.Evade_Left || currentAction == AnimationTypes.Evade_Right || currentAction == AnimationTypes.Evade_Backwards)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.EvadeLightAttack, m_characterMovesetData.weapon.EvadeHeavyAttack, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == AnimationTypes.Running)
            m_nextPossibleWeaponActions = new NextPossibleWeaponActions(m_characterMovesetData.weapon.SprintLightAttack, m_characterMovesetData.weapon.SprintHeavyAttack, m_characterMovesetData.weapon.SpecialLightAttack1, m_characterMovesetData.weapon.SpecialHeavyAttack1);

        else if (currentAction == AnimationTypes.Reset)
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
            //Debug.Log("Warning: Next Possible Light Attack in line has no AnimationData, so the next will be the first one again");
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
            //Debug.Log("Warning: Next Possible Heavy Attack in line has no AnimationData, so the next will be the first one again");
            return m_characterMovesetData.weapon.HeavyAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialLight(WeaponData.LightAttackSpecial specialLight)
        {
            switch (specialLight)
            {
                case WeaponData.LightAttackSpecial.Special_Light_Attack_1: if (m_characterMovesetData.weapon.SpecialLightAttack1.AnimData != null) return m_characterMovesetData.weapon.SpecialLightAttack1; break;
                case WeaponData.LightAttackSpecial.Special_Light_Attack_2: if (m_characterMovesetData.weapon.SpecialLightAttack2.AnimData != null) return m_characterMovesetData.weapon.SpecialLightAttack2; break;
            }
            //Debug.Log("Warning: Next Possible Special Light Attack in line has no AnimationData, so the next will be the first one again");
            return m_characterMovesetData.weapon.SpecialLightAttack1;
        }
        WeaponData.WeaponAttack GetNextAttackSpecialHeavy(WeaponData.HeavyAttackSpecial specialHeavy)
        {
            switch (specialHeavy)
            {
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_1: if (m_characterMovesetData.weapon.SpecialHeavyAttack1.AnimData != null) return m_characterMovesetData.weapon.SpecialHeavyAttack1; break;
                case WeaponData.HeavyAttackSpecial.Special_Heavy_Attack_2: if (m_characterMovesetData.weapon.SpecialHeavyAttack2.AnimData != null) return m_characterMovesetData.weapon.SpecialHeavyAttack2; break;
            }
            //Debug.Log("Warning: Next Possible Special Heavy Attack in line has no AnimationData, so the next will be the first one again");
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
    private void SetNextPossibleShieldActions(ShieldData.ShieldAction currentActionData = null, int currentAction = 0)
    {
        if (currentAction == AnimationTypes.Reset)
            m_nextPossibleShieldActions = new NextPossibleShieldActions(m_characterMovesetData.shield.shieldIdle, m_characterMovesetData.shield.shieldingUpperBody, m_characterMovesetData.shield.ShiledSpecialLight1, m_characterMovesetData.shield.ShiledSpecialHeavy1);

        else if (currentActionData != null)
            m_nextPossibleShieldActions = new NextPossibleShieldActions(m_characterMovesetData.shield.shieldIdle, m_characterMovesetData.shield.shieldingUpperBody, GetNextShieldSpecialLight(currentActionData.nextSpecialLight), GetNextShieldSpecialHeavy(currentActionData.nextSpecialHeavy));

        ShieldData.ShieldAction GetNextShieldSpecialLight(ShieldData.ShieldSpecialLight specialLight)
        {
            switch (specialLight)
            {
                case ShieldData.ShieldSpecialLight.Shield_Special_Light_Action_1: if (m_characterMovesetData.shield.ShiledSpecialLight1.AnimData != null) return m_characterMovesetData.shield.ShiledSpecialLight1; break;
                case ShieldData.ShieldSpecialLight.Shield_Special_Light_Action_2: if (m_characterMovesetData.shield.ShiledSpecialLight2.AnimData != null) return m_characterMovesetData.shield.ShiledSpecialLight2; break;
            }
            //Debug.Log("Warning: Next Possible Special Light Attack in line has no AnimationData, so the next will be the first one again");
            return m_characterMovesetData.shield.ShiledSpecialLight1;
        }
        ShieldData.ShieldAction GetNextShieldSpecialHeavy(ShieldData.ShieldSpecialHeavy specialHeavy)
        {
            switch (specialHeavy)
            {
                case ShieldData.ShieldSpecialHeavy.Shield_Special_Heavy_Action_1: if (m_characterMovesetData.shield.ShiledSpecialHeavy1.AnimData != null) return m_characterMovesetData.shield.ShiledSpecialHeavy1; break;
                case ShieldData.ShieldSpecialHeavy.Shield_Special_Heavy_Action_2: if (m_characterMovesetData.shield.ShiledSpecialHeavy2.AnimData != null) return m_characterMovesetData.shield.ShiledSpecialHeavy2; break;
            }
            //Debug.Log("Warning: Next Possible Special Heavy Attack in line has no AnimationData, so the next will be the first one again");
            return m_characterMovesetData.shield.ShiledSpecialHeavy1;
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








    #region MOVING AND ROTATING

    private void SetLookAtTarget(Transform transform)
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

        if (m_gravity == 0)
            m_velocityThroughGravity = 0;
        else if (m_characterController.isGrounded && m_velocityThroughGravity < 0)
            m_velocityThroughGravity = -2f; // small downward force to keep grounded
        m_velocityThroughGravity += m_gravity * Time.deltaTime;
        Vector3 gravity = new Vector3(0, m_velocityThroughGravity, 0);

        m_characterController.Move((nowMove + gravity) * Time.deltaTime);
        m_prevMove = nowMove;



        if (nowMove != Vector3.zero) testMoveDirection.transform.rotation = Quaternion.LookRotation(nowMove, Vector3.up);
        else testMoveDirection.transform.localScale = new Vector3(0.5f, 0.07f, Mathf.Min(Mathf.Max(nowSpeed/2, 0.2f), 1.5f));


    }


    #endregion















    #region ACTION CALCULATIONS

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



    private void SetActionValues(AnimationData animData, List<Action> effectList = null)
    {
        AnimationMovementData animMoveData = animData.AnimationMovementData;
        float animationDuration = animData.animationClip.length;
        float crossfadeOutTime = animData.crossfadeOutTime;
        float crossfadeStartBeforeEndTime = Mathf.Max(0, 1f, animData.crossfadeBeginn);

        if (m_ActionCoroutine != null)
        {
            StopCoroutine(m_ActionCoroutine);
            m_ActionCoroutine = null;
        }

        List<ProcessedAnimationMovementData.DataCurves> CurveValuesList = new List<ProcessedAnimationMovementData.DataCurves>(); 
        List<ProcessedAnimationMovementData.DataStartEnd> RangeValuesList = new List<ProcessedAnimationMovementData.DataStartEnd>(); 

        if (animMoveData == null)
        {
            Debug.Log("ANIMATION_Moveset_DATA IS NULL");
            animMoveData = m_characterMovesetData.emptyFallbackAnimation.AnimationMovementData;
        }


        int moveDirPredefinition = (int)animMoveData.moveDirPredefinition;
        int turningDirPredefinition = (int)animMoveData.turningDirPredefinition;
        float startMoveInfluence = animMoveData.moveInfluence  == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;  
        float startTurningInfluence = animMoveData.turningInfluence == AnimationMovementData.InfluenceValuePredefinitions.NoInputInfluence ? 1 : 0;

        m_actionTargetRelations = animMoveData.targetRelations;
        m_actionTurningRelations = animMoveData.turningRelations;
        m_isAdditionalRotationForbidden = animMoveData.forbidAdditinalRotation;


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



        foreach (var value in animMoveData.variableValue)
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


        m_currentActionAnimData = animData;
        ProcessedAnimationMovementData processedData = new ProcessedAnimationMovementData(RangeValuesList, CurveValuesList, animData, effectList); //This could be saved somewhere in future!

        m_ActionCoroutine = StartCoroutine(PerformAction(processedData));

    }


    private float m_actionTimeTillNextChange = 0f;
    private IEnumerator PerformAction(ProcessedAnimationMovementData processedData)
    {
        float elapsedTime = 0;
        float startTime = Time.time;
        float timeSteps = processedData.AnimationData.AnimationMovementData == null ? 0.05f : processedData.AnimationData.AnimationMovementData.timeStepsForCurves;
        float delayByMidAir = 0;

        float duration = processedData.AnimationData.animationClip.length; //what about blendtrees, do they affect it?

        DamageData actionDamageData = null;
        List<int> activeHitBoxActiveDataList = new List<int>();
        if (processedData.AnimationData.hitBoxActiveData.Count != 0)
            actionDamageData = m_currentWeaponAttackData != null ? m_characterStatus.GetActionDamageData(m_currentWeaponAttackData, transform.forward, m_characterMovesetData.weapon.BasePhysicalType) 
                                                                 : m_characterStatus.GetActionDamageData(m_currentShieldActionData, transform.forward, m_characterMovesetData.shield.PhysicalType);

        void SetValueByName(ProcessedAnimationMovementData.ValueName name, float newValue)
        {
            switch (name)
            {
                case ProcessedAnimationMovementData.ValueName.Move_Direction_Angle:                     
                    m_directionByAction = Quaternion.Euler(0, newValue, 0) * m_directionByActionBaseValue;
                    break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Direction_Angle:         m_actionInfluenceOverMoveDirection                      = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Speed:                               m_speedByAction                                         = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Speed:                   m_actionInfluenceOverMoveSpeed                          = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Move_Acceleration:                        m_moveAccelerationByAction                              = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Move_Acceleration:            m_actionInfluenceOverMoveAcceleration                   = newValue; break;
                
                case ProcessedAnimationMovementData.ValueName.Turning_Direction_Angle:
                    m_desiredFacingRotationDirInWSByAction = Quaternion.Euler(0, newValue, 0) * m_desiredFacingRotationDirInWSByActionBaseValue;
                    break; 
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Direction_Angle:      m_actionInfluenceOverDesiredFacingRotationDirInWS       = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Turning_Strenght:                         m_turningStrenghtByAction                               = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Turning_Strenght:             m_actionInfluenceOverTurningStrenght                    = newValue; break;
                case ProcessedAnimationMovementData.ValueName.Max_Turning_Speed:                        m_maxTurningSpeedByInputByAction                        = newValue; break;
                case ProcessedAnimationMovementData.ValueName.InfluenceOn_Max_Turning_Speed:            m_actionInfluenceOverMaxTurningSpeed                    = newValue; break;
            }
        }


        while (elapsedTime <= duration - processedData.AnimationData.crossfadeBeginn)
        {

            if (m_actionTimeTillNextChange <= 0)
            {
                float timeTillEnd = ((duration - processedData.AnimationData.crossfadeBeginn) - elapsedTime);
                float waitTime = timeTillEnd;
                float relativeElapsedTime = elapsedTime / duration;



                //INTERRUPTABILITY this is if a action is earlier interruptable than the lenght of the animation
                if (m_currentInteruptability != processedData.AnimationData.ChangedInterruptability)
                {
                    float timetillChangeInteruptability = timeTillEnd - processedData.AnimationData.crossfadeBeginn;
                    if (timetillChangeInteruptability <= 0)
                    {
                        m_characterStatus.ContinueEnergyRegenerationInTime();
                        m_currentInteruptability = processedData.AnimationData.ChangedInterruptability;
                        m_actionChangesInterruptabilityCoroutine = null;
                        if (m_playerInputManager.CheckRecallLatestBufferedInput())
                            EndActionReset();
                    }
                    else
                        waitTime = Mathf.Min(timetillChangeInteruptability, waitTime);
                }

                //EFFECT LIST //effects like pay stamina cost or switch weapons
                if (processedData.Effects != null)
                {
                    //float timetillEffectTime = timeTillEnd - duration * (1 - processedData.AnimationData.MainActionMomentTime);
                    float timetillEffectTime = (duration * processedData.AnimationData.MainActionMomentTime) - ((duration - processedData.AnimationData.crossfadeBeginn) - timeTillEnd)  ;
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

                //HITBOXES On and Off
                if (processedData.AnimationData.hitBoxActiveData.Count != 0)
                {
                    int activeDataIndex = 0;
                    float timetillNextHitBoxChange = timeTillEnd;
                    foreach (AnimationData.HitBoxActiveData hitActiveData in processedData.AnimationData.hitBoxActiveData)
                    {
                        if (relativeElapsedTime < hitActiveData.activeTime.x)   //before Hitbox activation
                        { timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.x - relativeElapsedTime) * duration, timetillNextHitBoxChange);}
                        else if (relativeElapsedTime < hitActiveData.activeTime.y) //while Hitbox activation
                        {
                            timetillNextHitBoxChange = Mathf.Min((hitActiveData.activeTime.y - relativeElapsedTime) * duration, timetillNextHitBoxChange);
                            if (!activeHitBoxActiveDataList.Contains(activeDataIndex))
                            {
                                if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null)  m_characterStatus.HitBoxManagerWeapon.ActivateHitboxCollection(hitActiveData.CollectionRefNumber, actionDamageData);
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


                //STARTEND VALUES
                foreach (var rangeData in processedData.RangeValuesList)
                {
                    float activeFactor = relativeElapsedTime >= rangeData.startEnd.x && relativeElapsedTime < rangeData.startEnd.y ? 1 : 0;
                    float valueInRange = rangeData.value * activeFactor;

                    //this calculates how long to wait for the next necessary canculation
                    float waitForTimeByRangeValues = timeTillEnd;
                    if (relativeElapsedTime < rangeData.startEnd.x) waitForTimeByRangeValues = (rangeData.startEnd.x * duration) - elapsedTime; //wait till range start
                    else if (relativeElapsedTime < rangeData.startEnd.y) waitForTimeByRangeValues = (rangeData.startEnd.y * duration) - elapsedTime; //wait till range end

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

            yield return null;

            delayByMidAir += Time.deltaTime * (1 - m_animationSpeed);
            elapsedTime = Time.time - (startTime + delayByMidAir); // time must be added after the first wait

            m_actionTimeTillNextChange -= Time.deltaTime * m_animationSpeed;
            //Debug.Log(m_animationSpeed);
            //Debug.Log(elapsedTime);
        }

        //End of Action
        //bool isRunning = m_isHoldRunning && m_inputStrenght != 0 && !m_isWalkingLocked;
        //if (isRunning) SetNextPossibleAttacks(currentAction: Running);
        SetNextPossibleWeaponAttacks(currentAction: AnimationTypes.Reset);
        SetNextPossibleShieldActions(currentAction: AnimationTypes.Reset);

        EndActionReset();

        //HERE NOTHING MORE

    }

    private void EndActionReset()
    {
        m_animator.SetTrigger("EndActionTrigger");

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
        m_isActionUpperBody = false;
        //IsRunning = (m_isHoldRunning && m_inputStrenght != 0 && !m_isAction && !m_isWalkingLocked);  //this is funny, many stab attacks haha
        m_isFreelyMoving = !m_isLockOn || m_isRunning || m_isStandingStill;
        m_isTurning = false;
        m_characterStatus.ContinueEnergyRegenerationInTime();
        m_currentActionAnimData = null;
        if (m_currentWeaponAttackData != null && m_characterStatus.HitBoxManagerWeapon != null) { m_currentWeaponAttackData = null; m_characterStatus.HitBoxManagerWeapon.DeactivateAllHitboxCollections();}
        if (m_currentShieldActionData != null && m_characterStatus.HitBoxManagerShield != null) { m_currentShieldActionData = null; m_characterStatus.HitBoxManagerShield.DeactivateAllHitboxCollections();}
        

        SetLookAtTarget(m_target);
        SetLookAtForward(false);
        if (m_isHoldShielding) IsShielding = true;

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
        if (m_actionPayCostCouroutine != null)
        {
            StopCoroutine(m_actionPayCostCouroutine);
            m_actionPayCostCouroutine = null;
        }
        if (m_isMidAirPause)
        {
            m_isMidAirPause = false;
            m_animationSpeed = 1;
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
            Gravity = c_gravity;
            m_gravityPauseCoroutine = null;
        }

        //Set Values
        m_inputDirInWS = !m_isStandingStill ? m_cameraYAxisRotationInWS * m_inputDir : transform.forward;
        if (!m_isFreelyMoving) SetFacingDirectionType(); else m_facingDirectionType = Direction.Forward; //just a reminder, in the past here was a issue, but it should not anymore
        m_desiredFacingRotationDirInWS = !m_isStandingStill ? AdditionalFacingRotation() * m_inputDirInWS : transform.forward;

        m_playerInputManager.RecallLatestBufferedInput();
    }

    #endregion




















    #region ANIMATION

    private void SetAnimatorMoveValues()
    {
        float animationDampTime = !m_isAction ? 0.12f : 0.12f; //smaller is faster transition
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




    private void CheckAnimation(bool forceNewAnim = false)
    {
        if (m_isAction && !m_isActionUpperBody)
            if (!forceNewAnim) return;
        
        if (m_isStandingStill)
        {
            if (m_isShielding)
            {
                if (m_currentBaseLayerAnimation != AnimationTypes.Shielding)
                {
                    SetAnimation(AnimationTypes.Shielding, crossFadeDuration: m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeInTime);
                    m_nextCrossfadeOutTime = m_nextPossibleShieldActions.shieldIdle.AnimData.crossfadeOutTime;
                }
            }
            else if (m_currentBaseLayerAnimation != AnimationTypes.Idle_1)
                SetAnimation(AnimationTypes.Idle_1, m_nextCrossfadeOutTime);
        }
        if (!m_isStandingStill && m_currentBaseLayerAnimation != AnimationTypes.Locomotion)
            SetAnimation(AnimationTypes.Locomotion, m_nextCrossfadeOutTime, 0.25f);




        //UpperBody
        if (m_isActionUpperBody)
            return;

        if (m_isShielding)
        {
            if(m_currentUpperBodyAnimation.x != AnimationTypes.Shielding)
            {
                SetUpperBodyAnimation(AnimationTypes.Shielding, (int)m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.bodyParts, crossFadeDuration: m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeInTime);
                m_nextUpperBodyCrossfadeOutTime = m_nextPossibleShieldActions.ShieldingUpperBody.AnimData.crossfadeOutTime;
            }
        }
        else if (!m_isShielding && m_currentUpperBodyAnimation.x != AnimationTypes.Empty_UpperBody)
        {
            SetUpperBodyAnimation(AnimationTypes.Empty_UpperBody, 0, m_nextUpperBodyCrossfadeOutTime); 
        }

    }





    private float m_baseCrossFadeDuration = 0.15f;
    private float m_nextCrossfadeOutTime = 0; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim
    private float m_nextUpperBodyCrossfadeOutTime = 0; //crossfadeOut is set by an animation and stored only for the next crossfadeOut if its not interrupted by an crossfade in of another anim

    private void SetAnimation(int animation, float crossFadeDuration, float timeOffset = 0)
    {
        m_animator.CrossFadeInFixedTime(animation, crossFadeDuration, 0, timeOffset);
        m_currentBaseLayerAnimation = animation;
        m_nextCrossfadeOutTime = m_baseCrossFadeDuration; 
        
    }


    private void SetUpperBodyAnimation(int upperBodyAnimation,  int layer, float crossFadeDuration, float timeOffset = 0, bool exeptionForMultipleLayerAnimation = false)
    {

        if (layer == 0 && upperBodyAnimation != AnimationTypes.Empty_UpperBody)
        { Debug.Log("This animationData should have a different animation layer, choose a bodypart beside wholeBody!"); return; }

        if (layer == 0 && m_currentUpperBodyAnimation.y != 0)
        {
            m_animator.CrossFadeInFixedTime(AnimationTypes.Empty_UpperBody, crossFadeDuration, (int)m_currentUpperBodyAnimation.y, timeOffset);
            m_currentUpperBodyAnimation = new Vector2(AnimationTypes.Empty_UpperBody, 0);
        }
        else if (layer != 0)
        {
        Debug.Log("remember switch weapon spam bug");
            m_animator.CrossFadeInFixedTime(upperBodyAnimation, crossFadeDuration, layer, timeOffset);
            if (!exeptionForMultipleLayerAnimation)
            {
                if ((int)m_currentUpperBodyAnimation.y != 0)
                m_animator.CrossFadeInFixedTime(AnimationTypes.Empty_UpperBody, m_nextUpperBodyCrossfadeOutTime, (int)m_currentUpperBodyAnimation.y, timeOffset);
                m_currentUpperBodyAnimation = new Vector2(upperBodyAnimation, layer);
            }
        }

        m_nextUpperBodyCrossfadeOutTime = m_baseCrossFadeDuration;


    }

    private void SetDamageAnimation(int upperBodyAnimation, int layer, float crossFadeDuration = 0f)
    {
        m_animator.CrossFadeInFixedTime(upperBodyAnimation, crossFadeDuration, layer);
    }

    #endregion










}
