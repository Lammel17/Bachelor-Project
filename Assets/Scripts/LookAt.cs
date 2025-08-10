using System;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private bool m_isActive = true;
    [SerializeField] [EditorAttributes.ReadOnly] private bool m_isActiveForwardCorrection = false;
    [SerializeField] [EditorAttributes.ReadOnly] private bool m_isActiveTarget = false;
    [SerializeField] private float m_applyRemoveSpeed = 1;
    [Space]

    [Header("Spinal Bone Collection")]
    [Tooltip("Order: from Root to ends")]
    [SerializeField] private Bones[] m_bones;
    private LookAtData m_forwardData;

    [Header("LookAt Target")]
    [SerializeField] private Transform m_target;
    private Vector3 m_fallbackTargetPos;
    [SerializeField] [Range(-180, 180)] private float offset = 0;
    [Header("LookAtData for LockOn")]
    [Tooltip("Order: from Root to ends")]
    [SerializeField] private LookAtElement[] m_targetLookAt;



    [System.Serializable]
    public class Bones
    {
        public SpineParts Bone;
        public Transform BoneRef;
    }

    [System.Serializable]
    public class LookAtElement 
    {
        public SpineParts Element;
        [Range(-1, 1)] public float Weight = 0;
        [GD.MinMaxSlider.MinMaxSlider(-180, 180)] public Vector2 ConstrainsAngleYAxis = new Vector2(0,0); // if 0,0, them it will be used always, otherwise only if the desiredLookDir is inside that angle range
        public bool Ignore = false;
        [NonSerialized] public float LastAngle = 0;
        [NonSerialized] public float LastApplyance = 0;

        [NonSerialized] public Quaternion LastRot = Quaternion.identity;
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


    public void SetForwardActive(LookAtData data)
    {
        if (data == null)
            return;

        foreach (LookAtData.ForwardElement we in data.m_forwardCorrections)
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


        //LOOK AT TARGET
        if (m_isActiveTarget)
        {
            if (m_target == null && m_fallbackTargetPos == Vector3.zero)
                return;

            float constraintAnglesAdded = 0; //with every next bone, the current angle must be subtracted, otherwise overshoot

            foreach (LookAtElement boneElement in m_targetLookAt)
            {
                if (boneElement.Ignore) continue;
                if (m_target != null) m_fallbackTargetPos = m_target.position;

                Transform bone = m_bones[(int)boneElement.Element].BoneRef;
                Vector3 boneForward = bone.forward;
                Vector3 DirToTarget = m_fallbackTargetPos - transform.position;
                float angleToTarget = Vector3.SignedAngle(new Vector3(boneForward.x, 0, boneForward.z), new Vector3(DirToTarget.x, 0, DirToTarget.z), Vector3.up);
                float applyance = 0;
                float usedAngle = 0;



                //the following is only for the smooth weighting when activating or deactivating (applyance means weighting)
                //when lookAtTarget is deactivating and looses applyance until 0
                if (boneElement.ConstrainsAngleYAxis != Vector2.zero && (m_isDeactivatingLookAt || angleToTarget - constraintAnglesAdded < boneElement.ConstrainsAngleYAxis.x || angleToTarget - constraintAnglesAdded > boneElement.ConstrainsAngleYAxis.y))
                {
                    applyance = UtilityFunctions.SmartLerp(boneElement.LastApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                    boneElement.LastApplyance = applyance;
                    usedAngle = boneElement.LastAngle;
                    if (m_isDeactivatingLookAt && applyance == 0)
                    {
                        m_isActiveTarget = false;
                        m_isDeactivatingLookAt = false;
                    }
                }
                else //when lookAtTarget is activating and increases applyance or is fully active
                {
                    applyance = UtilityFunctions.SmartLerp(boneElement.LastApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                    boneElement.LastApplyance = applyance;
                    usedAngle = angleToTarget;
                    boneElement.LastAngle = usedAngle;
                }



                if (boneElement.Weight < 0) // -1 will turn it to the ausgangsrotation
                    usedAngle = -constraintAnglesAdded;

                float weightedAngle = Mathf.Lerp(0, usedAngle, Mathf.Abs(boneElement.Weight)); //weight is not changed in runtime
                float angle = Mathf.Lerp(0, weightedAngle, applyance); //applyance is if targeting switches on or off

                constraintAnglesAdded += angle;

                bone.Rotate(new Vector3(0, angle + offset, 0), Space.World);

            }
        }



        //LOOK AT Forward
        if (m_isActiveForwardCorrection)
        {
            //this saves the rotation before its changed in the frame
            foreach (LookAtData.ForwardElement boneElement in m_forwardData.m_forwardCorrections)
            {
                if (boneElement.Ignore) { continue; }
                boneElement.originalRot = Quaternion.Inverse(transform.rotation) * boneElement.bone.rotation;
            }

            foreach (LookAtData.ForwardElement boneElement in m_forwardData.m_forwardCorrections)
            {
                if (boneElement.Ignore) { continue; }

                float applyance = 0;
                Quaternion usedRot = m_forwardData.m_applyAddRot ? Quaternion.Euler(m_forwardData.m_addRotEuler.x, m_forwardData.m_addRotEuler.y, m_forwardData.m_addRotEuler.z) : Quaternion.identity;



                if (m_isDeactivatingForward) // when deactivating
                {
                    applyance = UtilityFunctions.SmartLerp(boneElement.LastApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                    boneElement.LastApplyance = applyance;

                    if (m_isDeactivatingForward && applyance == 0)
                    {
                        m_isActiveForwardCorrection = false;
                        m_isDeactivatingForward = false;
                    }
                }
                else //when activating or is active
                {
                    applyance = UtilityFunctions.SmartLerp(boneElement.LastApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                    boneElement.LastApplyance = applyance;
                }



                // never try local space stuff here, better just change the word space to the inital bone rot as new worldspace and work from there
                switch (boneElement.IgnoreAxis)
                {
                    case IgnoreAxis.None: { break; }
                    case IgnoreAxis.IgnoreX: { usedRot = Quaternion.Euler(boneElement.originalRot.eulerAngles.x, usedRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreY: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, boneElement.originalRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreZ: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, usedRot.eulerAngles.y, boneElement.originalRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreXY: { usedRot = Quaternion.Euler(boneElement.originalRot.eulerAngles.x, boneElement.originalRot.eulerAngles.y, usedRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreXZ: { usedRot = Quaternion.Euler(boneElement.originalRot.eulerAngles.x, usedRot.eulerAngles.y, boneElement.originalRot.eulerAngles.z); break; }
                    case IgnoreAxis.IgnoreYZ: { usedRot = Quaternion.Euler(usedRot.eulerAngles.x, boneElement.originalRot.eulerAngles.y, boneElement.originalRot.eulerAngles.z); break; }
                }

                if (m_isActiveTarget && !m_isDeactivatingLookAt) usedRot = Quaternion.Euler(usedRot.eulerAngles.x, boneElement.originalRot.eulerAngles.y + m_forwardData.m_addRotEuler.y, usedRot.eulerAngles.z);

                Quaternion weightedRot = Quaternion.Slerp(boneElement.IsUsingOrigRot ? boneElement.originalRot : boneElement.bone.rotation, usedRot, boneElement.Weight); //weight is not changed in runtime
                Quaternion rot = Quaternion.Slerp(boneElement.bone.rotation, transform.rotation * weightedRot, applyance); //applyance is if targeting switches on or off
                boneElement.bone.rotation = rot;


            }
        }

    }





}
