using System;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform m_target;
    private Vector3 m_fallbackTargetPos;

    //[SerializeField] private WeightedElement[] m_weightedElement;

    [SerializeField] private bool m_isActive = true;
    [Space]
    [SerializeField] [EditorAttributes.ReadOnly] private bool m_isActiveTarget = false;
    [SerializeField] [EditorAttributes.ReadOnly] private bool m_isActiveForwardCorrection = false;
    [Space]
    [SerializeField] private float m_applyRemoveSpeed = 1;

    [Header("/////////////////////////////////////////////////////////////////")]

    [Header("Order: from Root to ends")]
    [SerializeField] [Range(-180, 180)] private float offset = 0;
    [SerializeField] private LookAtElement[] m_targetLookAt;
    [Space]
    [Header("/////////////////////////////////////////////////////////////////")]
    [Header("Order: from Root to ends")]
    [SerializeField] private Bones[] m_bones;
    private LookAtForwardData m_forwardData;
    //private bool m_applyAddRot = true;
    //private Vector3 m_addRotEuler = Vector3.zero; //////////////////// later add this to the shield and take it from there
    //private ForwardElement[] m_forwardCorrection;



    [System.Serializable]
    public class Bones
    {
        public LookAtForwardData.SpineParts Bone;
        public Transform BoneRef;
    }

    [System.Serializable]
    public class LookAtElement 
    {
        public Transform Element;
        [Range(-1, 1)] public float Weight = 0;
        [GD.MinMaxSlider.MinMaxSlider(-180, 180)] public Vector2 ConstrainsAngleYAxis = new Vector2(0,0); // if 0,0, them it will be used always
        public bool Ignore = false;
        [NonSerialized] public float LastAngle = 0;
        [NonSerialized] public float LastApplyance = 0;

        [NonSerialized] public Quaternion LastRot = Quaternion.identity;
    }

    //[System.Serializable]
    //public class ForwardElement
    //{
    //    public Transform Element;
    //    [Tooltip("This has no effect on the first element in list")]
    //    public bool IsUsingOrigRot = true;
    //    [Range(0, 1)] public float Weight = 0;
    //    public IgnoreAxis IgnoreAxis = IgnoreAxis.None;
    //    public bool Ignore = false;
    //    [NonSerialized] public float LastApplyance = 0;
    //    [NonSerialized] public Quaternion originalRot = Quaternion.identity;

    //}

    public enum IgnoreAxis
    {
        None = 0,
        IgnoreX,
        IgnoreY,
        IgnoreZ,
        IgnoreXY,
        IgnoreXZ,
        IgnoreYZ,
    }



    bool m_isDeactivatingLookAt = false;
    bool m_isDeactivatingForward = false;

    public void SetTarget(Transform Target)
    {
        if (Target == null)
        {
            m_target = null;
            if (!m_isActiveTarget)
                return;
            m_isDeactivatingLookAt = true;
            return;
        }

        m_isDeactivatingLookAt = false;
        m_isActiveTarget = true;
        m_target = Target;
        m_fallbackTargetPos = m_target.position;
    }


    public void SetForwardActive(LookAtForwardData data)
    {
        if (data == null)
            return;

        foreach (LookAtForwardData.ForwardElement we in data.m_forwardCorrections)
        {
            if (we.bone == null)
                we.bone = m_bones[(int)we.Element].BoneRef;
        }

        m_forwardData = data;

        m_isDeactivatingForward = false;
        m_isActiveForwardCorrection = true;
    }
    public void SetForwardDeactive()
    {
        if (!m_isActiveForwardCorrection)
            return;
        m_isDeactivatingForward = true;
        return;
    }






    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_isActive)
            return;

        if (m_isActiveTarget)
        {
            if (m_target == null && m_fallbackTargetPos == Vector3.zero)
                return;

            float constraintAnglesAdded = 0; //with every next bone, the current angle must be subtracted, otherwise overshoot

            foreach (LookAtElement we in m_targetLookAt)
            {
                if (we.Ignore) continue;
                if (m_target != null) m_fallbackTargetPos = m_target.position;

                Vector3 boneForward = we.Element.forward;
                Vector3 ToTarget = m_fallbackTargetPos - transform.position;

                float angleToTarget = Vector3.SignedAngle(new Vector3(boneForward.x, 0, boneForward.z), new Vector3(ToTarget.x, 0, ToTarget.z), Vector3.up);
                float applyance = 0;
                float usedAngle = 0;

                if (we.ConstrainsAngleYAxis != Vector2.zero && (m_isDeactivatingLookAt || angleToTarget - constraintAnglesAdded < we.ConstrainsAngleYAxis.x || angleToTarget - constraintAnglesAdded > we.ConstrainsAngleYAxis.y))
                {
                    applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                    we.LastApplyance = applyance;
                    usedAngle = we.LastAngle;
                    if (m_isDeactivatingLookAt && applyance == 0)
                    {
                        m_isActiveTarget = false;
                        m_isDeactivatingLookAt = false;
                    }
                }
                else //when activating or is active
                {
                    applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                    we.LastApplyance = applyance;
                    usedAngle = angleToTarget;
                    we.LastAngle = usedAngle;
                }

                if (we.Weight < 0) // -1 will turn it to the ausgangsrotation
                    usedAngle = -constraintAnglesAdded;

                float weightedAngle = Mathf.Lerp(0, usedAngle, Mathf.Abs(we.Weight)); //weight is not changed in runtime
                float angle = Mathf.Lerp(0, weightedAngle, applyance); //applyance is if targeting switches on or off

                constraintAnglesAdded += angle; 

                we.Element.Rotate(new Vector3(0, angle + offset, 0), Space.World);

            }
        }




        if (m_isActiveForwardCorrection)
        {
            foreach (LookAtForwardData.ForwardElement we in m_forwardData.m_forwardCorrections)
            {
                if (we.Ignore) { continue; }
                we.originalRot = Quaternion.Inverse(transform.rotation) * we.bone.rotation;
            }

            foreach (LookAtForwardData.ForwardElement we in m_forwardData.m_forwardCorrections)
            {
                if (we.Ignore) { continue; }

                float applyance = 0;
                Quaternion usedRot = m_forwardData.m_applyAddRot ? Quaternion.Euler(m_forwardData.m_addRotEuler.x, m_forwardData.m_addRotEuler.y, m_forwardData.m_addRotEuler.z) : Quaternion.identity;

                if (m_isDeactivatingForward) // when deactivating
                {
                    applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                    we.LastApplyance = applyance;

                    if (m_isDeactivatingForward && applyance == 0)
                    {
                        m_isActiveForwardCorrection = false;
                        m_isDeactivatingForward = false;
                    }
                }
                else //when activating or is active
                {
                    applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                    we.LastApplyance = applyance;
                }

                // never try local space stuff here, better just change the word space to the inital bone rot as new worldspace and work from there
                switch (we.IgnoreAxis)
                {
                    case IgnoreAxis.None: { break; }
                    case IgnoreAxis.IgnoreX: { usedRot = Quaternion.Euler(we.originalRot.eulerAngles.x, usedRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreY: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, we.originalRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreZ: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, usedRot.eulerAngles.y, we.originalRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreXY: { usedRot = Quaternion.Euler(we.originalRot.eulerAngles.x, we.originalRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreXZ: { usedRot = Quaternion.Euler(we.originalRot.eulerAngles.x, usedRot.eulerAngles.y, we.originalRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreYZ: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, we.originalRot.eulerAngles.y, we.originalRot.eulerAngles.z); break; }
                }

                if (m_isActiveTarget && !m_isDeactivatingLookAt) usedRot = Quaternion.Euler(usedRot.eulerAngles.x, we.originalRot.eulerAngles.y, usedRot.eulerAngles.z);

                Quaternion weightedRot = Quaternion.Slerp(we.IsUsingOrigRot ? we.originalRot : we.bone.rotation, usedRot, we.Weight); //weight is not changed in runtime
                Quaternion rot = Quaternion.Slerp(we.bone.rotation, transform.rotation * weightedRot, applyance); //applyance is if targeting switches on or off
                we.bone.rotation = rot;


            }
        }

    }





}
