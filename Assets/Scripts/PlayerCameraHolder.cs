using EditorAttributes;
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
    [SerializeField] private CharacterActionAndMovementHandler m_playerMovement;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_playerTransform;
    [Space]
    //[Header("Camera Holder")]
    private Vector3 s_camHolderLocalCenter = new Vector3(0, 1.5f, 0);
    private Vector3 s_camHolderRestDirection = new Vector3(0, -2, 4f);
    private float s_distCenterToCam = 4f;
    [Space]
    private float s_camHolderClampAngleXAxisMax = 75f;
    private float s_horizontalInputStrenght = 250f;
    private float s_verticalInputStrenght = 250f;
    private float s_accelerationOfCamHolderFollowPlayer = 5f;
    private float s_accelerationOfCamHolderRotation = 6f;
    [Space]

    private float m_camHolderClampAngleXAxis;
    private Vector3 m_camHolderCenterPosBase;
    private Vector3 m_camHolderCenterPos;
    private Quaternion m_camHolderLookDirection;

    private Quaternion m_WIP_camHolderRotationVerX;
    private Quaternion m_WIP_camHolderRotationHorY;
    private Quaternion m_camHolderRotationVerX;
    private Quaternion m_camHolderRotationHorY;

    [Header("Camera itfelf")]
    [SerializeField] private Transform m_chosenLockOnTransform;
    [SerializeField] [Range(0,1)]private float m_lookToPlayerOrTargetFactor = 0.6f;
    [SerializeField] private Vector2 m_additionalCamHeightLockOn = new Vector2(0.5f, 2);
    [SerializeField][EditorAttributes.ReadOnly][Range(0,1)]private float m_lockOnApplyance = 0;

    private Vector3 m_camPos;

    private Transform m_target;
    private Vector3 m_lastTargetPos = Vector3.zero;
    private Vector3 m_lastLocalForwardOfCam = Vector3.zero;
    private bool m_isLockOn = false;
    private float m_camLockOnSpeed = 4f;

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
        m_camera.transform.localPosition = new Vector3(0, 0, -s_distCenterToCam);
        m_camHolderClampAngleXAxis = s_camHolderClampAngleXAxisMax;

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

        m_camHolderCenterPosBase = UtilityFunctions.SmartLerp(m_camHolderCenterPosBase, m_playerTransform.position, Time.deltaTime * s_accelerationOfCamHolderFollowPlayer);
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

        m_camHolderClampAngleXAxis = CalculateClampAngleVerX(input, verTurn); 

        //Kameraposition als Vertikal und Horizontal Drehung, da wo ich sie linear hinschiebe mit den Right-Stick Input
        m_WIP_camHolderRotationHorY *= Quaternion.Euler(0, horTurn * s_horizontalInputStrenght * Time.deltaTime, 0);
        m_WIP_camHolderRotationVerX *= Quaternion.Euler(verTurn * s_verticalInputStrenght * Time.deltaTime, 0, 0);

        //Apply Clamping
        m_WIP_camHolderRotationVerX = Quaternion.Euler(UtilityFunctions.AngleClamping(m_WIP_camHolderRotationVerX.eulerAngles.x, -m_camHolderClampAngleXAxis, m_camHolderClampAngleXAxis),0,0);

        ForcingPosition();

        float camHolderRotAcc = m_isLockOn ? UtilityFunctions.SmartLerp(s_accelerationOfCamHolderRotation, 100f, Mathf.InverseLerp(200, 0, Vector3.Angle(CamPos - m_playerTransform.position, TargetPos - m_playerTransform.position) )) : s_accelerationOfCamHolderRotation;
        // Die eigentliche Kamera-Rotation wird hier smooth zur WorkInProgress Rotation gezogen | KEIN SMART SLERP HIER!!!
        m_camHolderRotationVerX = Quaternion.Slerp(Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0), m_WIP_camHolderRotationVerX, Time.deltaTime * camHolderRotAcc); 
        m_camHolderRotationHorY = Quaternion.Slerp(Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), m_WIP_camHolderRotationHorY, Time.deltaTime * camHolderRotAcc);

        m_camHolderLookDirection = Quaternion.Euler(m_camHolderRotationVerX.eulerAngles.x, m_camHolderRotationHorY.eulerAngles.y, 0);
        
    }




    private void ForcingPosition()
    {
        //if (m_playerMovement == null || m_playerMovement.MoveStrenght == 0 && !IsLockOn || !m_isLockOn)
        //    return;

        float desiredRotationForceVerX = 0;
        float desiredRotationForceHorY = 0;
        Quaternion desiredRotation = Quaternion.identity;

        if (m_isLockOn)
        {
            Vector3 camRestDir = TargetPos - m_camHolderCenterPos;
            desiredRotationForceVerX =  10;
            desiredRotationForceHorY =/* Mathf.InverseLerp(-10, 45, Vector2.Angle(new Vector2(camRestDir.x, camRestDir.z), new Vector2(transform.forward.x, transform.forward.z))) * */10; //if almost in center, force drops down
            desiredRotation = Quaternion.LookRotation(camRestDir);
        }
        else 
        {
            return;/////////////////////////////////////////////////Just for test
            float m_desiredDirForceFactor = 0.25f;
            Vector3 camRestDir = s_camHolderRestDirection;
            //here, abhängig nur vom seitwärts laufen, weil beim seitswärts laufen die kamera gedreht wird
            desiredRotationForceVerX = desiredRotationForceHorY = m_desiredDirForceFactor * Mathf.Abs((Quaternion.Inverse(CameraHolderForwardYAxis) * m_playerMovement.PreviousMove).x); 
            //Die Gewünschte End-Drehung von der Aktuellen Dreh-Richtung aus
            desiredRotation = m_playerTransform.transform.rotation * Quaternion.LookRotation(camRestDir);
        }

        //Die InputRichtung wird hier beim Laufen smooth zu desiredRotation gelenkt | KEIN SMART SLERP HIER!!!
        m_WIP_camHolderRotationVerX = Quaternion.Slerp(m_WIP_camHolderRotationVerX, Quaternion.Euler(desiredRotation.eulerAngles.x, 0, 0), Time.deltaTime * desiredRotationForceVerX); 
        m_WIP_camHolderRotationHorY = Quaternion.Slerp(m_WIP_camHolderRotationHorY, Quaternion.Euler(0, desiredRotation.eulerAngles.y, 0), Time.deltaTime * desiredRotationForceHorY);

    }

    private float CalculateClampAngleVerX(Vector2 input, float verTurn)
    {
        float clampAppyingAcceleration = 6F;

        if (input.sqrMagnitude >= 0.98f * 0.98f)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngleXAxis, Mathf.Abs(verTurn) * s_camHolderClampAngleXAxisMax, Time.deltaTime * clampAppyingAcceleration); //this is the clamp and how fast it applies when stick.magnitude is ~1  | 
        else if (m_camHolderClampAngleXAxis != s_camHolderClampAngleXAxisMax)
            return UtilityFunctions.SmartLerp(m_camHolderClampAngleXAxis, s_camHolderClampAngleXAxisMax, Time.deltaTime * clampAppyingAcceleration); //this is how fast the clampAngleMax applies when stick.magnitude is less than 1
        
        return s_camHolderClampAngleXAxisMax;
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
            m_lockOnApplyance = UtilityFunctions.SmartLerp(m_lockOnApplyance, 1, Time.deltaTime * m_camLockOnSpeed);
        else
            m_lockOnApplyance = UtilityFunctions.SmartLerp(m_lockOnApplyance, 0f, Time.deltaTime * m_camLockOnSpeed);

        //offset height is depending on angle
        float camYOffset = UtilityFunctions.RefitToNewRange(UtilityFunctions.Angle180(m_camHolderRotationVerX.eulerAngles.x), 0, s_camHolderClampAngleXAxisMax, m_additionalCamHeightLockOn.x, m_additionalCamHeightLockOn.y);
        //float camYOffset = m_lockOnApplyance <= 0.0001 ? 0 : Mathf.Lerp(m_additionalCamHeightLockOn.x, m_additionalCamHeightLockOn.y, Mathf.InverseLerp( 0, s_camHolderClampAngleXAxisMax, UtilityFunctions.Angle180(m_camHolderRotationVerX.eulerAngles.x))); 

        //cameraCenter gets an offset, to look over the players head a bit
        //m_camera.transform.localPosition = UtilityFunctions.SmartLerp(m_camera.transform.localPosition, m_camPos, Time.deltaTime * s_camLocalPosAcceleration);
        float height = UtilityFunctions.SmartLerp(0, camYOffset, m_lockOnApplyance);
        m_camera.transform.localPosition = new Vector3(0, height, -s_distCenterToCam);


        if (m_isLockOn) m_lastLocalForwardOfCam = Quaternion.Inverse(transform.rotation) * (TargetPos - m_camera.transform.position);
        Vector3 lookTotarget = (m_lockOnApplyance != 0) ? transform.rotation * m_lastLocalForwardOfCam : Vector3.forward;
        Vector3 lookToPlayer = m_camHolderCenterPos - m_camera.transform.position;
        Vector3 lookDir = Vector3.Slerp(lookToPlayer, lookTotarget, m_lockOnApplyance * m_lookToPlayerOrTargetFactor);
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
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
