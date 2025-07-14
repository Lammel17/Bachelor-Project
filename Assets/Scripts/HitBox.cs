using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [SerializeField] private Collider m_hitBoxCollider;
    private DamageData m_damageData;
    private HurtBoxManager m_ownHurtBoxManager;
    private List<HurtBoxManager> m_hurtboxesHitted = new List<HurtBoxManager>();

    private bool m_canHitOnlyOnce = false;
    

    public bool CheckIfCanBeHit(HurtBoxManager hurtBoxManager)
    {
        if (m_ownHurtBoxManager == hurtBoxManager || m_hurtboxesHitted.Contains(hurtBoxManager))
            return false;
        return true;
    }

    public DamageData HurtBoxWasHit(HurtBoxManager hurtBoxManager)
    {
        m_hurtboxesHitted.Add(hurtBoxManager);

        if (m_canHitOnlyOnce)
            DeactivateHitBox();

        return m_damageData;
    }

    public void ActivateHitBox(DamageData damageData)
    {
        if (m_hitBoxCollider == null)
            m_hitBoxCollider.GetComponent<Collider>();

        m_damageData = damageData;
        m_hitBoxCollider.enabled = true;
    }

    public void DeactivateHitBox()
    {
        if (m_hitBoxCollider == null)
            m_hitBoxCollider.GetComponent<Collider>();

        m_hitBoxCollider.enabled = false;
        m_hurtboxesHitted.Clear();
        m_damageData = null;
    }



}
