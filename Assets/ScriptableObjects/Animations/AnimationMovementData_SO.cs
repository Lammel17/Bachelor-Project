using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static AnimationData;
using EditorAttributes;
using GD.MinMaxSlider;



[CreateAssetMenu(fileName = "AnimationMovement", menuName = "Scriptable Objects/AnimationMovement_SO")]
public class AnimationMovementData : ScriptableObject
{
    [SerializeField] public string ActionDescription = "";


    [Header("Predefined Calculation")]
    [SerializeField] public Predefinitions movePredefinition;
    [SerializeField] public Predefinitions turningPredefinition;
    [Space]
    [Header("Animation Parameters for Movement and Rotation")]
    [SerializeField] public Value[] variableValue;

    public enum Predefinitions
    {
        LatestInput = 1,
        LatestFrame,
        InputOnly,
        Freeze,

    }

    public enum ValueName
    {
        Move_Direction_Angle,
        Move_Speed,
        Move_Acceleration,
        Turning_Direction_Angle,
        Turning_Speed,
        Turning_Acceleration,
    }
    public enum ValueType
    {
        ConstantValue,
        StartEndValue,
        CurvedValue
    }
    public enum InfluenceValueType
    {
        ConstantInfluence,
        StartEndInfluence,
        CurvedInfluence
    }


    [System.Serializable]
    public class Value
    {

        [SerializeField] public ValueName valueName;

        public Settings settings;
        [System.Serializable]
        public class Settings
        {
            [Header("The Value and how it may change")]
            [Tooltip("Constant: Only Value needed; StartEnd: Value and StartEnd needed; Curve: Value, StartEnd and Curve needed ")]
            public ValueType valueType = ValueType.ConstantValue;
            [Tooltip("Value is the Base Value or the MaxValue when Curve/ Or is the Angle when a Direction.")]
            public float value = 0f;

            [Tooltip("Settings only needed when Type is not Constant")]
            public ValueSettings valueSettings;
            [System.Serializable]
            public class ValueSettings
            {
                [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0. If Value is a Angle, it will rotate it in the given Range")]
                [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f);
                [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively. If Value is a Angle, it will rotate it (1 = 360°)")]
                public AnimationCurve curveValue;
            }



            [Space]
            [Header("Influence over Input")]
            [Tooltip("Constant: Only Value needed; StartEnd: Value and StartEnd needed; Curve: Value, StartEnd and Curve needed ")]
            public InfluenceValueType influenceValueType = InfluenceValueType.ConstantInfluence;
            [Tooltip("Value is the Base Value or the MaxValue when Curve")]
            [Range(0, 1)] public float influenceValue = 1f;

            [Tooltip("Settings only needed when Type is not Constant")]
            public ValueSettings influenceSettings;
            [System.Serializable]
            public class InfluenceSettings
            {
                [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0.")]
                [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 influenceStartEnd = new Vector2(0f, 1f);
                [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively.")]
                public AnimationCurve influenceCurveValue;
            }
        }


    }



}