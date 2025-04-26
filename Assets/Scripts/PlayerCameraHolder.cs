using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerCameraHolder : MonoBehaviour
{
    public static PlayerCameraHolder Instance { get; private set; }
    private PlayerInputManager m_playerInputManager;
    [SerializeField] private PlayerMovement m_playerMovement;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_playerTransform;
    [Space]
    [SerializeField] private Vector3 m_camHolderLocalCenter = new Vector3(0, 1.6f, 0);
    [SerializeField] private Vector3 m_camHolderRestDirection = new Vector3(0, -1, 6.5f);
    [SerializeField] private float m_camRestDist = 4f;
    
    [Space]
    [SerializeField] private float m_camHolderClampAngleMax = 60f;
    private float m_camHolderClampAngle;

    private float m_stickHorFactor = 0.5f;
    private float m_stickVerFactor = 0.5f;
    private float m_camHolderRotationAcceleration = 8f;
    private Vector3 m_camHolderCenterPosBase;
    private Vector3 m_camHolderCenterPos;
    private float m_camHolderCenterFollowAcceleration = 15f;
    private Quaternion m_camHolderLookDirection;

    private Quaternion m_WIP_camHolderRotationVerX;
    private Quaternion m_WIP_camHolderRotationHorY;
    private Quaternion m_camHolderRotationVerX;
    private Quaternion m_camHolderRotationHorY;

    private Vector3 m_camPos;
    private float m_camLocalPosAcceleration = 5f;

    [SerializeField] private Transform m_testLockOnTransform;
    private bool m_isLockOn = false;
    private float lockOnParameter = 0;

    public Vector3 CameraCenter { get { return m_camHolderCenterPosBase; } }
    public Vector3 CameraLookDirection { get { return m_camHolderLookDirection.eulerAngles; } }
    public Quaternion CameraTopDownForward { get { Vector3 lookDir = m_camHolderLookDirection.eulerAngles; return Quaternion.Euler(lookDir.x, 0, lookDir.z); } }
    public bool IsLockOn { get => m_isLockOn; set => m_isLockOn = value; }
    public Vector3 LockOnCoordinates { get => m_testLockOnTransform.position;}


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

        gameObject.transform.SetLocalPositionAndRotation(m_camHolderLocalCenter, Quaternion.LookRotation(m_camHolderRestDirection, Vector3.up));
        m_camera.transform.localPosition = new Vector3(0, 0, -m_camRestDist);

    }

    private void OnEnable()
    {
        m_WIP_camHolderRotationVerX = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
        m_WIP_camHolderRotationHorY = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }


    void Update()
    {
        CalculateCameraHolderRotation();
        CalculateCameraHolderCenter();
        SetCameraHolderCenterAndRotation();
        CalculateAndSetCameraPos();
        //CameraLookAt();
        //ControlCameraDistance();
    }

    private void CalculateCameraHolderCenter()
    {

        m_camHolderCenterPosBase = UtilityFunctions.SmartLerp(m_camHolderCenterPosBase, m_playerTransform.position, Time.deltaTime * m_camHolderCenterFollowAcceleration);

        m_camHolderCenterPos = m_camHolderCenterPosBase + m_camHolderLocalCenter;

    }

    private void CalculateCameraHolderRotation()
    {
        float verTurn = 0;
        float horTurn = 0;
        Vector2 input = m_isLockOn ? Vector2.zero : m_playerInputManager.RightStick;

        if (input != Vector2.zero)
        {
            verTurn = -CalculateVerticalTurning(input);
            horTurn = CalculateHorizontalTurning(input);
        }

        m_camHolderClampAngle = CalculateClampAngleVerX(input, verTurn); //sqrMagnitude, weil der Wert eh immer unter 1 ist

        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_camHolderRotationHorY *= Quaternion.Euler(0, horTurn * m_stickHorFactor, 0);
        m_WIP_camHolderRotationVerX *= Quaternion.Euler(verTurn * m_stickVerFactor, 0, 0);

        //Apply Clamping
        m_WIP_camHolderRotationVerX = Quaternion.Euler(UtilityFunctions.AngleClamping(m_WIP_camHolderRotationVerX.eulerAngles.x, -m_camHolderClampAngle, m_camHolderClampAngle),0,0);

        ForcingPosition(input);

        // Die eigentliche Kamera-Rotation wird hier smooth zur WorkInProgress Rotation gezogen
        m_camHolderRotationVerX = UtilityFunctions.SmartSlerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_camHolderRotationVerX, Time.deltaTime * m_camHolderRotationAcceleration);
        m_camHolderRotationHorY = UtilityFunctions.SmartSlerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_camHolderRotationHorY, Time.deltaTime * m_camHolderRotationAcceleration);

        m_camHolderLookDirection = Quaternion.Euler(m_camHolderRotationVerX.eulerAngles.x, m_camHolderRotationHorY.eulerAngles.y, 0);

    }




    private void ForcingPosition(Vector2 input)
    {
        if (m_playerMovement.MoveStrenght == 0 && !IsLockOn)
            return;

        float m_desiredDirForceFactor = 0.4f;
        float desiredRotationForce = 0;
        Quaternion desiredRotation = Quaternion.identity;

        if (m_isLockOn)
        {
            Vector3 camRestDir = m_testLockOnTransform.transform.position - m_camHolderCenterPos;
            desiredRotationForce = 5;
            desiredRotation = Quaternion.LookRotation(camRestDir);
        }
        else 
        {
            Vector3 camRestDir = m_camHolderRestDirection;
            //here, abhängig nur von input.x, weil beim seitswärts laufen die kamera gedreht wird
            desiredRotationForce = m_desiredDirForceFactor * Mathf.Abs((m_playerMovement.InputDirection * m_playerMovement.MoveStrenght).x); 
            //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
            desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);
        }

        //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt
        m_WIP_camHolderRotationVerX = UtilityFunctions.SmartSlerp(m_WIP_camHolderRotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * desiredRotationForce); 
        m_WIP_camHolderRotationHorY = UtilityFunctions.SmartSlerp(m_WIP_camHolderRotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * desiredRotationForce);
    }

    private float CalculateClampAngleVerX(Vector2 input, float verTurn)
    {
        float clampAppyingAcceleration = 6F;

        if (input.sqrMagnitude >= 0.98f * 0.98f)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngle, Mathf.Abs(verTurn) * m_camHolderClampAngleMax, Time.deltaTime * clampAppyingAcceleration); //this is the clamp and how fast it applies when stick.magnitude is ~1  | 
        else if (m_camHolderClampAngle != m_camHolderClampAngleMax)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngle, m_camHolderClampAngleMax, Time.deltaTime * clampAppyingAcceleration); //this is how fast the clampAngleMax applies when stick.magnitude is less than 1
        
        return m_camHolderClampAngleMax;
    }

    private float CalculateVerticalTurning(Vector2 input)
    {
        return input.y; 
    }

    //Diese funktion gibt entweder die magnitude vom input zurück, bei input.x > input.y, andernfalls smoothet es den wert ab von der Magnitude zu input.x hin
    private float CalculateHorizontalTurning(Vector2 input)
    {
        float absInputX = Mathf.Abs(input.x);

        if (absInputX <= 0.01f)
            return 0f;

        float signInputX = Mathf.Sign(input.x);
        float absInputY = Mathf.Abs(input.y);

        float inpMagnitude = input.magnitude;

        if (absInputX > absInputY)
            return inpMagnitude * signInputX;
        else
            return Mathf.Lerp(input.x, inpMagnitude * signInputX, Vector2.Angle(Vector2.up, new Vector2(absInputX, absInputY)) / 45);
        //damit die x drehung gleich schnell bleibt, solange der stick unter 45° ist, also absInputX > absInputY.
    }

    private void SetCameraHolderCenterAndRotation()
    {
        transform.SetLocalPositionAndRotation(m_camHolderCenterPos, m_camHolderLookDirection);
    }


    private void CalculateAndSetCameraPos()
    {


        if (m_isLockOn)
        {
            //offset height is depending on angle
            float camYOffset = UtilityFunctions.RefitRange(UtilityFunctions.Angle180(m_camHolderRotationVerX.eulerAngles.x, false), 0, m_camHolderClampAngleMax, 0.5f, 2); 

            m_camPos = new Vector3(0, camYOffset, -m_camRestDist); 
            lockOnParameter = UtilityFunctions.SmartLerp(lockOnParameter, 0.5f, Time.deltaTime * 2f);
        }
        else
        {
            m_camPos = new Vector3(0, 0, -m_camRestDist);
            lockOnParameter = UtilityFunctions.SmartLerp(lockOnParameter, 0f, Time.deltaTime * 2f);
        }

        //cameraCenter gets an offset, to look over the players head a bit
        m_camera.transform.localPosition = UtilityFunctions.SmartLerp(m_camera.transform.localPosition, m_camPos, Time.deltaTime * m_camLocalPosAcceleration);
        
        //cameraRotation follows the target and player a bit
        Quaternion lookTotarget = Quaternion.LookRotation(m_testLockOnTransform.position - m_camera.transform.position);
        Quaternion lookToPlayer = Quaternion.LookRotation(m_camHolderCenterPos - m_camera.transform.position);
        m_camera.transform.rotation = Quaternion.Slerp(lookToPlayer, lookTotarget, lockOnParameter);

    }





    private void CameraLookAt()
    {
        
    }

    private void ControlCameraDistance()
    {
        
    }

}
