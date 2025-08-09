using UnityEngine;

[System.Serializable]
public class ShieldDamageNegationData
{
    [Header("Range from 0 to 1")]
    public float OverallMultiplicator = 0; //only to physical, thermic, electric, metaphysic
    [Space]
    public float PhysicalNegation = 0;
    public float ThermicNegation = 0;
    public float ElectricNegation = 0;
    public float MetaphysicNegation = 0;
    //[Space]
    //public float 

    public ShieldDamageNegationData(float overallMultiplicator, float physicalNegation, float thermicNegation, float electricNegation, float metaphysicNegation)
    {
        OverallMultiplicator = overallMultiplicator;
        PhysicalNegation = physicalNegation;
        ThermicNegation = thermicNegation;
        ElectricNegation = electricNegation;
        MetaphysicNegation = metaphysicNegation;
    }
}
