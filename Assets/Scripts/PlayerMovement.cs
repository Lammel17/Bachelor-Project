using System;
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

    [SerializeField] private Animator m_animator;
    [Space]
    [Header("Costants / static readonly")]
    private static readonly float m_inputFactor = 1f;
    private static readonly float m_moveAcceleration = 10f;
    private static readonly float m_turningAcceleration = 10f;

    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_moveDir = Vector3.forward;
    private float m_moveStrenght = 0f;
    private Vector3 m_move = Vector3.forward;

    private float m_speed = 4f;
    private Quaternion m_cameraContextRotation = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private float m_inputAngleToForward = 0;
    private bool m_isLockOn = false;
    private float m_forwardSidewardThreshholdAngle = 45f;
    private float m_sidewardBackwardThreshholdAngle = 135f;


    public Animator Animator { get => m_animator; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public Vector3 MoveDirection { get => m_moveDir; } //is always normalized due to InputDirection
    public float MoveStrenght { get => m_moveStrenght; set => m_moveStrenght = value; } //is already snapped by inputmanager
    public Quaternion CameraContextRotation { get => m_cameraContextRotation; set => m_cameraContextRotation = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target { get { if (m_target != null) return m_target; else { Debug.Log("target gets called, but is empty"); return null; } } set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get => Target.position; }
    public float TargetDist { get => m_targetDist; set => m_targetDist = value; }
    public float InputAngleToForward { get => m_inputAngleToForward; set => m_inputAngleToForward = value; }
    public Vector3 PlayerToTargetXZVector { get => new Vector3(TargetPos.x - transform.position.x, 0, TargetPos.z - transform.position.z); }



    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    void Update()
    {
        SetValues();
        
        SetAnimatorMoveValues();

        MovingPlayer();

        RotatingPlayer();
    }

    private void SetValues() //moveDir, threshholds, TargetDist
    {
        if (!m_isLockOn)
            m_moveDir = m_cameraContextRotation * m_inputDir;
        else
        {
            TargetDist = (TargetPos - transform.position).magnitude;
            InputAngleToForward = Vector3.Angle(Vector3.forward, m_inputDir);

            //playerToTargetAndContextRotationSlerp: weil wenn man nah am target stand und vorwärts lief, dann zirkulierte man ewig um es rum anstatt straight drauf zu zu lenken, daher nun halb halb
            Quaternion playerToTargetLookRotation = Quaternion.LookRotation(PlayerToTargetXZVector);
            Quaternion playerToTargetAndContextRotationSlerp = Quaternion.Slerp(CameraContextRotation, playerToTargetLookRotation, 0.5f);
            m_moveDir = playerToTargetAndContextRotationSlerp * m_inputDir;

            SetThreshholds();
        }

        void SetThreshholds()
        {
            //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
            float firstThreshholdAngleMin = 15f;
            float secondThreshholdAngleMin = 110f;
            float distThreshhold = 10f;

            m_forwardSidewardThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, TargetDist / distThreshhold);
            m_sidewardBackwardThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, TargetDist / distThreshhold);
        }

    }

    private void SetAnimatorMoveValues()
    {
        float animationDampTime = 0.15f; //smaller is faster transition
        float VerticalMovement = m_moveStrenght; //is already snapped in inputmanager
        m_animator.SetFloat("MoveMag", VerticalMovement, animationDampTime, Time.deltaTime);

        if (!m_playerCameraHolder.IsLockOn)
        {
            m_animator.SetFloat("Vertical", 1, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else
        {

            float angle = InputAngleToForward;

            Vector2 horAndVerMovement = Vector2.zero;

            if (angle < m_forwardSidewardThreshholdAngle) //forward walking
                horAndVerMovement = new Vector2(0, 1);
            else if (angle < m_sidewardBackwardThreshholdAngle) //sideward walking
                horAndVerMovement = new Vector2(Mathf.Sign(m_inputDir.x), 0);
            else //backward walking
                horAndVerMovement = new Vector2(0, -1);

            m_animator.SetFloat("Vertical", horAndVerMovement.y, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", horAndVerMovement.x, animationDampTime, Time.deltaTime);

        }
    }


    private void MovingPlayer()
    {
        m_move = UtilityFunctions.SmartLerp(m_move, m_moveDir * m_inputFactor * m_moveStrenght * m_speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
    }



    private void RotatingPlayer()
    {
        float turningAcceleration = m_turningAcceleration;

        if (m_moveStrenght == 0)
            return;

        Vector3 desiredDirection = Vector3.zero;

        if (!m_playerCameraHolder.IsLockOn) // no LockOn
        {
            desiredDirection = m_moveDir;
        }
        else //LockOn
        {
            float angle = InputAngleToForward;

            if (angle < m_forwardSidewardThreshholdAngle) //forward walking
                desiredDirection = m_moveDir;
            else if (angle < m_sidewardBackwardThreshholdAngle) //sideward walking
                desiredDirection = Quaternion.Euler(0, 90 * -Mathf.Sign(m_inputDir.x), 0) * m_moveDir;
            else //backward walking
                desiredDirection = Quaternion.Euler(0, 180, 0) * m_moveDir;

            //the slerp makes the turning less extreme
            desiredDirection = Vector3.Slerp(desiredDirection, PlayerToTargetXZVector, 0.5f);

        }

        transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, Quaternion.LookRotation(desiredDirection), Time.deltaTime * turningAcceleration);


    }


}
