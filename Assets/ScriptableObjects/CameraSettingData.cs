using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettingData", menuName = "Scriptable Objects/CameraSettingData")]
public class CameraSettingData : ScriptableObject
{
    [Header("Camera Holder")]
    public Vector2 HorAndVerInputStrenght = new Vector2(250, 250);
    public Vector3 CamHolder_RestDirection = new Vector3(0, -2, 4f);
    public float CamHolder_ClampAngleXAxisMax = 75f;
    [Space]
    public Vector3 CamHolder_LocalCenter = new Vector3(0, 1.5f, 0);
    public float DistCenterToCam = 4f;
    [Space]
    public float CamHolder_RotationForceByDrag = 0.13f;
    public float CamHolder_AccelerationOfFollowPlayer = 5f;
    public float CamHolder_AccelerationOfRotation = 6f;
    [Space]
    [Header("Camera itfelf")]
    [Range(0, 1)] public float LookToPlayerOrTargetFactor = 0.6f;
    public Vector2 AdditionalCamHeightLockOn = new Vector2(0.5f, 2);
    public float Cam_LockOnSpeed = 5f;
}
