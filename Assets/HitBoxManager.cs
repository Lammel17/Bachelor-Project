using UnityEngine;
using System.Collections.Generic;
using System;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody))]
public class HitBoxManager : MonoBehaviour
{
    [SerializeField] private List<HitBoxCollectionData> m_weaponHitboxes = new List<HitBoxCollectionData>();
    private List<int> m_activeCollections = new List<int>();
    [SerializeField][ReadOnly] private DamageData m_damageData;
    private HurtBoxManager m_ownHurtBoxManager;
    private List<HurtBoxManager> m_hurtboxesHitted = new List<HurtBoxManager>();
    private bool m_canHitOnlyOnce = false;


    public HitBoxManager ReadyWeapon(HurtBoxManager ownHurtBoxManager)
    {
        m_ownHurtBoxManager = ownHurtBoxManager;

        if (m_weaponHitboxes.Count != 0)
        {
            foreach (HitBoxCollectionData hbcd in m_weaponHitboxes)
            {
                foreach (Collider coll in hbcd.HitColliders)
                {
                    coll.enabled = false;
                }
            }
        }

        return this;
    }

    public bool CheckIfCanBeHit(HurtBoxManager hurtBoxManager)
    {
        if (m_ownHurtBoxManager == hurtBoxManager || m_hurtboxesHitted.Contains(hurtBoxManager))
            return false;
        return true;
    }

    public DamageData HurtBoxWasHit(HurtBoxManager hurtBoxManager)
    {
        if (!CheckIfCanBeHit(hurtBoxManager))
            return null;

        m_hurtboxesHitted.Add(hurtBoxManager);

        if (m_canHitOnlyOnce)
            DeactivateAllHitboxCollections();

        return m_damageData;
    }


    public void ActivateHitboxCollection(int hitboxCollectionRef, DamageData damageData)
    {
        if (m_weaponHitboxes.Count == 0) return;
        m_damageData = damageData;

        foreach (HitBoxCollectionData hbcd in m_weaponHitboxes)
        {
            //Debug.Log(hitboxCollectionRef);

            if (hitboxCollectionRef != hbcd.CollectionRefNumber) 
                continue;
            if (m_activeCollections.Contains(hitboxCollectionRef))
            {
                m_activeCollections.Add(hitboxCollectionRef);
                continue;
            }
            else
                m_activeCollections.Add(hitboxCollectionRef);


            foreach (Collider coll in hbcd.HitColliders)
            {
                if (coll != null)
                    coll.enabled = true;
            }
        }
    }

    public void DeactivateHitboxCollection(int hitboxCollectionRef)
    {
        if (!m_activeCollections.Contains(hitboxCollectionRef)) 
            return;
        m_activeCollections.Remove(hitboxCollectionRef);
        if (m_activeCollections.Contains(hitboxCollectionRef)) 
            return;

        foreach (HitBoxCollectionData hbcd in m_weaponHitboxes)
        {
            if (hitboxCollectionRef != hbcd.CollectionRefNumber)
                continue;

            foreach (Collider coll in hbcd.HitColliders)
            {
                if (coll != null)
                    coll.enabled = false;
            }
        }
    }


    public void DeactivateAllHitboxCollections()
    {
        m_damageData = null;
        if (m_activeCollections.Count == 0) return;

        foreach (HitBoxCollectionData hbcd in m_weaponHitboxes)
        {
            if (!m_activeCollections.Contains(hbcd.CollectionRefNumber))
                continue;

            foreach (Collider coll in hbcd.HitColliders)
            {
                if (coll != null)
                    coll.enabled = false;
            }
        }
        m_activeCollections.Clear();
        m_hurtboxesHitted.Clear();

    }



}
