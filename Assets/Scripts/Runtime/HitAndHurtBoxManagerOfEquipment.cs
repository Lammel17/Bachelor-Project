using UnityEngine;
using System.Collections.Generic;
using System;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody))]
public class HitAndHurtBoxManagerOfEquipment : MonoBehaviour
{
    [SerializeField] private List<HitBoxCollectionData> m_hitBoxCollectionList = new List<HitBoxCollectionData>();
    private List<int> m_activeCollections = new List<int>();
    [SerializeField][ReadOnly] private DamageData m_damageData;
    private HurtBoxManager m_ownHurtBoxManager;
    private List<HurtBoxManager> m_hurtboxesHitted = new List<HurtBoxManager>();
    private bool m_canHitOnlyOnce = false;

    public DamageData DamageData { get => m_damageData; }

    [Header("HurtBox For defensive Blocking")]
    [SerializeField] private List<HurtBox> m_defensiveBox;

    //gets set when weapon/shield is put out and set active
    public HitAndHurtBoxManagerOfEquipment ReadyHitBoxManager(HurtBoxManager ownHurtBoxManager)
    {
        m_ownHurtBoxManager = ownHurtBoxManager;

        if (m_hitBoxCollectionList.Count != 0)
        {
            foreach (HitBoxCollectionData hbcd in m_hitBoxCollectionList)
            {
                foreach (Collider coll in hbcd.HitColliders)
                {
                    coll.enabled = false;
                }
            }
        }


        //if the equipment has a hurtbox as a defensive collider, like a shield
        if (m_defensiveBox.Count != 0)
        {
            foreach(HurtBox hrtb in m_defensiveBox)
            {
                hrtb.SetValues(m_ownHurtBoxManager, new DamageMultiplikatorData(1,1,1,1,1,1,1), true);
                hrtb.EnableHurtBox(false);
            }
        }

        return this;
    }


    public DamageData CheckIfCanHitAndGetDamageData(HurtBoxManager hurtBoxManager)
    {
        if (m_ownHurtBoxManager == hurtBoxManager || m_hurtboxesHitted.Contains(hurtBoxManager))
            return null;

        m_hurtboxesHitted.Add(hurtBoxManager);

        if (m_canHitOnlyOnce)
            DeactivateAllHitboxCollections();

        return m_damageData;
    }


    public void ActivateHitboxCollection(int hitboxCollectionRef, DamageData damageData)
    {
        if (m_hitBoxCollectionList.Count == 0) return;
        m_damageData = damageData;

        foreach (HitBoxCollectionData hbcd in m_hitBoxCollectionList)
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
                {
                    coll.enabled = true;
                    //Debug.Log(coll.gameObject.name);
                }
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

        foreach (HitBoxCollectionData hbcd in m_hitBoxCollectionList)
        {
            if (hitboxCollectionRef != hbcd.CollectionRefNumber)
                continue;

            foreach (Collider coll in hbcd.HitColliders)
            {
                if (coll != null)
                    coll.enabled = false;
            }
        }

        if (m_activeCollections.Count == 0)
            m_hurtboxesHitted.Clear();
    }


    public void DeactivateAllHitboxCollections()
    {
        m_damageData = null;
        if (m_activeCollections.Count == 0) return;

        foreach (HitBoxCollectionData hbcd in m_hitBoxCollectionList)
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


    //if the equipment has a hurtbox as a defensive collider, like a shield
    public void ActivateBlockBox()
    {
        foreach (HurtBox hrtb in m_defensiveBox)
        {
            hrtb.EnableHurtBox(true);
        }
    }

    public void DeactivateBlockBox()
    {
        foreach (HurtBox hrtb in m_defensiveBox)
        {
            hrtb.EnableHurtBox(false);
        }
    }


}
