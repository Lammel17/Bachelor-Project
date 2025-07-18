using System.Collections.Generic;
using UnityEngine;

public class ProcessedAnimationMovementData
{

    public List<DataStartEnd> rangeValuesList;
    public List<DataCurves> curveValuesList;
    public AnimationData animationData; 
    //public int turningRelations;
    //public float timeSteps;
    //public float animationDuration;

    ////public float crossfadeInTime = 0.2f;
    //public float crossfadeOutTime = 0.2f;
    //public float crossfadeStartBeforeEndTime = 0.2f;

    public ProcessedAnimationMovementData(List<DataStartEnd> StartEndValueList, List<DataCurves> CureveValueList, AnimationData animData/*int TurningRelations, float TimeSteps, float AnimDuration, float CrossfadeOutTime, float CrossfadeOutBeforeEnd*/)
    {
        rangeValuesList = StartEndValueList;
        curveValuesList = CureveValueList;
        animationData = animData;
        //turningRelations = TurningRelations;
        //timeSteps = TimeSteps;
        //animationDuration = Mathf.Max(AnimDuration, 0);
        //crossfadeOutTime = Mathf.Max(CrossfadeOutTime, 0);
        //crossfadeStartBeforeEndTime = Mathf.Max(CrossfadeOutBeforeEnd, 0);
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
