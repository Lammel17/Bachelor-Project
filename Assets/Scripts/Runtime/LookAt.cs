using System;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private bool m_isActive = true;
    [SerializeField][EditorAttributes.ReadOnly] LookAtState m_lookAtTargetState = LookAtState.Deactive;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_lastLookTargetApplyance = 0;
    [SerializeField][EditorAttributes.ReadOnly] LookAtState m_lookAtForwardState = LookAtState.Deactive;
    [SerializeField][EditorAttributes.ReadOnly][Range(0, 1)] private float m_lastLookForwardApplyance = 0;
    [SerializeField] private float m_applyRemoveSpeed = 1;
    [Space]

    [Header("Spinal Bone Collection")]
    [Tooltip("Order: from Root to ends")]
    [SerializeField] private Bones[] m_bones;
    private LookAtData m_forwardData;

    [Header("LookAt Target")]
    [SerializeField] private Transform m_target;
    private Vector3 m_fallbackTargetPos;
    [SerializeField][Range(-180, 180)] private float offset = 0;
    [Header("LookAtData for LockOnTarget")]
    [GD.MinMaxSlider.MinMaxSlider(-180, 180)] public Vector2 m_constrainsAngleYAxis = new Vector2(-90, 90);
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
        public bool Ignore = false;
        [NonSerialized] public float LastAngle = 0;
        //[NonSerialized] public float LastApplyance = 0;

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
    private enum LookAtState { Deactive, Active, Deactivating, ActiveButOutOfArea}



    public void SetTarget(Transform Target)
    {
        if (Target == null)
        {
            m_target = null;
            if (m_lookAtTargetState == LookAtState.Deactive )
                return;
            m_lookAtTargetState = LookAtState.Deactivating;
            return;
        }
        else
        {
            m_lookAtTargetState = LookAtState.Active;
            m_target = Target;
            m_fallbackTargetPos = m_target.position;
        }

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

        m_lookAtForwardState = LookAtState.Active;
    }
    public void SetForwardDeactive()
    {
        if (m_lookAtForwardState != LookAtState.Active)
            return;
        m_lookAtForwardState = LookAtState.Deactivating;
        return;
    }







    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_isActive)
            return;


        //LOOK AT TARGET
        if (m_lookAtTargetState != LookAtState.Deactive)
        {

            float constraintAnglesAdded = 0; //with every next bone, the current angle must be subtracted, otherwise overshoot
            float applyance = m_lastLookTargetApplyance;
            if (m_target != null) m_fallbackTargetPos = m_target.position;
            Vector3 DirToTarget = m_fallbackTargetPos - transform.position;
            float angleToTarget = Vector3.SignedAngle(new Vector3(transform.forward.x, 0, transform.forward.z), new Vector3(DirToTarget.x, 0, DirToTarget.z), Vector3.up) - constraintAnglesAdded;

            //the following is only for the smooth weighting when activating or deactivating (applyance means weighting)
            //when lookAtTarget is deactivating and looses applyance until 0
            if (m_lookAtTargetState != LookAtState.Deactivating  && (angleToTarget - constraintAnglesAdded < m_constrainsAngleYAxis.x || angleToTarget - constraintAnglesAdded > m_constrainsAngleYAxis.y))
                m_lookAtTargetState = LookAtState.ActiveButOutOfArea;
            else if (m_lookAtTargetState == LookAtState.ActiveButOutOfArea)
                m_lookAtTargetState = LookAtState.Active;

            if ((m_lookAtTargetState == LookAtState.Deactivating || m_lookAtTargetState == LookAtState.ActiveButOutOfArea))
            {
                if (m_lookAtTargetState == LookAtState.Deactivating && applyance == 0)
                    m_lookAtTargetState = LookAtState.Deactive;

                applyance = UtilityFunctions.SmartLerp(m_lastLookTargetApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                m_lastLookTargetApplyance = applyance;
            }
            else //when lookAtTarget is activating and increases applyance or is fully active
            {
                m_lookAtTargetState = LookAtState.Active;
                applyance = m_lastLookTargetApplyance == 1 ? 1 : UtilityFunctions.SmartLerp(m_lastLookTargetApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                m_lastLookTargetApplyance = applyance;
            }

            foreach (LookAtElement boneElement in m_targetLookAt)
            {
                if (boneElement.Ignore) continue;

                float usedAngle = 0;
                Transform bone = m_bones[(int)boneElement.Element].BoneRef;
                Vector3 boneForward = transform.forward;
                float boneAngleToTarget = Vector3.SignedAngle(new Vector3(boneForward.x, 0, boneForward.z), new Vector3(DirToTarget.x, 0, DirToTarget.z), Vector3.up) - constraintAnglesAdded;



                //the following is only for the smooth weighting when activating or deactivating (applyance means weighting)
                if (m_lookAtTargetState == LookAtState.Deactivating)
                    usedAngle = boneElement.LastAngle;
                else if (m_lookAtTargetState == LookAtState.Active)
                {
                    boneElement.LastAngle = usedAngle;
                    usedAngle = boneAngleToTarget;
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
        if (m_lookAtForwardState != LookAtState.Deactive)
        {
            float applyance = m_lastLookForwardApplyance;
            //the following is only for the smooth weighting when activating or deactivating (applyance means weighting)
            if (m_lookAtForwardState == LookAtState.Deactivating) // when deactivating
            {
                if (applyance == 0)
                    m_lookAtForwardState = LookAtState.Deactive;

                applyance = UtilityFunctions.SmartLerp(m_lastLookForwardApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                m_lastLookForwardApplyance = applyance;
            }
            else //when activating or is active
            {
                m_lookAtForwardState = LookAtState.Active;
                applyance = m_lastLookForwardApplyance == 1 ? 1 : UtilityFunctions.SmartLerp(m_lastLookForwardApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                m_lastLookForwardApplyance = applyance;
            }

            //this is when booth the LookAtTarget and LookAtForward is active, needs to be calculated
            Quaternion DirToTargetRot = transform.rotation;
            if (m_lookAtTargetState != LookAtState.Deactive)
            {
                Vector3 DirToTarget = new Vector3((m_fallbackTargetPos - transform.position).x, 0, (m_fallbackTargetPos - transform.position).z);
                DirToTargetRot = Quaternion.LookRotation(DirToTarget);
            }
            Quaternion characterForwardRot = Quaternion.Slerp(transform.rotation, DirToTargetRot, m_lastLookTargetApplyance);

            //this saves the rotation before its changed in the frame
            foreach (LookAtData.ForwardElement boneElement in m_forwardData.m_forwardCorrections)
            {
                if (boneElement.Ignore) { continue; }
                boneElement.originalRot = Quaternion.Inverse(characterForwardRot) * boneElement.bone.rotation;
            }

            foreach (LookAtData.ForwardElement boneElement in m_forwardData.m_forwardCorrections)
            {
                if (boneElement.Ignore) { continue; }

                Quaternion newForward = m_forwardData.m_applyAddRot ? Quaternion.Euler(boneElement.m_newDirection.x, boneElement.m_newDirection.y, boneElement.m_newDirection.z) : Quaternion.identity;
                Quaternion zeroWeightRot = boneElement.IgnoreInfluence ? boneElement.originalRot : Quaternion.Inverse(characterForwardRot) * boneElement.bone.rotation;
                //Quaternion weightedRot = UtilityFunctions.WeightIndividualAxesOfQuaternionNoGIMBAL(zeroWeightRot, newForward, boneElement.Weight_X, boneElement.Weight_Y, boneElement.Weight_Z, true);
                Quaternion weightedRot = UtilityFunctions.WeightIndividualAxesOfQuaternion(zeroWeightRot, newForward, boneElement.Weight_X, boneElement.Weight_Y, boneElement.Weight_Z);

                Quaternion rot = Quaternion.Slerp(boneElement.bone.rotation, characterForwardRot * weightedRot, applyance); //applyance is if targeting switches on or off
                boneElement.bone.rotation = rot;


            }
        }

    }





}
