using UnityEngine;


[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    public string Description = "";

    [Header("(Current, Max)")]
    public Vector2Int HealthPoints = new Vector2Int(500, 500);
    public Vector2Int EnergyPoints = new Vector2Int(500, 500);
    public Vector2Int SpecialEnergyPoints = new Vector2Int(500, 500);
    public Vector2 PoisePoints = new Vector2(100, 100);
    [Space]
    public Vector3 ThermicBuildUp = new Vector3(0, 100, -100);
    public Vector3 ElectricBuildUp = new Vector3(0, 100, -100);
    public Vector3 MetaphysicBuildUp = new Vector3(0, 100, -100);
    [Space]
    public Vector2 ContaminationBuildUp = new Vector2(0, 100);
    [Space]
    [Header("From 0 to 100")]
    public int PhysicalSliceNegation = 0;
    public int PhysicalBluntNegation = 0;
    public int PhysicalPierceNegation = 0;
    public int ThermicNegation = 0;
    public int ElectricNegation = 0;
    public int MetaphysicNegation = 0;
}
