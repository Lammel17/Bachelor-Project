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
    [Tooltip("1: will use the latestInput ss starting orientation. 2: willuse the latest actual Orientation as starting orientation. 3. Will override the influence to 0. 4. Will ")]
    [SerializeField] public DirectionPredefinitions moveDirPredefinition = DirectionPredefinitions.LatestInput;
    [Tooltip("1: will use the latestInput ss starting orientation. 2: willuse the latest actual Orientation as starting orientation. 3. Will override the influence to 0. 4. Will ")]
    [SerializeField] public DirectionPredefinitions turningDirPredefinition = DirectionPredefinitions.LatestInput;
    [Space]
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1")]
    [SerializeField] public InfluenceValuePredefinitions moveInfluence = InfluenceValuePredefinitions.InitialAllWithFullInfluence;
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1")]
    [SerializeField] public InfluenceValuePredefinitions turningInfluence = InfluenceValuePredefinitions.InitialAllWithFullInfluence;
    [Space]
    [Tooltip("  ")]
    [SerializeField] public InitialRelations relations = InitialRelations.None;
    [Space]
    [Header("Animation Parameters for Movement and Rotation")]
    [SerializeField] public Values[] variableValue;

    public float timeStepsForCurves = 0.05f;

    public enum DirectionPredefinitions
    {
        LatestInput = 1,    //Will use the latest inputDir and TurningDir as StartingPoint
        LatestFrame,
        //InputOnly,
        //Freeze,

    }
    public enum InfluenceValuePredefinitions
    {
        InitialAllWithFullInfluence = 1,    //Will use the latest inputDir and TurningDir as StartingPoint
        InitialAllWithNoInfluence
        

    }

    public enum InitialRelations
    {
        None = 1,
        TurningDirFollowsMoveDir,    //Will use the latest inputDir and TurningDir as StartingPoint
        MoveDirFollowsTurningDir


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
    public class Values
    {

        [SerializeField] public ValueName valueName;
        public bool ignore = false;

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
                ////////////////////public bool applyValue = true;
                [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0. If Value is a Angle, it will rotate it in the given Range")]
                [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f);
                [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively. If Value is a Angle, it will rotate it (1 = 360°)")]
                public AnimationCurve curveValue;
            }



            [Space]
            [Header("Influence over Input")]
            [Tooltip("Constant: Only Value needed; StartEnd: Value and StartEnd needed; Curve: Value, StartEnd and Curve needed ")]
            public InfluenceValueType influenceType = InfluenceValueType.ConstantInfluence;
            [Tooltip("Value is the Base Value or the MaxValue when Curve")]
            [Range(0, 1)] public float influence = 1f;

            [Tooltip("Settings only needed when Type is not Constant")]
            public ValueSettings influenceSettings;
            [System.Serializable]
            public class InfluenceSettings
            {
                [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0.")]
                [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 influenceStartEnd = new Vector2(0f, 1f);
                [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively.")]
                public AnimationCurve influenceCurve;
            }
        }


    }



}