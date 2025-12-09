using UnityEngine;
using System;
using System.Collections.Generic;
using EditorAttributes;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{
    [SerializeField] [Required] private HitAndHurtBoxManagerOfEquipment m_hitBoxManager;


    private void OnTriggerEnter(Collider other)
    {
        if (m_hitBoxManager == null)
            return;

        if (other.transform.gameObject.layer == (int)Layers.Damageable && other.TryGetComponent<HurtBox>(out HurtBox hurt))
        {
            DamageData damageData = m_hitBoxManager.CheckIfCanHitAndGetDamageData(hurt.HurtBoxManager);
            if (damageData != null)
            {
                damageData = damageData.CreateACopy(); //Creates a copy to not alter the original one

                if (damageData.Direction == Vector3.zero)
                {
                    Vector3 dir = (hurt.HurtBoxManager.gameObject.transform.position - m_hitBoxManager.gameObject.transform.position);
                    damageData.Direction = (new Vector3(dir.x, 0, dir.z)).normalized;
                    //Debug.DrawLine(transform.position, transform.position + damageData.Direction, Color.green);

                }
                hurt.HurtBoxWasHit(damageData);
            }
        }

    }


}
