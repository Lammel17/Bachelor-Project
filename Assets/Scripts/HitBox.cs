using UnityEngine;
using System;
using System.Collections.Generic;
using EditorAttributes;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [SerializeField] [Required] private HitBoxManager m_hitBoxManager;


    private void OnTriggerEnter(Collider other)
    {
        if (m_hitBoxManager == null)
            return;

        if (other.transform.gameObject.layer == (int)Layers.Damageable && other.TryGetComponent<HurtBox>(out HurtBox hurt))
        {
            DamageData damageData = m_hitBoxManager.CheckIfCanHitAndGetDamageData(hurt.HurtBoxManager);
            if (damageData != null)
                hurt.HurtBoxWasHit(damageData);
        }

    }


}
