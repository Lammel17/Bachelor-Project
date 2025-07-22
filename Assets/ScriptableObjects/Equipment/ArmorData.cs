using UnityEngine;

[CreateAssetMenu(fileName = "ArmorData", menuName = "Scriptable Objects/ArmorData")]
public class ArmorData : ScriptableObject
{
    public string Description = "";

    public Sprite ArmorSprite;
    public GameObject ArmorModel;
    [Space]
    public ArmorPart armorPart;



    public enum ArmorPart
    {
        Head = 1,
        Chest,
        Arms,
        Legs
    }
}
