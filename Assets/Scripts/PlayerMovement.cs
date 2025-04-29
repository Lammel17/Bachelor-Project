using System;
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

    private float m_speed = 6f;
    private Quaternion m_contextRotation = Quaternion.identity;


    public Animator Animator { get => m_animator; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value == Vector3.zero) return; m_inputDir = value.normalized; }} //is always normalized and never zero
    public Vector3 MoveDirection { get => m_moveDir; } //is always normalized due to InputDirection
    public float MoveStrenght { get => m_moveStrenght; set => m_moveStrenght = value; }
    public Quaternion ContextRotation { get => m_contextRotation; set => m_contextRotation = Quaternion.Euler(0, value.eulerAngles.y, 0); }



    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    void Update()
    {
        m_moveDir = m_contextRotation * m_inputDir;
        SetAnimatorMoveValues();

        MovingPlayer();

        RotatingPlayer();
    }

    private void SetAnimatorMoveValues()
    {
        float animationDampTime = 0.1f;

        if (!m_playerCameraHolder.IsLockOn)
        {
            m_animator.SetFloat("Vertical", m_moveStrenght, animationDampTime, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, animationDampTime, Time.deltaTime);
        }
        else
        {
            m_animator.SetFloat("Vertical", m_inputDir.z * m_moveStrenght, animationDampTime, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", m_inputDir.x * m_moveStrenght, animationDampTime, Time.deltaTime);
        }
    }


    private void MovingPlayer()
    {
        m_move = UtilityFunctions.SmartLerp(m_move, m_moveDir * m_inputFactor * m_moveStrenght * m_speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
    }



   [SerializeField] private AnimationCurve m_additionalRotation;
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
            desiredDirection = new Vector3(m_playerCameraHolder.LockOnCoordinates.x - transform.position.x, 0, m_playerCameraHolder.LockOnCoordinates.z - transform.position.z);
        }

        transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, Quaternion.LookRotation(desiredDirection), Time.deltaTime * turningAcceleration);


    }


}
