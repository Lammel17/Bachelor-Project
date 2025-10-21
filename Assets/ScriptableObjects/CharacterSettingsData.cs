using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "Scriptable Objects/CharacterSettingsData")]
public class CharacterSettingsData : ScriptableObject
{
    public Vector3 SpeedValues = new Vector3(2, 4, 6); //slow, walk, running
    public float MoveAcceleration = 20f;
    [Space]
    public Vector3 MaxTurningSpeedBaseValue = new Vector3(12, 12, 12); //slow, walk, running
    public float TurningStrenghtBaseValues = 15f;
    [Space]
    public float Gravity = -15f;

}
