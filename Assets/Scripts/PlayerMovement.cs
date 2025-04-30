using System;
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
    private static readonly float m_turningAcceleration = 12f;

    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_moveDir = Vector3.forward;
    private float m_moveStrenght = 0f;
    private Vector3 m_move = Vector3.forward;

    private float m_speed = 4f;
    private Quaternion m_contextRotation = Quaternion.identity;
    private Transform m_target;
    private float m_targetDist = 0;
    private bool m_isLockOn = false;


    public Animator Animator { get => m_animator; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public Vector3 MoveDirection { get => m_moveDir; } //is always normalized due to InputDirection
    public float MoveStrenght { get => m_moveStrenght; set => m_moveStrenght = value; } //is already snapped by inputmanager
    public Quaternion ContextRotation { get => m_contextRotation; set => m_contextRotation = Quaternion.Euler(0, value.eulerAngles.y, 0); }
    public Transform Target { get => m_target; set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get { if (m_target != null) return m_target.position; else { Debug.Log("target gets called, but is empty"); return Vector3.zero; } }}
    public float TargetDist { get { if (m_target != null) return m_targetDist; else { Debug.Log("target gets called, but is empty"); return 0; } } set => m_targetDist = value; }



    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    void Update()
    {
        SetMoveDir();
        
        SetAnimatorMoveValues();

        MovingPlayer();

        RotatingPlayer();
    }

    private void SetMoveDir()
    {
        //m_moveDir = m_contextRotation * m_inputDir;

        if (!m_isLockOn)
            m_moveDir = m_contextRotation * m_inputDir;
        else
        {
            TargetDist = (TargetPos - transform.position).magnitude;

            float sidewardSpeedFactorMinWhenClose = 0.4f; // this is because the sidewardmovement is too fast when being close
            m_moveDir = transform.rotation * new Vector3(m_inputDir.x * Mathf.Lerp(sidewardSpeedFactorMinWhenClose, 1f, TargetDist / 5), 0, m_inputDir.z);
        }

    }

    private void SetAnimatorMoveValues()
    {
        float animationDampTime = 0.1f; //smaller is faster transition
        if (!m_playerCameraHolder.IsLockOn)
        {
            float VerticalMovement = Snapping.Snap(m_moveStrenght, 0.5f);

            m_animator.SetFloat("Vertical", VerticalMovement, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else
        {
            //das setzt den threshhold für ab welchen winkel die vorwärts, seitswärt order rückwärts animation abgespielt wird
            float firstThreshholdAngleMin = 15f;
            float secondThreshholdAngleMin = 110f;
            float distThreshhold = 10f;

            float firstThreshholdAngle = Mathf.Lerp(firstThreshholdAngleMin, 45f, TargetDist / distThreshhold);
            float secondThreshholdAngle = Mathf.Lerp(secondThreshholdAngleMin, 135f, TargetDist / distThreshhold);

            float angle = Vector3.Angle(Vector3.forward, m_inputDir);

            float verticalMovement = 0;
            float horizontalMovement = 0;

            if (angle < firstThreshholdAngle) //forward walking
            {
                verticalMovement = MoveStrenght;
                horizontalMovement = 0;
            }
            else if (angle < secondThreshholdAngle) //sideward walking
            {
                verticalMovement = 0;
                horizontalMovement = MoveStrenght * Mathf.Sign(m_inputDir.x);
            }
            else //backward walking
            {
                verticalMovement = -MoveStrenght;
                horizontalMovement = 0;
            }

            m_animator.SetFloat("Vertical", verticalMovement, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", horizontalMovement, animationDampTime, Time.deltaTime);

            Debug.Log($"1: {firstThreshholdAngle}, 2: {secondThreshholdAngle}");
            Debug.Log($"angle: {angle}");
            Debug.Log($"vertical: {(int)verticalMovement}, horizontal: {(int)horizontalMovement}");
        }
    }


    private void MovingPlayer()
    {
        m_move = UtilityFunctions.SmartLerp(m_move, m_moveDir * m_inputFactor * m_moveStrenght * m_speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
    }



   [SerializeField] private AnimationCurve m_additionalRotationCurve;
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
            Vector3 targetToPlayer = TargetPos - transform.position;
            //Quaternion additionalRotation =  targetToPlayer.magnitude ;

            desiredDirection = new Vector3(targetToPlayer.x, 0, targetToPlayer.z);
        }

        transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, Quaternion.LookRotation(desiredDirection), Time.deltaTime * turningAcceleration);


    }


}
