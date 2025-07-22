using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string Description = "";

    public Sprite ItemSprite;
    public GameObject ItemModel;
    [Space]
    public ItemAction ItemUse;
    public ItemAction ItemUseHold;


    [System.Serializable]

    public class ItemAction
    {
        public AnimationData AnimData;
        [NonSerialized] public int ActionkHash;
        public int EnergyCost;
    }
}
