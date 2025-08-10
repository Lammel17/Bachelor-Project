using System;
using UnityEngine;
using static LookAt;

[System.Serializable]
public class LookAtData
{
    [SerializeField] public bool m_applyAddRot = true;
    [Header("Order: from Hip to Head")]
    [SerializeField] public ForwardElement[] m_forwardCorrections;


    [System.Serializable]
    public class ForwardElement
    {
        public bool Ignore = false;
        public SpineParts Element = SpineParts.hip;
        [NonSerialized] public Transform bone;
        [Tooltip("This has no effect on the first element in list")]
        public bool IgnoreInfluence = true;
        [SerializeField] public Vector3 m_newDirection = Vector3.zero;
        [Range(0, 1)] public float Weight_X = 0;
        [Range(0, 1)] public float Weight_Y = 0;
        [Range(0, 1)] public float Weight_Z = 0;
        //public IgnoreAxis IgnoreAxis = IgnoreAxis.None;
        [NonSerialized] public float LastApplyance = 0;
        [NonSerialized] public Quaternion originalRot = Quaternion.identity;

    }

}
