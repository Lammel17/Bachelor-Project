using System;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class PlayerCameraHolder : MonoBehaviour
{
    public static PlayerCameraHolder Instance { get; private set; }
    private PlayerInputManager m_playerInputManager;
    [SerializeField] private PlayerMovement m_playerMovement;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_playerTransform;
    [Space]
    [Header("Constants / static readonly")] //its the same apparently
    private static readonly Vector3 s_camHolderLocalCenter = new Vector3(0, 1.5f, 0);
    private static readonly Vector3 s_camHolderRestDirection = new Vector3(0, -1, 4f);
    private static readonly float s_camRestDist = 4f;
    [Space]
    private static readonly float s_camHolderClampAngleMax = 75f;
    private static readonly float s_stickHorFactor = 250f;
    private static readonly float s_stickVerFactor = 250f;
    private static readonly float s_camHolderCenterFollowAcceleration = 5f;
    private static readonly float s_camHolderRotationAcceleration = 6f;
    [Space]
    private static readonly float s_camLocalPosAcceleration = 5f;

    private float m_camHolderClampAngle;
    private Vector3 m_camHolderCenterPosBase;
    private Vector3 m_camHolderCenterPos;
    private Quaternion m_camHolderLookDirection;

    private Quaternion m_WIP_camHolderRotationVerX;
    private Quaternion m_WIP_camHolderRotationHorY;
    private Quaternion m_camHolderRotationVerX;
    private Quaternion m_camHolderRotationHorY;

    private Vector3 m_camPos;

    [SerializeField] private Transform m_chosenLockOnTransform;
     private Transform m_target;
    private Vector3 m_lastTargetPos = Vector3.zero;
    private bool m_isLockOn = false;
    private float lockOnParameter = 0;

    public Vector3 CameraHolderCenterBase { get => m_camHolderCenterPosBase; }
    public Vector3 CameraHolderLookDirection { get => m_camHolderLookDirection.eulerAngles; }
    public Quaternion CameraHolderForwardYAxis { get => Quaternion.Euler(0, m_camHolderLookDirection.eulerAngles.y, 0); }
    //public float LockOnDistance { get => (m_testLockOnTransform.position - m_playerTransform.position).magnitude; }
    public bool IsLockOn { get => m_isLockOn; set { m_isLockOn = value; if (m_isLockOn) m_playerMovement.Target = m_chosenLockOnTransform; else { m_lastTargetPos = TargetPos; m_playerMovement.Target = null; } } }
    public Transform Target { get => m_target; set { m_target = value; m_isLockOn = (m_target != null); } }
    public Vector3 TargetPos { get { if (m_target != null) return m_target.position; else { Debug.Log("target gets called, but is empty"); return m_lastTargetPos; } } }
    public Vector3 CamPos { get => m_camera.transform.position; }



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

        gameObject.transform.SetLocalPositionAndRotation(s_camHolderLocalCenter, Quaternion.LookRotation(s_camHolderRestDirection, Vector3.up));
        m_camera.transform.localPosition = new Vector3(0, 0, -s_camRestDist);
        m_camHolderClampAngle = s_camHolderClampAngleMax;

        m_target = m_chosenLockOnTransform;///////////////////

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
        CalculateAndSetCameraPosAndRot();

        //CameraLookAt();
        //ControlCameraDistance();
    }


    private void CalculateCameraHolderCenter()
    {

        m_camHolderCenterPosBase = UtilityFunctions.SmartLerp(m_camHolderCenterPosBase, m_playerTransform.position, Time.deltaTime * s_camHolderCenterFollowAcceleration);
        m_camHolderCenterPos = m_camHolderCenterPosBase + s_camHolderLocalCenter;

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

        m_camHolderClampAngle = CalculateClampAngleVerX(input, verTurn); 

        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_camHolderRotationHorY *= Quaternion.Euler(0, horTurn * s_stickHorFactor * Time.deltaTime, 0);
        m_WIP_camHolderRotationVerX *= Quaternion.Euler(verTurn * s_stickVerFactor * Time.deltaTime, 0, 0);

        //Apply Clamping
        m_WIP_camHolderRotationVerX = Quaternion.Euler(UtilityFunctions.AngleClamping(m_WIP_camHolderRotationVerX.eulerAngles.x, -m_camHolderClampAngle, m_camHolderClampAngle),0,0);

        ForcingPosition();

        float camHolderRotAcc = m_isLockOn ? UtilityFunctions.SmartLerp(s_camHolderRotationAcceleration, 100f, Mathf.InverseLerp(200, 0, Vector3.Angle(CamPos - m_playerTransform.position, TargetPos - m_playerTransform.position) )) : s_camHolderRotationAcceleration;
        // Die eigentliche Kamera-Rotation wird hier smooth zur WorkInProgress Rotation gezogen | KEIN SMART SLERP HIER!!!
        m_camHolderRotationVerX = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_camHolderRotationVerX, Time.deltaTime * camHolderRotAcc); 
        m_camHolderRotationHorY = Quaternion.Slerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_camHolderRotationHorY, Time.deltaTime * camHolderRotAcc);

        m_camHolderLookDirection = Quaternion.Euler(m_camHolderRotationVerX.eulerAngles.x, m_camHolderRotationHorY.eulerAngles.y, 0);
        
    }




    private void ForcingPosition()
    {
        if (m_playerMovement == null || m_playerMovement.MoveStrenght == 0 && !IsLockOn)
            return;

        float m_desiredDirForceFactor = 0.6f;
        float desiredRotationForce = 0;
        Quaternion desiredRotation = Quaternion.identity;

        if (m_isLockOn)
        {
            Vector3 camRestDir = TargetPos - m_camHolderCenterPos;
            desiredRotationForce = 10;
            desiredRotation = Quaternion.LookRotation(camRestDir);
        }
        else 
        {
            Vector3 camRestDir = s_camHolderRestDirection;
            //here, abhängig nur von input.x, weil beim seitswärts laufen die kamera gedreht wird
            desiredRotationForce = m_desiredDirForceFactor * Mathf.Abs((m_playerMovement.InputDirection * m_playerMovement.MoveStrenght).x); 
            //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
            desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);
        }

        //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt | KEIN SMART SLERP HIER!!!
        m_WIP_camHolderRotationVerX = Quaternion.Slerp(m_WIP_camHolderRotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * desiredRotationForce); 
        m_WIP_camHolderRotationHorY = Quaternion.Slerp(m_WIP_camHolderRotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * desiredRotationForce);

    }

    private float CalculateClampAngleVerX(Vector2 input, float verTurn)
    {
        float clampAppyingAcceleration = 6F;

        if (input.sqrMagnitude >= 0.98f * 0.98f)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngle, Mathf.Abs(verTurn) * s_camHolderClampAngleMax, Time.deltaTime * clampAppyingAcceleration); //this is the clamp and how fast it applies when stick.magnitude is ~1  | 
        else if (m_camHolderClampAngle != s_camHolderClampAngleMax)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngle, s_camHolderClampAngleMax, Time.deltaTime * clampAppyingAcceleration); //this is how fast the clampAngleMax applies when stick.magnitude is less than 1
        
        return s_camHolderClampAngleMax;
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


    private void CalculateAndSetCameraPosAndRot()
    {

        if (m_isLockOn)
        {
            float howMuchRotatingToTarget = 0.6f;
            //offset height is depending on angle
            float camYOffset = UtilityFunctions.RefitRange(UtilityFunctions.Angle180(m_camHolderRotationVerX.eulerAngles.x, false), 0, s_camHolderClampAngleMax, 0.5f, 2); 

            m_camPos = new Vector3(0, camYOffset, -s_camRestDist); 
            lockOnParameter = UtilityFunctions.SmartLerp(lockOnParameter, howMuchRotatingToTarget, Time.deltaTime * 2f);
        }
        else
        {
            m_camPos = new Vector3(0, 0, -s_camRestDist);
            lockOnParameter = UtilityFunctions.SmartLerp(lockOnParameter, 0f, Time.deltaTime * 2f);
        }

        //cameraCenter gets an offset, to look over the players head a bit
        m_camera.transform.localPosition = UtilityFunctions.SmartLerp(m_camera.transform.localPosition, m_camPos, Time.deltaTime * s_camLocalPosAcceleration);
        
        //cameraRotation follows the target and player a bit
        Quaternion lookTotarget = lockOnParameter != 0 ? Quaternion.LookRotation(TargetPos - m_camera.transform.position) : Quaternion.identity;
        Quaternion lookToPlayer = Quaternion.LookRotation(m_camHolderCenterPos - m_camera.transform.position);
        Quaternion lookRotation = Quaternion.Slerp(lookToPlayer, lookTotarget, lockOnParameter);
        m_camera.transform.rotation = lookRotation;

        if (m_playerMovement != null)
            m_playerMovement.CameraYAxisRotation = lookRotation;

    }





    private void CameraLookAt()
    {
        
    }

    private void ControlCameraDistance()
    {
        
    }

}
