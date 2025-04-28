using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Animator m_animator;

    private CharacterController m_characterController;

    private PlayerCameraHolder m_playerCameraHolder;

    private float m_inputFactor = 1f;

    private Vector3 m_inputDir = Vector3.forward;
    private Vector3 m_moveDir = Vector3.forward;
    private float m_moveStrenght = 0f;
    private Vector3 m_move = Vector3.forward;
    private float m_moveAcceleration = 12f;
    private float m_turningAcceleration = 12f;
    private Quaternion m_contextRotation = Quaternion.identity;

    private float m_speed = 6f;

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

        if (m_moveStrenght != 0 || m_move.sqrMagnitude > 0.0001f * 0.0001f)
            MovingPlayer();

        RotatingPlayer();
    }

    private void SetAnimatorMoveValues()
    {
        if (!m_playerCameraHolder.IsLockOn)
        {
            m_animator.SetFloat("Vertical", m_moveStrenght, 0.1f, Time.deltaTime);
            m_animator.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
        }
        else
        {
            m_animator.SetFloat("Vertical", m_inputDir.z * m_moveStrenght, 0.1f, Time.deltaTime);    
            m_animator.SetFloat("Horizontal", m_inputDir.x * m_moveStrenght, 0.1f, Time.deltaTime);
        }
    }



    private void MovingPlayer()
    {
        m_move = UtilityFunctions.SmartLerp(m_move, m_moveDir * m_inputFactor * m_moveStrenght * m_speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
    }

    private float additionalTurningCorrection = 0;
    private void RotatingPlayer()
    {
        float turningAcceleration = m_turningAcceleration;

        int dir = (int)Mathf.Sign(m_inputDir.x);
        if (m_inputDir.x > 0.001f) dir = -1;
        else if (m_inputDir.x < -0.001f) dir = 1;
        if (dir == 0) additionalTurningCorrection = Mathf.Lerp(additionalTurningCorrection, 0, Time.deltaTime * 6);

        if (m_moveStrenght == 0)
            return;

        if (!m_playerCameraHolder.IsLockOn)
        {
            transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, Quaternion.LookRotation(m_moveDir), Time.deltaTime * turningAcceleration);
        }
        else
        {
            //Vector3 lockOnDir = new Vector3(m_playerCameraHolder.LockOnCoordinates.x - transform.position.x, 0, m_playerCameraHolder.LockOnCoordinates.z - transform.position.z);
            //transform.rotation = UtilityFunctions.SmartSlerp(transform.rotation, Quaternion.LookRotation(lockOnDir), Time.deltaTime * turningAcceleration);

            additionalTurningCorrection = Mathf.Lerp(additionalTurningCorrection, 12f * dir * (4 * (22 - (m_playerCameraHolder.LockOnCoordinates-transform.position).magnitude) / 22), Time.deltaTime * 6);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, m_contextRotation.eulerAngles.y + additionalTurningCorrection, 0), Time.deltaTime * 6);
        }
    }


}
