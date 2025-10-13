using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsSettingsData", menuName = "Scriptable Objects/CharacterStatsSettingsData")]
public class CharacterStatsSettingsData : ScriptableObject
{
    [Header("Character Stats")]
    public float EnergyRecoverySpeed = 50f;
    public float EnergyRecoveryPause = 2f;
    public float SpecialEnergyRecoverySpeed = 1f;
    [Space]
    public float BuildUpsRecoverySpeed = 1f;
    public float PoiseRecoverySpeed = 2f;
    [Space]
    public float PoiseRecoverPauseTime = 8f;

    [Space]
    [Tooltip("When energy was empty, the next action can only be done after this energy amount has been recovered")]
    public float MinRecoveredEnergyForAction = 40f;
    [Tooltip("When energy was empty,running is possible only after this energy amount has been recovered")]
    public float MinRecoveredEnergyConstantForAction = 90f;

    [Space]
    [Tooltip("When below x% poise, AND when recieving poiseDamage which is more than y% of maxPoise, then get stun")]
    public Vector2 StunThreshhold = new Vector2(0.4f, 0.05f);
    [Tooltip("When below x% energy, AND when recieving energyDamage which is more than y% of maxEnergy, then get stun")]
    public Vector2 ShieldStunThreshhold = new Vector2(0.4f, 0.1f);
}
