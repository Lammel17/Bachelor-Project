using System;
using UnityEngine;

[System.Serializable]
public abstract class WeightedElement 
{
    public GameObject Element;
    [Range(0,1)] public float Weight = 0;
}
