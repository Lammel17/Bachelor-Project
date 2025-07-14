using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour
{
    [SerializeField] private Collider m_hurtBoxCollider;
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

    public void ActivateHurtBox(DamageData damageData)
    {
        if (m_hurtBoxCollider == null)
            m_hurtBoxCollider.GetComponent<Collider>();
        m_hurtBoxCollider.enabled = true;
    }
    public void DeactivateHurtBox()
    {
        if (m_hurtBoxCollider == null)
            m_hurtBoxCollider.GetComponent<Collider>();
        m_hurtBoxCollider.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (m_hurtboxManager == null)
            return;
        if ((m_triggerLayer.value & (1 << other.transform.gameObject.layer)) == 0)
            return;

        if(other.TryGetComponent<HitBox>(out HitBox hit))
        {

            if (!hit.CheckIfCanBeHit(m_hurtboxManager))
                return;

            DamageData damageData = hit.HurtBoxWasHit(m_hurtboxManager);
            m_hurtboxManager.TriggerCollision(m_damageMultiplikator, damageData);
        }
        


    }






}
