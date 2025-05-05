using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static AnimationData;
using GD.MinMaxSlider;


[CreateAssetMenu(fileName = "AnimationData_SO", menuName = "Scriptable Objects/AnimationData")]
public class AnimationData : ScriptableObject
{
    public AnimationInterruptableType AnimationInteruptability = AnimationInterruptableType.Always_Interruptable;
    [Space]
    public bool IsXAxisMirrored = false;
    public bool IsZAxisMirrored = false;
    [Space]
    [Space]
    [Header("Movement Values")]
    public ChangeingValue[] InfluenceOverInputMoveDirection;
    public ChangeingVector[] MoveDirection;
    [Space]
    public ChangeingValue[] InfluenceOverInputMoveSpeed;
    public ChangeingValue[] MoveSpeed;
    [Space]
    public ChangeingValue[] InfluenceOverInputMoveAcceleration;
    public ChangeingValue[] MoveAcceleration;


    [Space]
    [Space]
    [Header("Turning Values")]
    public ChangeingValue[] InfluenceOverInputDesiredFacingRotationDir;
    public ChangeingVector[] DesiredFacingRotationDir;
    [Space]
    public ChangeingValue[] InfluenceOverInputTurningSpeed;
    public ChangeingValue[] TurningSpeed;
    [Space]
    public ChangeingValue[] InfluenceOverInputTurningAcceleration;
    public ChangeingValue[] TurningAcceleration;





    [System.Serializable]
    public class ChangeingValue
    {
        public enum Type
        {
            Constant,
            StartEnd,
            Curve
        }
        [Tooltip("Constant: Only Value needed; StartEnd: Value and StartEnd needed; Curve: Value, StartEnd and Curve needed ")]
            public Type valueType = Type.Constant;
        [Tooltip("Value is the Base Value or the MaxValue when Curve")]
            public float value = 0f;
        [Tooltip("This is in what Part the Value/Curve starts and ends to the animation leght relatively. Outside the Range its 0.")]
            [MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f); 
        [Tooltip("1 is the Value, -1 is -Value. This curve starts and ends at StartEnd to the animation leght relatively.")]
            public AnimationCurve curveValue;

    }

    [System.Serializable]
    public class ChangeingVector
    {
        public enum Type
        {
            ConstantDirection,
            //StartEnd,
            RotateDirection
        }

        [Tooltip("ConstantDirection: Only Direction needed; RotateDirection: Direction, StartEnd and Rotation needed ")]
        public Type valueType = Type.ConstantDirection;
        [Tooltip("Value is the base/start Direction")]
        public Vector3 Direction = Vector3.forward;
        [Tooltip("This is in what Part the Rotation starts and ends. Outside the Range its 0 Rotation.")]
        [MinMaxSlider(0, 1)] public Vector2 startEnd = new Vector2(0f, 1f);
        [Tooltip("1 is the 360, -1 is -360°. This curve starts and ends at StartEnd to the animation leght relatively.")]
        public AnimationCurve Rotation = AnimationCurve.Linear(0,0,0,0);

    }



}
