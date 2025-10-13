using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettingsData", menuName = "Scriptable Objects/CharacterSettingsData")]
public class CharacterSettingsData : ScriptableObject
{
    public Vector3 SpeedValues = new Vector3(2, 4, 6); //slow, walk, running
    public float MoveAcceleration = 20f;
    [Space]
    public Vector3 TurningStrenghtBaseValues = new Vector3(15, 15, 10); //slow, walk, running
    public float MaxTurningSpeedBaseValue = 10f;
    [Space]
    public float Gravity = -15f;

}
