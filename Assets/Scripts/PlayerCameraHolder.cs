using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerCameraHolder : MonoBehaviour
{
    public static PlayerCameraHolder Instance { get; private set; }
    private PlayerInputManager m_playerInputManager;

    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_playerTransform;
    [Space]
    [SerializeField] private Vector3 m_camRestCenter = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 m_camRestDirection = new Vector3(0, -1, 4);
    [SerializeField] private float m_camRestDist = 4f;
    
    [Space]
    [SerializeField] private float m_clampAngleMax = 60f;

    private float m_stickHorFactor = 0.5f;
    private float m_stickVerFactor = 0.5f;
    private float m_turnDrag = 5f;
    private float m_slerpAcceleration = 12f;
    private float m_clampAngle;
    private Vector3 m_camCenter;
    private float m_camCenterFollowAcceleration = 15f;
    private Quaternion m_camLookDirection;

    private Quaternion m_WIP_rotationVerX;
    private Quaternion m_WIP_rotationHorY;
    private Quaternion m_camRotationVerX;
    private Quaternion m_camRotationHorY;
    public Vector3 CameraCenter { get { return m_camCenter; } }
    public Vector3 CameraLookDirection { get { return m_camLookDirection.eulerAngles; } }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        m_playerInputManager = PlayerInputManager.Instance;

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
        float verTurn = 0;
        float horTurn = 0;

        if (input != Vector2.zero)
        {
            verTurn = -CalculateVerticalTurning(input);
            horTurn = CalculateHorizontalTurning(input);
        }

        m_clampAngle = CalculateClampAngleVerX(input.magnitude, verTurn);

        //Debug.Log($" ver {verTurn}");
        //Debug.Log($" hor {horTurn}");

        //Die gewünschte ausgangsPosition wo hinter dem Player ist, zu der sich die camera gegebenfalls hinziehen soll
        Vector3 camRestDir = m_camRestDirection;
        //float turnDrag2 = isLockOn ? turnDrag * 10 : turnDrag * 0.05f;

        //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
        Quaternion desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);

        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_rotationHorY *= Quaternion.Euler(0, horTurn * m_stickHorFactor, 0);
        m_WIP_rotationVerX *= Quaternion.Euler(verTurn * m_stickVerFactor, 0, 0);

        //Apply Clamping
        m_WIP_rotationVerX = Quaternion.Euler(UtilityFunctions.AngleClamping(m_WIP_rotationVerX.eulerAngles.x, -m_clampAngle, m_clampAngle),0,0);

        //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt
        //m_WIP_rotationVerX = Quaternion.Slerp(m_WIP_rotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * turnDrag2); 
        //m_WIP_rotationHorY = Quaternion.Slerp(m_WIP_rotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * turnDrag2);
        //Debug.Log(pseudoCameraRotationVerX.eulerAngles);

        // Die eigentliche Kamera-Rotation wird hier smooth zur WorkInProgress Rotation gezogen
        m_camRotationVerX = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_rotationVerX, Time.deltaTime * m_slerpAcceleration);
        m_camRotationHorY = Quaternion.Slerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_rotationHorY, Time.deltaTime * m_slerpAcceleration);

        
    }

    private float CalculateClampAngleVerX(float inputMagnitude, float verTurn)
    {
        if (inputMagnitude >= 0.98f)
            return Mathf.Lerp(m_clampAngle, Mathf.Abs(verTurn) * m_clampAngleMax, Time.deltaTime * 6); //this is how fast the clamp applies when stick.magnitude is ~1  | 
        else
            return Mathf.Lerp(m_clampAngle, m_clampAngleMax, Time.deltaTime * 6); //this is how fast the clamp applies else
    }

    private float CalculateVerticalTurning(Vector2 input)
    {
        float signInputY = Mathf.Sign(input.y);
        float absInputX = Mathf.Abs(input.x);
        float absInputY = Mathf.Abs(input.y);

        //return Mathf.InverseLerp(0.05f, 0.9f, absY) * signY; 
        float angle = Mathf.Sqrt(Vector2.Angle(Vector2.right, new Vector2(absInputX, absInputY))/ 90) * 90; //maybe rework: wie stark der y input reagiert, the sqrt part
        return (1 - Mathf.Cos(angle* Mathf.Deg2Rad)) * signInputY * absInputY;
    }

    //Diese funktion gibt entweder die magnitude vom input zurück, bei input.x > input.y, andernfalls smoothet es den wert ab von der Magnitude zu input.x hin
    private float CalculateHorizontalTurning(Vector2 input)
    {
        float absInputX = Mathf.Abs(input.x);

        if (absInputX <= 0.01f)
            return 0f;

        float signInputX = Mathf.Sign(input.x);
        float absInputY = Mathf.Abs(input.y);

        return (absInputX > absInputY) ? input.magnitude * signInputX : Mathf.Lerp(input.x, input.magnitude * signInputX, Vector2.Angle(Vector2.up, new Vector2(absInputX, absInputY)) / 45);
        //damit die x drehung gleich schnell bleibt, solange der stick unter 45° ist, also absInputX > absInputY.
    }

    private void SetCameraCenterAndRotation()
    {
        //Set Rotation and Center Position
        m_camCenter = Vector3.Lerp(gameObject.transform.position, m_playerTransform.position + m_camRestCenter, Time.deltaTime * m_camCenterFollowAcceleration);
        m_camLookDirection = Quaternion.Euler(m_camRotationVerX.eulerAngles.x, m_camRotationHorY.eulerAngles.y, 0);

        transform.SetLocalPositionAndRotation(m_camCenter, m_camLookDirection);
    }

    private void CameraLookAt()
    {
        
    }

    private void ControlCameraDistance()
    {
        
    }

}
