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
    [Header("LookAtData for LockOnTarget")]
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
        [Tooltip("-1: ignoring all influence; 0: with influence of the other bones; 1: own influence to lookAtTarget")]
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
                Vector3 DirToTarget = m_fallbackTargetPos - transform.position;

                Transform bone = m_bones[(int)boneElement.Element].BoneRef;
                Vector3 boneForward = transform.forward;
                float angleToTarget = Vector3.SignedAngle(new Vector3(boneForward.x, 0, boneForward.z), new Vector3(DirToTarget.x, 0, DirToTarget.z), Vector3.up) - constraintAnglesAdded;
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
                Quaternion newForward = m_forwardData.m_applyAddRot ? Quaternion.Euler(boneElement.m_newDirection.x, boneElement.m_newDirection.y, boneElement.m_newDirection.z) : Quaternion.identity;



                //the following is only for the smooth weighting when activating or deactivating (applyance means weighting)
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


                Quaternion zeroWeightRot = boneElement.IgnoreInfluence ? boneElement.originalRot : Quaternion.Inverse(transform.rotation) * boneElement.bone.rotation;

                Quaternion weightedRot = UtilityFunctions.WeightIndividualAxesOfQuaternion(zeroWeightRot, newForward, boneElement.Weight_X, boneElement.Weight_Y, boneElement.Weight_Z);


                Quaternion rot = Quaternion.Slerp(boneElement.bone.rotation, transform.rotation * weightedRot, applyance); //applyance is if targeting switches on or off
                boneElement.bone.rotation = rot;



                //Quaternion DirToTargetRot = Quaternion.identity;
                //if (m_isActiveTarget && !m_isDeactivatingLookAt)
                //{
                //    Vector3 DirToTarget = new Vector3((m_fallbackTargetPos - transform.position).x, 0, (m_fallbackTargetPos - transform.position).z);
                //    DirToTargetRot = Quaternion.FromToRotation(transform.forward, DirToTarget);
                //}


            }
        }

    }





}
