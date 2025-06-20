using System;
using UnityEngine;
using static LookAt;

[System.Serializable]
public class LookAtForwardData
{
    [SerializeField] public bool m_applyAddRot = true;
    [SerializeField] public Vector3 m_addRotEuler = Vector3.zero;
    [Header("Order: from Hip to Head")]
    [SerializeField] public ForwardElement[] m_forwardCorrections;


    [System.Serializable]
    public class ForwardElement
    {
        public SpineParts Element = SpineParts.hip;
        [NonSerialized] public Transform bone;
        [Tooltip("This has no effect on the first element in list")]
        public bool IsUsingOrigRot = true;
        [Range(0, 1)] public float Weight = 0;
        public IgnoreAxis IgnoreAxis = IgnoreAxis.None;
        public bool Ignore = false;
        [NonSerialized] public float LastApplyance = 0;
        [NonSerialized] public Quaternion originalRot = Quaternion.identity;

    }

    public enum SpineParts
    {
        hip = 0,
        lowerCore,
        upperCore,
        chest,
        lowerNeck,
        upperNeck,
        head
    }
}
