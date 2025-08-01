using NUnit.Framework;
using UnityEngine;

public static class CombatUtils
{
    [System.Serializable]
    public enum PhysicalDamageType
    {
        TypeByBase = 0,
        Slice,
        Blunt,
        Pierce,
        None
    }

    public static DamageData CalculateBaseDamageData(DamageTableData tableData, float levelFactor)
    {
        int physicalSlice = (int)(Mathf.Lerp(tableData.PhysicalSlice.x, tableData.PhysicalSlice.y, levelFactor));
        int physicalBlunt = (int)(Mathf.Lerp(tableData.PhysicalBlunt.x, tableData.PhysicalBlunt.y, levelFactor));
        int physicalPierce = (int)(Mathf.Lerp(tableData.PhysicalPierce.x, tableData.PhysicalPierce.y, levelFactor));

        int thermic = (int)(Mathf.Lerp(tableData.Thermic.x, tableData.Thermic.y, levelFactor));
        int electric = (int)(Mathf.Lerp(tableData.Electric.x, tableData.Electric.y, levelFactor));
        int metaphysic = (int)(Mathf.Lerp(tableData.Metaphysic.x, tableData.Metaphysic.y, levelFactor));

        DamageData dmgDat = new DamageData(
            physicalSlice,
            physicalBlunt,
            physicalPierce,
            new Vector2Int(thermic, 0), 
            new Vector2Int(electric, 0),
            new Vector2Int(metaphysic, 0),
            (int)(Mathf.Lerp(tableData.ContaminationBuildUp.x, tableData.ContaminationBuildUp.y, levelFactor)),
            (int)(Mathf.Lerp(tableData.Poise.x, tableData.Poise.y, levelFactor)),
            Vector3.forward);

        return dmgDat;
    }

    public static int CalculateElementBuildUp(int elementDamage, int physicalDamage)
    {
        //this formula is provisorical 
        int minElementThreshhold = 30;

        int buildUp = (int)UtilityFunctions.RefitToNewRange(elementDamage - minElementThreshhold, 0, elementDamage + physicalDamage - minElementThreshhold, 0, elementDamage * 0.1f);
        //int buildUp = (int)Mathf.Lerp(0, elementDamage * 0.1f, Mathf.InverseLerp(0, elementDamage + physicalDamage - minElementThreshhold, elementDamage - minElementThreshhold));

        return buildUp;
    }

    public static DamageData CalculateActionDamageData(DamageData baseDmgDat, DamageMultiplikatorData actionDamageMultiplikator, Vector3 playerDirection, PhysicalDamageType physicaltype)
    {
        int physicalSlice = physicaltype != PhysicalDamageType.Slice ? 0 : (int)(baseDmgDat.PhysicalSliceDamage * actionDamageMultiplikator.PhysicalFactor * actionDamageMultiplikator.OverallMultiplicator);
        int physicalBlunt = physicaltype != PhysicalDamageType.Blunt ? 0 : (int)(baseDmgDat.PhysicalBluntDamage * actionDamageMultiplikator.PhysicalFactor * actionDamageMultiplikator.OverallMultiplicator);
        int physicalPierce = physicaltype != PhysicalDamageType.Pierce ? 0 : (int)(baseDmgDat.PhysicalPierceDamage * actionDamageMultiplikator.PhysicalFactor * actionDamageMultiplikator.OverallMultiplicator);

        int thermic = (int)(baseDmgDat.ThermicDamageAndBuildUp.x * actionDamageMultiplikator.ThermicFactor * actionDamageMultiplikator.OverallMultiplicator);
        int electric = (int)(baseDmgDat.ElectricDamageAndBuildUp.x * actionDamageMultiplikator.ElectricFactor * actionDamageMultiplikator.OverallMultiplicator);
        int metaphysic = (int)(baseDmgDat.MetaphysicDamageAndBuildUp.x * actionDamageMultiplikator.MetaphysicFactor * actionDamageMultiplikator.OverallMultiplicator);

        DamageData dmgDat = new DamageData(
            physicalSlice,
            physicalBlunt,
            physicalPierce,
            new Vector2Int(thermic, CalculateElementBuildUp(thermic, physicalSlice + physicalBlunt + physicalPierce)),
            new Vector2Int(electric, CalculateElementBuildUp(electric, physicalSlice + physicalBlunt + physicalPierce)),
            new Vector2Int(metaphysic, CalculateElementBuildUp(metaphysic, physicalSlice + physicalBlunt + physicalPierce)),
            (int)(baseDmgDat.ContaminationBuildUpDamage * actionDamageMultiplikator.AilmentsFactor),
            (int)(baseDmgDat.PoiseDamage * actionDamageMultiplikator.PoiseDamageFactor),
            Quaternion.LookRotation(playerDirection) * baseDmgDat.Direction);

        return dmgDat;
    }

    public static DamageData CalculateMultiplicatedDamageData(DamageMultiplikatorData damageMultiplikator, DamageData damageData)
    {
        DamageData dmgDat = new DamageData(
            (int)(damageData.PhysicalSliceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalBluntDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalPierceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            new Vector2Int((int)(damageData.ThermicDamageAndBuildUp.x * damageMultiplikator.ThermicFactor * damageMultiplikator.OverallMultiplicator), damageData.ThermicDamageAndBuildUp.y),
            new Vector2Int((int)(damageData.ElectricDamageAndBuildUp.x * damageMultiplikator.ElectricFactor * damageMultiplikator.OverallMultiplicator), damageData.ElectricDamageAndBuildUp.y),
            new Vector2Int((int)(damageData.MetaphysicDamageAndBuildUp.x * damageMultiplikator.MetaphysicFactor * damageMultiplikator.OverallMultiplicator), damageData.MetaphysicDamageAndBuildUp.y),
            (int)(damageData.ContaminationBuildUpDamage * damageMultiplikator.AilmentsFactor),
            (int)(damageData.PoiseDamage * damageMultiplikator.PoiseDamageFactor),
            damageData.Direction);

        return dmgDat;

    }
}
