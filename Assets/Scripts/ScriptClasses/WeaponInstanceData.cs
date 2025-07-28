using EditorAttributes;
using System;
using UnityEngine;

[System.Serializable]
public class WeaponInstanceData
{
    public WeaponData WeaponData;
    public Vector2Int WeaponLevelCurrentMax = new Vector2Int(1, 10);
    [ReadOnly] public DamageData DamageData;

}
