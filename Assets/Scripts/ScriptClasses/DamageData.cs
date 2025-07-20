using UnityEngine;

public class DamageData //this script is for the data that is created and send through when hitboxes collide and damage is calculated and stored inside this
{
    public int PhysicalSliceDamage = 0;
    public int PhysicalBluntDamage = 0;
    public int PhysicalPierceDamage = 0;
    public Vector2Int ThermicDamageAndBuildUp = new Vector2Int(0, 0);
    public Vector2Int ElectricDamageAndBuildUp = new Vector2Int(0, 0);
    public Vector2Int MetaphysicDamageAndBuildUp = new Vector2Int(0, 0);

    public int ContaminationBuildUpDamage = 0;
    public int PoiseDamage = 0;

    public Vector3 Direction = new Vector3(0, 0, 0);

    public DamageData(int physicalSlice, int physicalBlunt, int physicalPierce, Vector2Int thermic, Vector2Int electric, Vector2Int metaphysic, int contamination, int poise , Vector3 direction)
    {
        PhysicalSliceDamage = physicalSlice;
        PhysicalBluntDamage = physicalBlunt;
        PhysicalPierceDamage = physicalPierce;
        ThermicDamageAndBuildUp = thermic;
        ElectricDamageAndBuildUp = electric;
        MetaphysicDamageAndBuildUp = metaphysic;
        ContaminationBuildUpDamage = contamination;
        PoiseDamage = poise;
        Direction = direction;
    }
}
