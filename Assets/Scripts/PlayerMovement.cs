using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    private CharacterController m_characterController;

    private PlayerCameraHolder m_playerCameraHolder;

    private float m_inputFactor = 1f;

    private Vector2 m_input = Vector2.zero;
    private Vector3 m_WIP_moveDir = Vector3.forward;
    private Vector3 m_move = Vector3.forward;
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

        if (m_input != Vector2.zero || m_move.magnitude > 0.0001f)
            MovingPlayer();

        RotatingPlayer();
    }


    public void HandleMoveInput()
    {
        m_input = PlayerInputManager.Instance.LeftStick;
        Debug.Log(m_input.magnitude);
        //m_input = m_input.normalized * UtilityFunctions.RefitRange(m_input.magnitude, 0.08f, 1, 0, 1); //maybe better in input script, bc in the 0.08, the camera still reacts
        Debug.Log(m_input.magnitude);

        m_input = new Vector2(UtilityFunctions.RefitRange(Mathf.Abs(m_input.x), 0.1f * m_input.magnitude, 1, 0, 1) * Mathf.Sign(m_input.x), m_input.y);

        m_WIP_moveDir = m_input.magnitude == 0 ? m_WIP_moveDir : (Quaternion.Euler(0, m_playerCameraHolder.CameraLookDirection.y, 0) * new Vector3(m_input.x, 0, m_input.y)).normalized;
    }

    private void MovingPlayer()
    {
        m_move = Vector3.Lerp(m_move, m_WIP_moveDir * m_inputFactor * m_input.magnitude * speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_move * Time.deltaTime);
        //Debug.Log(m_moveDir);
    }

    private void RotatingPlayer()
    {

        float turningAcceleration = 15f;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_WIP_moveDir), Time.deltaTime * turningAcceleration );
    }


}
