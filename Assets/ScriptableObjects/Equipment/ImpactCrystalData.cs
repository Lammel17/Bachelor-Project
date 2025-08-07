using UnityEngine;

[CreateAssetMenu(fileName = "ImactCrystalData", menuName = "Scriptable Objects/ImactCrystalData")]
public class ImpactCrystalData
    : ScriptableObject
{
    public int MaxEnergyPointsGain = 0;
    public AnimationCurve AbsorbtionCurveSpeed;
    public float AbsorbtionCurveDuration;
}
