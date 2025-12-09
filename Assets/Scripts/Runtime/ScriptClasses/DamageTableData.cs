using UnityEngine;

[System.Serializable]
public class DamageTableData //this is the table is for weapons which contains the data of its damage from min to max weapon level
{
    public Vector2Int PhysicalSlice = new Vector2Int(0, 0);
    public Vector2Int PhysicalBlunt = new Vector2Int(0, 0);
    public Vector2Int PhysicalPierce = new Vector2Int(0, 0);
    public Vector2Int Thermic = new Vector2Int(0, 0);
    public Vector2Int Electric = new Vector2Int(0, 0);
    public Vector2Int Metaphysic = new Vector2Int(0, 0);

    public Vector2Int ContaminationBuildUp = new Vector2Int(0, 0);
    public Vector2Int Poise = new Vector2Int(0, 0);

    public AnimationCurve UpgradeCurve = AnimationCurve.Linear(0, 0, 1, 1);
}
