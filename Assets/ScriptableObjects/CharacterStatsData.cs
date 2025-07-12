using UnityEngine;


[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    public string Description = "";

    [Header("(Current, Max)")]
    public Vector2 HealthPoints = new Vector2(100, 100);
    public Vector2 EnergyPoints = new Vector2(100, 100);
    public Vector2 SpecialEnergyPoints = new Vector2(100, 100);
    public Vector2 PoisePoints = new Vector2(100, 100);
    [Space]
    public int PhysicalSliceNegation = 0;
    public int PhysicalBluntNegation = 0;
    public int PhysicalPierceNegation = 0;
    public int ThermicNegation = 0;
    public int ElectricNegation = 0;
    public int MetaphysicNegation = 0;
    [Space]
    public Vector3 ThermicBuildUp = new Vector3(100, 0, -100);
    public Vector3 ElectricBuildUp = new Vector3(100, 0, -100);
    public Vector3 MetaphysicBuildUp = new Vector3(100, 0, -100);
    [Space]
    public Vector2 ContaminationBuildUp = new Vector2(100, 100);
}
