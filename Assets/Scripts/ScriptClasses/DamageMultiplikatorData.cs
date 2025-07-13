using UnityEngine;

    [System.Serializable]
public class DamageMultiplikatorData
{
    public float OverallMultiplicator = 1; //only to physical, thermic, electric, metaphysic
    [Space]
    public float PhysicalFactor = 1;
    public float ThermicFactor = 1;
    public float ElectricFactor = 1;
    public float MetaphysicFactor = 1;
    [Space]
    public float AilmentsFactor = 1;
    public float PoiseDamageFactor = 1;

    public DamageMultiplikatorData(float overallMultiplicator, float physicalFactor, float thermicFactor, float electricFactor, float metaphysicFactor, float ailmentFactor, float poiseFactor)
    {
        OverallMultiplicator = overallMultiplicator;
        PhysicalFactor = physicalFactor;
        ThermicFactor = thermicFactor;
        ElectricFactor = electricFactor;
        MetaphysicFactor = metaphysicFactor;
        AilmentsFactor = ailmentFactor;
        PoiseDamageFactor = poiseFactor;
    }
}
