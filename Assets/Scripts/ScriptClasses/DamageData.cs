using UnityEngine;

public class DamageData //this script is for the data that is created and send through when hitboxes collide and damage is calculated and stored inside this
{
    public int PoiseDamage = 0;
    public int PhysicalSliceDamage = 0;
    public int PhysicalBluntDamage = 0;
    public int PhysicalPierceDamage = 0;
    public int ThermicDamage = 0;
    public int ElectricDamage = 0;
    public int MetaphysicDamage = 0;

    public int ContaminationBuildUpDamage = 0;

    public DamageData(int poise, int physicalSlice, int physicalBlunt, int physicalPierce, int thermic, int electric, int metaphysic, int contamination)
    {
        PoiseDamage = poise;
        PhysicalSliceDamage = physicalSlice;
        PhysicalBluntDamage = physicalBlunt;
        PhysicalPierceDamage = physicalPierce;
        ThermicDamage = thermic;
        ElectricDamage = electric;
        MetaphysicDamage = metaphysic;
        ContaminationBuildUpDamage = contamination;
    }
}
