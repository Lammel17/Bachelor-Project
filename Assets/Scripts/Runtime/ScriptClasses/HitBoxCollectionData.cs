using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class HitBoxCollectionData 
{
    [SerializeField] public int CollectionRefNumber;
    [SerializeField] public List<Collider> HitColliders;

}
