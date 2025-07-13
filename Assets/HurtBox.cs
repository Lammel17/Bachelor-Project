using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private HurtBoxManager m_hurtboxManager;
    private LayerMask m_triggerLayer;
    private DamageMultiplikatorData m_damageMultiplikator = new DamageMultiplikatorData(1,1,1,1,1,1,1);

    //this might cause issues if there are alot of chars with many hurtboxes
    public void SetValues(LayerMask triggerLayer, HurtBoxManager hurtBoxManager, DamageMultiplikatorData damageMultiplikator) 
    {
        m_triggerLayer = triggerLayer;
        m_hurtboxManager = hurtBoxManager;
        m_damageMultiplikator = damageMultiplikator;

        return;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_hurtboxManager == null)
            return;
        if ((m_triggerLayer.value & (1 << other.transform.gameObject.layer)) == 0)
            return;

        if(other.TryGetComponent<HitBox>(out HitBox hit))
        {
            DamageData damageData = hit.GetDamageData();
            m_hurtboxManager.TriggerCollision(m_damageMultiplikator, damageData);
        }
        


    }






}
