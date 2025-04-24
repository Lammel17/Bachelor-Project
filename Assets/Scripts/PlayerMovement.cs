using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private float m_moveAcceleration = 6f;
    private float turningAcceleration = 60f;

    private float m_speed = 6f;

    public Animator Animator { get => m_animator; }
    public Vector3 InputDirection { get => m_inputDir; set { if (value.magnitude == 0) return; m_inputDir = value.normalized; }} //is always normalized
    public Vector3 MoveDirection { get => m_moveDir; } //is always normalized
    public float MoveStrenght { get => m_moveStrenght; set => m_moveStrenght = value; }

    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    void Update()
    {
        Quaternion cameraRot = Quaternion.Euler(0, m_playerCameraHolder.CameraLookDirection.y, 0);
        m_moveDir = m_moveStrenght == 0 ? m_moveDir : cameraRot * m_inputDir;

        m_animator.SetFloat("Vertical", m_moveStrenght, 0.1f, Time.deltaTime);

        if (m_moveStrenght != 0 || m_move.magnitude > 0.0001f)
            MovingPlayer();

        RotatingPlayer();
    }


    private void MovingPlayer()
    {
        m_move = Vector3.Lerp(m_move, m_moveDir * m_inputFactor * m_moveStrenght * m_speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
        //Debug.Log(m_moveDir);
    }

    private void RotatingPlayer()
    {
        float turningAcceleration = 15f;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_moveDir), Time.deltaTime * turningAcceleration );
    }


}
