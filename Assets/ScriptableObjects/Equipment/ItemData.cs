using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{


    public ItemAction ItemUse;
    public ItemAction ItemUseHold;


    [System.Serializable]

    public class ItemAction
    {
        public AnimationData AnimData;
        [NonSerialized] public int ActionkHash;
    }
}
