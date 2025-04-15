using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerCameraHolder : MonoBehaviour
{
    private PlayerInputManager m_playerInputManager;

    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_playerTransform;

    [SerializeField] private Vector3 m_camRestCenter = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 m_camRestDirection = new Vector3(0, -1, 4);
    [SerializeField] private float m_camRestDist = -4f;


    private float m_turnAmount = 40f;
    private float m_turnDrag = 5f;
    private float m_delaySpeed = 6f;

    [NonSerialized] public static Quaternion m_cameraRotationVerX;
    [NonSerialized] public static Quaternion m_cameraRotationHorY;
    [NonSerialized] public static Quaternion m_WIP_rotationVerX;
    [NonSerialized] public static Quaternion m_WIP_rotationHorY;


    void Start()
    {
        m_playerInputManager = GameManager.InputManager;

        gameObject.transform.SetLocalPositionAndRotation(m_camRestCenter, Quaternion.LookRotation(m_camRestDirection, Vector3.up));
        m_camera.transform.localPosition = new Vector3(0, 0, m_camRestDist);

    }

    
    void Update()
    {
        SetCenterPosition();
        ControlCameraRotation();
        //CameraLookAt();
        //ControlCameraDistance();
    }

    private void SetCenterPosition()
    {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, m_playerTransform.position + m_camRestCenter, Time.deltaTime * 10); //??? 10?
    }

    private void ControlCameraRotation()
    {
        Vector2 input = m_playerInputManager.RightStick;
        float verTurn = input.y;
        float horTurn = input.x;
        //Debug.Log($" x {horTurn}, y {verTurn}");

        //Die gewünschte ausgangsPosition wo hinter dem Player ist, zu der sich die camera gegebenfalls hinziehen soll
        Vector3 camRestDir = m_camRestDirection;
        float turnDrag2 = m_turnDrag * 0.05f;

        //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
        Quaternion desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);

        m_WIP_rotationVerX = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
        m_WIP_rotationHorY = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);


        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_rotationVerX *= Quaternion.Euler(verTurn * m_turnAmount, 0, 0);
        m_WIP_rotationHorY *= Quaternion.Euler(0, horTurn * m_turnAmount, 0);

                //Cut für die Vertikale Drehung, damit man die kamera nicht überdreht
                //if (m_WIP_rotationVerX.eulerAngles.x > 60f && m_WIP_rotationVerX.eulerAngles.x < 180f) m_WIP_rotationVerX = Quaternion.Euler(60f, 0, 0); //oben
                //if (m_WIP_rotationVerX.eulerAngles.x < 340f && m_WIP_rotationVerX.eulerAngles.x > 180f) m_WIP_rotationVerX = Quaternion.Euler(340f, 0, 0); //unten

                //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt
                //m_WIP_rotationVerX = Quaternion.Slerp(m_WIP_rotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * turnDrag2); 
                //m_WIP_rotationHorY = Quaternion.Slerp(m_WIP_rotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * turnDrag2);
                //Debug.Log(pseudoCameraRotationVerX.eulerAngles);

        // Die eigentliche Kamera wird hier smooth zur Pseudo-Kamera gezogen
        m_cameraRotationVerX = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_rotationVerX, Time.deltaTime * m_delaySpeed);
        m_cameraRotationHorY = Quaternion.Slerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_rotationHorY, Time.deltaTime * m_delaySpeed);

        transform.rotation = Quaternion.Euler(m_cameraRotationVerX.eulerAngles.x, m_cameraRotationHorY.eulerAngles.y, 0);

        //hier wird aus der Kameradrehung die Position berechnet
        //Vector3 finalPosition = gameObject.transform.position + (m_cameraRotationHorY * (m_cameraRotationVerX * new Vector3(0, 0, m_camRestDirection.z)));

    }

    private void CameraLookAt()
    {
        
    }

    private void ControlCameraDistance()
    {
        
    }
}
