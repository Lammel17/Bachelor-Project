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
    [Header("")]
    [SerializeField] private Vector3 m_speedValues = new Vector3(2, 4, 6); //slow, walk, running
    [SerializeField] private float m_moveAcceleration = 20f;
    [SerializeField] private float m_turningSpeedBaseValue = 30f;
    [SerializeField] private float m_turningAcceleration = 15f;

    private readonly float m_inputFactor = 1f;
    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_contextDir = Vector3.forward;
    private Vector3 m_latestProcessedDir = Vector3.forward;
    private float m_moveStrenght = 0f;
    private Vector3 m_move = Vector3.zero;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;
    private float m_turningSpeed;

    private float m_speed = 0; //slow, walk, running
    private Quaternion m_cameraContextRotation = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private float m_inputAngleToForward = 0;
    private bool m_isLockOn = false;
    private bool m_isForcedDirectedToTarget = false;
    private enum Direction { Forward, Sideward, Backward };
    private Direction m_directionWhenLockOn = Direction.Forward;

    private bool m_isRunning = false;
    private int m_runningspeed = 2;

    private Coroutine m_turningCoroutine;
    private bool m_isTurning = false;


    public Animator Animator { get => m_animator; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public Vector3 MoveDirection { get => m_contextDir; } //is always normalized due to InputDirection
    public float MoveStrenght { get => m_moveStrenght; set { if (m_isRunning && value > 0f)  m_moveStrenght = 2; else m_moveStrenght = value; Speed = m_moveStrenght; } } //is already snapped by inputmanager
    public float Speed { get => m_speed; set { m_speed = value == 0 ? 0 : value == 0.5 ? m_speedValues.x : value == 1 ? m_speedValues.y : m_speedValues.z; } } //is already snapped by inputmanager
    public Quaternion CameraContextRotation { get => m_cameraContextRotation; set => m_cameraContextRotation = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target { get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get => Target.position; }
    public float TargetDist { get => m_targetDist; set => m_targetDist = value; }
    public float InputAngleToForward { get => m_inputAngleToForward; set => m_inputAngleToForward = value; }
    public Vector3 PlayerToTargetXZVector { get => new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z); }
    public bool IsRunning { get => m_isRunning; set { m_isRunning = value; MoveStrenght = m_playerInputManager.LeftStickSnappedMag; m_animator.SetBool("IsRunning", value); } }


    void Start()
    {
        m_playerInputManager = PlayerInputManager.Instance;
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;

        m_turningSpeed = m_turningSpeedBaseValue;
    }

    void Update()
    {
        SetValues();
        
        RotatingPlayer();

        MovingPlayer();

        SetAnimatorMoveValues();
    }

    private void SetValues() //moveDir, threshholds, TargetDist
    {
        Vector3 prevmoveDir = m_contextDir;
        m_isForcedDirectedToTarget = (!m_isLockOn || m_isRunning);

        if (m_isForcedDirectedToTarget)
        {
            // contextDir is relative to cameraRotation, so it should not affect the contextDir when for example standing still
            m_contextDir = m_moveStrenght > 0 ? m_cameraContextRotation * m_inputDir : m_contextDir; 
            m_directionWhenLockOn = Direction.Forward;
        }
        else
        {
            TargetDist = (TargetPos - transform.position).magnitude;
            InputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);

            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector);
            Quaternion playerToTargetAndContextRotationSlerp = Quaternion.Slerp(CameraContextRotation, playerToTargetLookRotation, 0.5f);
            m_contextDir = playerToTargetAndContextRotationSlerp * m_inputDir;

            SetDirection();
            void SetDirection()
            {
                //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
                float firstThreshholdAngleMin = 25f;
                float secondThreshholdAngleMin = 110f;
                float distThreshhold = 10f;
                float additionalThreshhold = 7f;

                if (m_directionWhenLockOn == Direction.Sideward) additionalThreshhold = -additionalThreshhold;

                m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, TargetDist / distThreshhold) + additionalThreshhold;
                m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, TargetDist / distThreshhold) - additionalThreshhold;

                float angle = InputAngleToForward;

                if (angle < m_forwardSidewardThreshholdAngle) m_directionWhenLockOn = Direction.Forward;
                else if (angle < m_sidewardBackwardThreshholdAngle) m_directionWhenLockOn = Direction.Sideward;
                else m_directionWhenLockOn = Direction.Backward;
            }
        }
        TriggerTurning();
        void TriggerTurning()
        {
            if (m_isTurning)
                return;
            //problem for tomorrow, i need a way to check if the stick gets flipped over instead of turned over, so i know if turning animation or not
            float angleMoveDirToPrevMoveDir = Vector3.Angle(m_contextDir, prevmoveDir);
            if ( (!m_isLockOn && (!m_isRunning || m_move.sqrMagnitude == 0) && angleMoveDirToPrevMoveDir > 90) || (m_isRunning && angleMoveDirToPrevMoveDir > 150))
            {
                float turnAnimationTurningSpeed = 45f;
                m_turningSpeed = turnAnimationTurningSpeed;

                m_animator.SetTrigger("IsTurning");
                m_isTurning = true;
                Action resetTurnAction = () => { m_isTurning = false; m_animator.SetBool("IsTurningg", false); m_turningSpeed = m_turningSpeedBaseValue; m_turningCoroutine = null;};
                m_turningCoroutine = StartCoroutine(UtilityFunctions.Wait(0.45f, resetTurnAction));
            }
        }
    }



    private void SetAnimatorMoveValues()
    {
        float animationDampTime = 0.15f; //smaller is faster transition
        float VerticalMovement = m_moveStrenght; //is already snapped in inputmanager
        m_animator.SetFloat("MoveMag", VerticalMovement, animationDampTime, Time.deltaTime);

        if (m_isForcedDirectedToTarget)
        {
            m_animator.SetFloat("Vertical", 1, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else
        {
            Vector2 horAndVerMovement = Vector2.zero;

            if      (m_directionWhenLockOn == Direction.Forward)  horAndVerMovement = new Vector2(0, 1);
            else if (m_directionWhenLockOn == Direction.Sideward) horAndVerMovement = new Vector2(Mathf.Sign(m_inputDir.x), 0);
            else                                        horAndVerMovement = new Vector2(0, -1);

            m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);
        }

    }


    private void MovingPlayer()
    {
        //less movement gets applied if the character is still not turned into moveDir //not sure if this is a nice solution
        float forwardFactor = m_isTurning ? UtilityFunctions.RefitRange(Vector3.Angle(transform.forward, m_contextDir), 30, 20, 0, 1) : 1f;

        Vector3 direction = !m_isForcedDirectedToTarget ? m_contextDir : transform.forward;
        m_move =  UtilityFunctions.SmartLerp(m_move, direction * m_inputFactor * m_speed * forwardFactor, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
    }


    private void RotatingPlayer()
    {
        float turningAcceleration = m_turningAcceleration;
        Vector3 desiredDirection = Vector3.zero;

        //if no input, then it should not recalculate direction
        if(m_moveStrenght <= 0)
            desiredDirection = m_latestProcessedDir;
        else
        {
            if (m_isForcedDirectedToTarget) // no LockOn
            {
                desiredDirection = m_contextDir;
            }
            else //LockOn
            {
                if (m_directionWhenLockOn == Direction.Forward) desiredDirection = m_contextDir;
                else if (m_directionWhenLockOn == Direction.Sideward) desiredDirection = Quaternion.Euler(0, 90 * -Mathf.Sign(m_inputDir.x), 0) * m_contextDir;
                else desiredDirection = Quaternion.Euler(0, 180, 0) * m_contextDir;

                //the slerp makes the turning less extreme
                //desiredDirection = Vector3.Slerp(desiredDirection, PlayerToTargetXZVector, 0.0f); /////////////momantan zum testing auf 0, ist aber ehn nicht so ne schöne lösung
                //better solution: Bone look at + constrains
            }

            m_latestProcessedDir = desiredDirection;
        }
        

        float angle = Mathf.Clamp(Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up), -m_turningSpeed, m_turningSpeed); //Only ever 5° steps, the turning speed
        Quaternion newDirection = transform.rotation * Quaternion.Euler(0, angle, 0);

        transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, newDirection, Time.deltaTime * turningAcceleration);

        if(angle != 0) m_animator.SetFloat("TurningDir", angle > 0 ? 1 : -1);
    }


}
