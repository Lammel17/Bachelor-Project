using System;
using System.Collections.Generic;
using UnityEngine;

public class ProcessedAnimationMovementData
{

    public List<DataStartEnd> RangeValuesList;
    public List<DataCurves> CurveValuesList;
    public AnimationData AnimationData;
    public List<Action> Effects;


    public ProcessedAnimationMovementData(List<DataStartEnd> startEndValueList, List<DataCurves> cureveValueList, AnimationData animData, List<Action> effects)
    {
        RangeValuesList = startEndValueList;
        CurveValuesList = cureveValueList;
        AnimationData = animData;
        Effects = effects;

    }

    public class DataCurves
    {
        public ValueName name;
        public float value;
        public Vector2 startEnd;
        public AnimationCurve curve = null;

        public DataCurves(ValueName n, float v, Vector2 se, AnimationCurve c = null)
        {
            name = n;
            value = v;
            startEnd = se;
            curve = c;
        }
    }

    public class DataStartEnd
    {
        public ValueName name;
        public float value;
        public Vector2 startEnd;
        //public float TimeFactor = 1;

        public DataStartEnd(ValueName n, float v, Vector2 se)
        {
            name = n;
            value = v;
            startEnd = se;
        }
    }


    public enum ValueName
    {
        Move_Direction_Angle,
        InfluenceOn_Move_Direction_Angle,
        Move_Speed,
        InfluenceOn_Move_Speed,
        Move_Acceleration,
        InfluenceOn_Move_Acceleration,
        Turning_Direction_Angle,
        InfluenceOn_Turning_Direction_Angle,
        Turning_Strenght,
        InfluenceOn_Turning_Strenght,
        Max_Turning_Speed,
        InfluenceOn_Max_Turning_Speed,
    }
}
