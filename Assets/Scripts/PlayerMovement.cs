using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerCameraHolder m_playerCameraHolder; 
    private PlayerInputManager m_playerInputManager;

    [SerializeField] private Animator m_animator;
    [Space]
    [Header("")] //DONT CHANGE THEM HERE! DO IT IN INSPECTOR!
    private float m_inputFactor = 1f; //should stay 1
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private float m_turningSpeedBaseValue = 45f;
    [SerializeField] private float m_turningAccelerationBaseValue = 10f;
    private const int m_runningMoveStrenght = 2;

    private float m_moveStrenght = 0f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_inputDirInWS = Vector3.forward;
    private Vector3 m_desiredFacingRotationDirInWS = Vector3.forward;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningSpeed;
    private float m_turningAcceleration;
    private float m_speed = 0; //slow, walk, running


    //Values Depending on Camera
    private Quaternion m_cameraYAxisRotationInWS = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private enum Direction { Forward, Sideward, Backward };
    private Direction m_facingDirectionType = Direction.Forward;

    //bools
    private bool m_isLockOn = false;
    private bool m_isRunning = false;
    private bool m_isFreelyMoving = false;
    private bool m_isTurning = false;

    //Coroutines
    private Coroutine m_turningCoroutine;

    //Previous Frame Values
    private Vector3 m_prevMove = Vector3.zero;
    private Vector3 m_prevFacingRotationDir = Vector3.forward;

    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public float MoveStrenght { get => m_moveStrenght; set { if (m_isRunning && value > 0f)  m_moveStrenght = m_runningMoveStrenght; else m_moveStrenght = value; Speed = m_moveStrenght; } } //is already snapped by inputmanager
    public float Speed { get => m_speed; set { m_speed = value == 0 ? 0 : value == 0.5 ? m_speedValues.x : value == 1 ? m_speedValues.y : m_speedValues.z; } } //is already snapped by inputmanager
    public Quaternion CameraYAxisRotation { get => m_cameraYAxisRotationInWS; set => m_cameraYAxisRotationInWS = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target { get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get => Target.position; }
    public Vector3 PlayerToTargetXZVector { get => new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z); }
    public bool IsRunning { get => m_isRunning; set { m_isRunning = value; MoveStrenght = m_playerInputManager.LeftStickSnappedMag; m_animator.SetBool("IsRunning", value); } }


    void Start()
    {
        m_playerInputManager = PlayerInputManager.Instance;
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;

        m_turningSpeed = m_turningSpeedBaseValue;
        m_turningAcceleration = m_turningAccelerationBaseValue;
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
        m_isFreelyMoving = (!m_isLockOn || m_isRunning);

        if (m_isFreelyMoving)
        {
            // InputDirRelativeToCam is relative to cameraRotation, so it should not affect the InputDirRelativeToCam when for example standing still
            m_inputDirInWS = m_moveStrenght > 0 ? m_cameraYAxisRotationInWS * m_inputDir : m_inputDirInWS; 

            m_facingDirectionType = Direction.Forward;
        }
        else
        {
            m_targetDist = (TargetPos - transform.position).magnitude;

            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector);
            Quaternion playerToTargetAndCameraForwardSlerp = Quaternion.Slerp(m_cameraYAxisRotationInWS, playerToTargetLookRotation, 0.5f);
            m_inputDirInWS = playerToTargetAndCameraForwardSlerp * m_inputDir;

            void SetFacingDirectionType()
            {
                //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
                float firstThreshholdAngleMin = 25f;
                float secondThreshholdAngleMin = 110f;
                float distThreshhold = 10f;    //Ab diesem abstand werden die threshholds anfangen zu den Min Threshholds zu lerpen
                float additionalThreshhold = 7f; //Damit es keine flickerzone gibt, wo das überschreiten und unterschreiten des threshholds gleich ist

                if (m_facingDirectionType == Direction.Sideward) additionalThreshhold = -additionalThreshhold;

                m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, m_targetDist / distThreshhold) + additionalThreshhold;
                m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, m_targetDist / distThreshhold) - additionalThreshhold;

                float inputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);
                if (inputAngleToForward < m_forwardSidewardThreshholdAngle) m_facingDirectionType = Direction.Forward;
                else if (inputAngleToForward < m_sidewardBackwardThreshholdAngle) m_facingDirectionType = Direction.Sideward;
                else m_facingDirectionType = Direction.Backward;
            }
            SetFacingDirectionType();
        }
    }


    void TriggerTurning()
    {
        if (m_isTurning)
            return;

        // if the input differs too much, its will trigger an turn. Therefore we need the current and pevious frame latestProcessedDir
        float angleMoveDirToPrevMoveDir = Vector3.Angle(m_desiredFacingRotationDirInWS, m_prevFacingRotationDir);

        if (m_isFreelyMoving && (!m_isRunning && angleMoveDirToPrevMoveDir > 90) || (m_isRunning && angleMoveDirToPrevMoveDir > 150))
        {
            float turnAnimationTurningSpeed = 45f;
            float turningAcceleration = 15f;
            m_turningSpeed = turnAnimationTurningSpeed;
            m_turningAcceleration = turningAcceleration;

            m_animator.SetTrigger("TriggerTurning");
            m_isTurning = true;
            Action resetTurnAction = () =>
            {
                m_isTurning = false; m_animator.SetBool("IsTurningg", false);
                m_turningSpeed = m_turningSpeedBaseValue;
                m_turningAcceleration = m_turningAccelerationBaseValue;
                m_turningCoroutine = null;
            };
            m_turningCoroutine = StartCoroutine(UtilityFunctions.Wait(0.45f, resetTurnAction));
        }
    }


    private void SetAnimatorMoveValues()
    {
        float animationDampTime = 0.15f; //smaller is faster transition
        float VerticalMovement = m_moveStrenght; //is already snapped in inputmanager
        m_animator.SetFloat("MoveMag", VerticalMovement, animationDampTime, Time.deltaTime);

        if (m_isFreelyMoving)
        {
            m_animator.SetFloat("Vertical", 1, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else
        {
            Vector2 horAndVerMovement = Vector2.zero;

            if      (m_facingDirectionType == Direction.Forward)    horAndVerMovement = new Vector2(0, 1);
            else if (m_facingDirectionType == Direction.Sideward)   horAndVerMovement = new Vector2(Mathf.Sign(m_inputDir.x), 0);
            else                                                    horAndVerMovement = new Vector2(0, -1);

            m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);
        }

    }







    private AnimationInterruptableType m_currentInteruptability = AnimationInterruptableType.Always_Interruptable;
    
    //Action Influence Values
    private Vector3 m_directionByAction = Vector3.forward;
    private float m_actionInfluenceOverMoveDirection = 0;

    private float m_speedByAction = 0;
    private float m_actionInfluenceOverMoveSpeed = 0;

    private float m_moveAccelerationByAction = 0;
    private float m_actionInfluenceOverMoveAcceleration = 0;

    private Vector3 m_desiredFacingRotationDirInWSByAction = Vector3.forward;
    private float m_actionInfluenceOverDesiredFacingRotationDirInWS = 0;

    private float m_turningSpeedByAction = 0;
    private float m_actionInfluenceOverTurningSpeed = 0;

    private float m_turningAccelerationByAction = 0;
    private float m_actionInfluenceOverTurningAcceleration = 0;


    public void Evading()
    {
        if(m_currentInteruptability == AnimationInterruptableType.Always_Interruptable)
        m_animator.SetTrigger("TriggerEvade");


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


        Vector3 nowMove =  UtilityFunctions.SmartLerp(m_prevMove, nowMoveDirection * nowSpeed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(nowMove * Time.deltaTime);
        m_prevMove = nowMove;
        
    }




    private void RotatingPlayer()
    {
        m_prevFacingRotationDir = m_desiredFacingRotationDirInWS;

        //if no input, then it should not recalculate the desired facing direction, because what if i stand still and then lock on something behind me, it should not affect any calculation as long as i dont move

        Vector3 SetDesiredFacingRotation()
        {
            Quaternion additionalFacingRotation = Quaternion.identity;
            if (m_facingDirectionType == Direction.Sideward)        additionalFacingRotation = Quaternion.Euler(0, 90 * -Mathf.Sign(m_inputDir.x), 0);
            else if (m_facingDirectionType == Direction.Backward)   additionalFacingRotation = Quaternion.Euler(0, 180, 0);

            return additionalFacingRotation * m_inputDirInWS;

            //the slerp makes the turning less extreme. better solution: Bone look at + constrains
            //m_desiredFacingRotationDirInWS = Vector3.Slerp(m_desiredFacingRotationDirInWS, PlayerToTargetXZVector, 0.0f); /////////////momantan zum testing auf 0, ist aber ehn nicht so ne schöne lösung
        }
        m_desiredFacingRotationDirInWS = (m_moveStrenght > 0) ? SetDesiredFacingRotation() : m_desiredFacingRotationDirInWS;


        //direction
        Vector3 desiredFacingRotationDirInWSByInput = m_desiredFacingRotationDirInWS;
        Vector3 desiredFacingRotationDirInWSByAction = m_desiredFacingRotationDirInWSByAction;
        Vector3 nowdesiredFacingRotationDirInWS = Vector3.Slerp(desiredFacingRotationDirInWSByInput, desiredFacingRotationDirInWSByAction, m_actionInfluenceOverDesiredFacingRotationDirInWS);

        //Speed
        float turningSpeedByInput = m_turningSpeed;
        float turningSpeedByAction = m_turningSpeedByAction;
        float nowTurningSpeed = Mathf.Lerp(turningSpeedByInput, turningSpeedByAction, m_actionInfluenceOverTurningSpeed);

        //acceleration
        float turningAccelerationByInput = m_turningAcceleration;
        float turningAccelerationByAction = m_turningAccelerationByAction;
        float nowTurningAcceleration = Mathf.Lerp(turningAccelerationByInput, turningAccelerationByAction, m_actionInfluenceOverTurningAcceleration);


        float angle = Mathf.Clamp(Vector3.SignedAngle(transform.forward, nowdesiredFacingRotationDirInWS, Vector3.up), -nowTurningSpeed, nowTurningSpeed); //Only ever 5° steps, the turning speed
        if(angle != 0) m_animator.SetFloat("TurningDir", angle > 0 ? 1 : -1);

        Quaternion nowDirection = transform.rotation * Quaternion.Euler(0, angle, 0);
        transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, nowDirection, Time.deltaTime * nowTurningAcceleration);

    }


}
