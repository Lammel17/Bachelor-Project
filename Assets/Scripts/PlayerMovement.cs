using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    private CharacterController m_characterController;

    private PlayerCameraHolder m_playerCameraHolder;

    private float m_inputFactor = 1f;

    private Vector2 m_input = Vector2.zero;
    private Vector3 m_WIP_moveDir = Vector3.zero;
    private Vector3 m_moveDir = Vector3.zero;
    private float m_moveAcceleration = 6f;
    private float turningAcceleration = 60f;

    private float speed = 6f;

    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMoveInput();

        if (m_input != Vector2.zero || m_moveDir.magnitude > 0.0001f)
            MovingPlayer();

        RotatingPlayer();
    }


    public void HandleMoveInput()
    {
        m_input = PlayerInputManager.Instance.LeftStick;
        Debug.Log(m_input);

        m_input = new Vector2(UtilityFunctions.RefitRange(Mathf.Abs(m_input.x), 0.1f * m_input.magnitude, 1, 0, 1) * Mathf.Sign(m_input.x), m_input.y);
        Debug.Log(m_input);
        m_WIP_moveDir = (Quaternion.Euler(0, m_playerCameraHolder.CameraLookDirection.y, 0) * new Vector3(m_input.x, 0, m_input.y)).normalized;
    }

    private void MovingPlayer()
    {
        m_moveDir = speed == 0 && m_WIP_moveDir.magnitude < 0.005f ? Vector3.zero : Vector3.Lerp(m_moveDir, m_WIP_moveDir * m_inputFactor * speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_moveDir * Time.deltaTime);
        //Debug.Log(m_moveDir);
    }

    private void RotatingPlayer()
    {

        float turningAcceleration = 60f;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_moveDir), Time.deltaTime * turningAcceleration);
    }


}
