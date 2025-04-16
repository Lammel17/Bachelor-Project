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
    [SerializeField] private float m_camRestDist = 4f;


    private float m_stickFactorHor = 0.8f;
    private float m_stickFactorVer = 0.5f;
    private float m_turnDrag = 5f;
    private float m_slerpToNewRot = 6f;
    private const float c_clampAngle = 60f;
    private float m_clampAngle;

    [NonSerialized] public static Quaternion m_cameraRotationVerX;
    [NonSerialized] public static Quaternion m_cameraRotationHorY;
    [NonSerialized] public static Quaternion m_WIP_rotationVerX;
    [NonSerialized] public static Quaternion m_WIP_rotationHorY;


    void Start()
    {
        m_playerInputManager = GameManager.InputManager;

        gameObject.transform.SetLocalPositionAndRotation(m_camRestCenter, Quaternion.LookRotation(m_camRestDirection, Vector3.up));
        m_camera.transform.localPosition = new Vector3(0, 0, -m_camRestDist);

    }

    private void OnEnable()
    {
        m_WIP_rotationVerX = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
        m_WIP_rotationHorY = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }


    void Update()
    {
        CalculateCameraRotation();
        SetCameraCenterAndRotation();
        //CameraLookAt();
        //ControlCameraDistance();
    }

    private void CalculateCameraRotation()
    {
        Vector2 input = m_playerInputManager.RightStick;
        float verTurn = - CalculateVerticalTurn(input);
        float horTurn = CalculateHorizontalTurn(input);
        m_clampAngle = CalculateClampAngleVerX(input.magnitude, verTurn);

        Debug.Log($" ver {verTurn}");
        Debug.Log($" hor {horTurn}");



        //Die gewünschte ausgangsPosition wo hinter dem Player ist, zu der sich die camera gegebenfalls hinziehen soll
        Vector3 camRestDir = m_camRestDirection;
        //float turnDrag2 = m_turnDrag * 0.05f;

        //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
        Quaternion desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);

        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_rotationHorY *= Quaternion.Euler(0, horTurn * m_stickFactorHor, 0);
        m_WIP_rotationVerX *= Quaternion.Euler(verTurn * m_stickFactorVer, 0, 0);

        //Clamping
        m_WIP_rotationVerX = Quaternion.Euler(UtilityFunctions.AngleClamping(m_WIP_rotationVerX.eulerAngles.x, -m_clampAngle, m_clampAngle),0,0);

        //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt
        //m_WIP_rotationVerX = Quaternion.Slerp(m_WIP_rotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * turnDrag2); 
        //m_WIP_rotationHorY = Quaternion.Slerp(m_WIP_rotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * turnDrag2);
        //Debug.Log(pseudoCameraRotationVerX.eulerAngles);

        // Die eigentliche Kamera wird hier smooth zur Pseudo-Kamera gezogen
        m_cameraRotationVerX = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_rotationVerX, Time.deltaTime * m_slerpToNewRot);
        m_cameraRotationHorY = Quaternion.Slerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_rotationHorY, Time.deltaTime * m_slerpToNewRot);

        
    }

    private float CalculateClampAngleVerX(float inputMagnitude, float verTurn)
    {
        if (inputMagnitude >= 0.95f)
            return Mathf.Lerp(m_clampAngle, Mathf.Abs(verTurn) * c_clampAngle, Time.deltaTime * 6); //this is how fast the clamp applies when stick.magnitude is ~1  | 
        else
            return Mathf.Lerp(m_clampAngle, c_clampAngle, Time.deltaTime * 6); //this is how fast the clamp applies else
    }

    private float CalculateVerticalTurn(Vector2 input)
    {
        float signInputY = Mathf.Sign(input.y);
        float absInputX = Mathf.Abs(input.x);
        float absInputY = Mathf.Abs(input.y);

        //return Mathf.InverseLerp(0.05f, 0.9f, absY) * signY; 
        float angle = Mathf.Sqrt(Vector2.Angle(Vector2.right, new Vector2(absInputX, absInputY))/ 90) * 90; //maybe rework: wie stark der y input reagiert, the sqrt part
        //Debug.Log(angle);
        return (1 - Mathf.Cos(angle* Mathf.Deg2Rad)) * signInputY * absInputY;
    }

    //Diese funktion gibt entweder die magnitude vom input zurück, bei input.x > input.y, andernfalls smoothet es den wert ab von der Magnitude zu input.x hin
    private float CalculateHorizontalTurn(Vector2 input)
    {
        float absInputX = Mathf.Abs(input.x);

        if (absInputX <= 0.01f)
            return 0f;

        float signInputX = Mathf.Sign(input.x);
        float absInputY = Mathf.Abs(input.y);

        return (absInputX > absInputY) ? input.magnitude * signInputX : Mathf.Lerp(input.x, input.magnitude * signInputX, Vector2.Angle(Vector2.up, new Vector2(absInputX, absInputY)) / 45);

    }

    private void SetCameraCenterAndRotation()
    {
        //Set Rotation and Center Position
        transform.SetLocalPositionAndRotation(Vector3.Lerp(gameObject.transform.position, m_playerTransform.position + m_camRestCenter, Time.deltaTime * 10),
                                                      Quaternion.Euler(m_cameraRotationVerX.eulerAngles.x, m_cameraRotationHorY.eulerAngles.y, 0));
    }

    private void CameraLookAt()
    {
        
    }

    private void ControlCameraDistance()
    {
        
    }

}
