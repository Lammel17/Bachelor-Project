using UnityEngine;



public enum ValueType
{
    //Inactive,
    ConstantValue,
    StartEndValue,
    CurvedValue
}

public enum InfluenceValueType
{
    FullInfluence,
    ConstantInfluence,
    StartEndInfluence,
    CurvedInfluence
}
[System.Serializable]
public class MovementValuesData
{
    [LabelOverride(" ")] public bool isInUse = false;
    [LabelOverride(" ")] public ValueType valueType = ValueType.ConstantValue;
    [Tooltip("Value is the Base Value or the MaxValue when Curve/ Or is the Angle when a Direction.")]
    public float value = 0f;
    [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0. If Value is a Angle, it will rotate it in the given Range")]
    [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f);
    [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively. If Value is a Angle, it will rotate it (1 = 360°)")]
    public AnimationCurve curve;



    [Tooltip("This allows to mix the value with the player values, can be used to fade in and out")]
    [SerializeField] public InfluenceValueType influenceType = InfluenceValueType.FullInfluence;
    [Tooltip("Influence is the Base Value or the MaxValue when Curve")]
    [Range(0, 1)] public float influence = 1f;
    [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0.")]
    [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 influenceStartEnd = new Vector2(0f, 1f);
    [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively.")]
    public AnimationCurve influenceCurve;






}
