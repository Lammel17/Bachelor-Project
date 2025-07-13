using UnityEngine;

[System.Serializable]
public class DamageTableData //this is the table is for weapons which contains the data of its damage from min to max weapon level
{
    public Vector2 PhysicalSlice = new Vector2(0, 0);
    public Vector2 PhysicalBlunt = new Vector2(0, 0);
    public Vector2 PhysicalPierce = new Vector2(0, 0);
    public Vector2 Thermal = new Vector2(0, 0);
    public Vector2 Electrical = new Vector2(0, 0);
    public Vector2 Metaphysical = new Vector2(0, 0);

    public Vector2 CorrosionBuildUp = new Vector2(0, 0);
    public Vector2 Poise = new Vector2(0, 0);

    public AnimationCurve UpgradeCurve = AnimationCurve.Linear(0, 0, 1, 1);
}
