using System;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform m_target;
    private Vector3 m_fallbackTargetPos;

    //[SerializeField] private WeightedElement[] m_weightedElement;

    [SerializeField] private bool m_isActive = false;
    [SerializeField] [Range(-180, 180)] private float offset = 0;
    [SerializeField] private float m_applyRemoveSpeed = 1;


    [Header("Order: from Root to ends")]
    [SerializeField] private LookAtElement[] m_lookAtElement;
    [System.Serializable]
    public class LookAtElement : WeightedElement
    {
        public bool Ignore = false;
        [GD.MinMaxSlider.MinMaxSlider(-180, 180)] public Vector2 ConstrainsAngleYAxis = new Vector2(0,0);
        [NonSerialized] public float LastAngle = 0;
        [NonSerialized] public float LastApplyance = 0;
    }



    bool m_isDeactivating = false;

    public void SetTarget(Transform Target)
    {
        if (Target == null)
        {
            //m_isDeactivating = true;
            m_isActive = false;
            m_target = null;
            return;
        }

        m_isActive = true;
        m_target = Target;
        m_fallbackTargetPos = m_target.position;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_isActive)
            return;

        if (m_target == null && m_fallbackTargetPos == Vector3.zero)
            return;

        float constraintAnglesAdded = 0; //with every next bone, the current angle must be subtracted, otherwise overshoot

        foreach(LookAtElement we in m_lookAtElement)
        {
            if (we.Ignore) continue;
            if (m_target != null) m_fallbackTargetPos = m_target.position;

            Vector3 animForward = we.Element.transform.forward;
            Vector3 ToTarget = m_fallbackTargetPos - transform.position;

            float angle = Vector3.SignedAngle(new Vector3(animForward.x, 0, animForward.z), new Vector3(ToTarget.x, 0, ToTarget.z), Vector3.up);

            float applyance = 0;
            if (/*m_isDeactivating ||*/ we.ConstrainsAngleYAxis == Vector2.zero || angle - constraintAnglesAdded < we.ConstrainsAngleYAxis.x || angle - constraintAnglesAdded > we.ConstrainsAngleYAxis.y)
            {
                applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 0, m_applyRemoveSpeed * Time.deltaTime);
                we.LastApplyance = applyance;
                angle = we.LastAngle;
                //if (m_isDeactivating && applyance == 0)
                //{
                //    m_isActive = false;
                //    m_isDeactivating = false;
                //}
            }
            else
            {
                applyance = UtilityFunctions.SmartLerp(we.LastApplyance, 1, m_applyRemoveSpeed * Time.deltaTime);
                we.LastApplyance = applyance;
                constraintAnglesAdded += angle;
                we.LastAngle = angle;
            }

            angle = Mathf.Lerp(0, angle, applyance);

            angle = Mathf.Lerp(0, angle, we.Weight);

            we.Element.transform.Rotate(new Vector3(0, angle + offset, 0), Space.World);


        }
    }





}
