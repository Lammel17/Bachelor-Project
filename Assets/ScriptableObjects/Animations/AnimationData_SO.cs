using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static AnimationData;


[CreateAssetMenu(fileName = "AnimationData_SO", menuName = "Scriptable Objects/AnimationData")]
public class AnimationData : ScriptableObject
{
    public AnimationInterruptableType AnimationInteruptability = AnimationInterruptableType.Always_Interruptable;

    [Header("Movement Values")]
    public ChangeingValue[] InfluenceOverInputMoveDirection;
    public ChangeingVector[] MoveDirection;
    [Space]
    public ChangeingValue[] InfluenceOverInputMoveSpeed;
    public ChangeingVector[] MoveSpeed;
    [Space]
    public ChangeingValue[] InfluenceOverInputMoveAcceleration;
    public ChangeingVector[] MoveAcceleration;


    [Space]
    [Space]
    [Header("Turning Values")]
    public ChangeingValue[] InfluenceOverInputDesiredFacingRotationDir;
    public ChangeingVector[] DesiredFacingRotationDir;
    [Space]
    public ChangeingValue[] InfluenceOverInputTurningSpeed;
    public ChangeingVector[] TurningSpeed;
    [Space]
    public ChangeingValue[] InfluenceOverInputTurningAcceleration;
    public ChangeingVector[] TurningAcceleration;





    [System.Serializable]
    public class ChangeingValue
    {
        public enum ValueType
        {
            Constant,
            StartEnd,
            Curve
        }

        public ValueType valueType = ValueType.Constant;
        public float constantValue = 0f;
        /*[MinMaxSlider(0, 1)]*/ public Vector2 sliderRange = new Vector2(0f, 1f); // Min and max values
        public AnimationCurve curveValue = AnimationCurve.Linear(0, 0, 1, 1);

    }

    [System.Serializable]
    public class ChangeingVector
    {
        public enum ValueType
        {
            ConstantDirection,
            RotateDirection
        }

        public ValueType valueType = ValueType.ConstantDirection;
        public Vector3 StartDirection = Vector3.zero;
        public AnimationCurve Rotation = AnimationCurve.Linear(0, 0, 1, 1);

    }



}
