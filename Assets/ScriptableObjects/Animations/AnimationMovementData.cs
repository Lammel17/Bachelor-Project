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
    [SerializeField] public bool m_disableSidewardMovement = false;
    [Space]
    [Header("Movement Parameters")]
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1")]
    [LabelOverride("Inactive Parameters")][SerializeField] public InfluenceValuePredefinitions moveInfluence = InfluenceValuePredefinitions.NoInputInfluence;
    [Space]
    [SerializeField] public MovementValuesData moveDirection = new MovementValuesData();
    [Space]
    [SerializeField] public MovementValuesData moveSpeed = new MovementValuesData();
    [Space]
    [SerializeField] public MovementValuesData moveAcceleration = new MovementValuesData();
    [Space]
    [Header("Turning Parameters")]
    [Tooltip("sets if the initial Influence value of dir, speed and acc is set to 0 or 1, ")]
    [LabelOverride("Inactive Parameters")][SerializeField] public InfluenceValuePredefinitions turningInfluence = InfluenceValuePredefinitions.NoInputInfluence;
    [Space]
    [SerializeField] public MovementValuesData turningDirection = new MovementValuesData();
    [Space]
    [SerializeField] public MovementValuesData turningMaxSpeed = new MovementValuesData();
    [Space]
    [SerializeField] public MovementValuesData turningStrenght = new MovementValuesData();
    [Space]
    [Space]
    [Space]
    [Space]
    [Space]
    public float timeStepsForCurves = 0.05f;
    [Space]
    [Space]
    [Space]
    [Space]
    [Space]
    [SerializeField][ReadOnly] private string tipp = "Base Player Values: 0/4/20 0/14/15 ";

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
        [InspectorName("Acts as Value 0")]
        NoInputInfluence = 1,
        [InspectorName("Gets Ignored")]
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


    //public enum ValueName
    //{
    //    Move_Direction_Angle,
    //    Move_Speed,
    //    Move_Acceleration,
    //    Turning_Direction_Angle,
    //    Max_Turning_Speed,
    //    Turning_Strenght,
    //}

    



}