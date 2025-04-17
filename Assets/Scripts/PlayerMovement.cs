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

    private float speed = 6f;

    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerCameraHolder = PlayerCameraHolder.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMoveInput(); /////////

        if (m_input != Vector2.zero || m_moveDir.magnitude > 0.0001f)
            Moving();

    }

    public void HandleMoveInput()
    {
        m_input = PlayerInputManager.Instance.LeftStick;
        m_WIP_moveDir = (Quaternion.Euler(0, m_playerCameraHolder.CameraLookDirection.y, 0) * new Vector3(m_input.x, 0, m_input.y)).normalized;
        //Debug.Log(m_input);

    }

    private void Moving()
    {

        m_moveDir = speed == 0 && m_WIP_moveDir.magnitude < 0.005f ? Vector3.zero : Vector3.Lerp(m_moveDir, m_WIP_moveDir * m_inputFactor * speed, Time.deltaTime * m_moveAcceleration);
        m_characterController.Move(m_moveDir * Time.deltaTime);
        Debug.Log(m_moveDir);
    }



}
