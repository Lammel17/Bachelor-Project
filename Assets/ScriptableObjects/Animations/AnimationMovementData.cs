using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EditorAttributes;
using GD.MinMaxSlider;



[CreateAssetMenu(fileName = "MovementData", menuName = "Scriptable Objects/MovementData")]
public class AnimationMovementData : ScriptableObject
{
    [SerializeField] public string MovementDescription = "";

    [Space]
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1")]
    [SerializeField] public InfluenceValuePredefinitions moveInfluence = InfluenceValuePredefinitions.NoInputInfluence;
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1")]
    [SerializeField] public InfluenceValuePredefinitions turningInfluence = InfluenceValuePredefinitions.NoInputInfluence;
    [Space]
    [Header("Initial Directions by:")]
    [Tooltip("1: will use the latestInput ss starting orientation. 2: will use the latest actual Orientation as starting orientation.")]
    [SerializeField] public MoveDirectionPredefinitions moveDirPredefinition = MoveDirectionPredefinitions.LatestInput;
    [Tooltip("1: will use the latestInput ss starting orientation. 2: will use the latest actual Orientation as starting orientation. 3. will use the latest Orientation without the additional rotation as starting orientation . ")]
    [SerializeField] public TurningDirectionPredefinitions turningDirPredefinition = TurningDirectionPredefinitions.LatestInput;
    [Header("Target and Turning Relations")]
    [Tooltip("Applies only when LockOn ")]
    [SerializeField] public TargetRelations targetRelations = TargetRelations.None;
    [SerializeField] public TurningRelations turningRelations = TurningRelations.None;
    [Tooltip("only needed if InputInfluence for TurningDir is used in any way")]
    [SerializeField] public bool forbidAdditinalRotation = false;
    [Space]
    [Header("Animation Parameters for Movement and Rotation")]
    [SerializeField] public Values[] variableValue = new Values[0];

    public float timeStepsForCurves = 0.05f;

    public enum MoveDirectionPredefinitions
    {
        LatestInput = 1,    //Will use the latest inputDir and TurningDir as StartingPoint
        LatestFrame,
    }

    public enum TurningDirectionPredefinitions
    {
        LatestInput = 1,    
        //LatestInputWithoutAddTurning,
        LatestFrame
    }

    public enum InfluenceValuePredefinitions
    {
        NoInputInfluence = 1,  
        FullInputInfluence
        
    }

    public enum TurningRelations
    {
        None = 0,
        TurningDirFollowsMoveDir, 
        MoveDirFollowsTurningDir
    }
    public enum TargetRelations
    {
        None = 0,
        TurningDirFollowsTarget,
        MoveDirFollowsTarget
    }


    public enum ValueName
    {
        Move_Direction_Angle,
        Move_Speed,
        Move_Acceleration,
        Turning_Direction_Angle,
        Max_Turning_Speed,
        Turning_Strenght,
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
                //[Tooltip("Apply the last value in the animation as the new current value")]
                //public bool applyValue = true;
                [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0. If Value is a Angle, it will rotate it in the given Range")]
                [GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f);
                [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively. If Value is a Angle, it will rotate it (1 = 360°)")]
                public AnimationCurve curveValue;
            }



            [Space]
            public Influence customInfluenceOverInput;

            [System.Serializable]
            public class Influence
            {
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



}