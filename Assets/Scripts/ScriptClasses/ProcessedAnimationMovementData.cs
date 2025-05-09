using System.Collections.Generic;
using UnityEngine;

public class ProcessedAnimationMovementData
{

    public List<DataStartEnd> rangeValuesList;
    public List<DataCurves> curveValuesList;
    public int noneMoveTurningRelations;
    public float timeSteps;
    public float animationDuration;

    public ProcessedAnimationMovementData(List<DataStartEnd> se, List<DataCurves> c, int r, float t, float d)
    {
        rangeValuesList = se;
        curveValuesList = c;
        noneMoveTurningRelations = r;
        timeSteps = t;
        animationDuration = d;
    }

    public class DataCurves
    {
        public ValueName name;
        public float value;
        public Vector2 startEnd;
        public AnimationCurve curve = null;
        //public float TimeFactor = 1;

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
        Turning_Speed,
        InfluenceOn_Turning_Speed,
        Turning_Acceleration,
        InfluenceOn_Turning_Acceleration,
    }
}
