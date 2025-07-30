using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour
{
    [SerializeField] private Collider m_hurtBoxCollider;
    private HurtBoxManager m_hurtboxManager;
    private LayerMask m_triggerLayer;
    private DamageMultiplikatorData m_damageMultiplikator = new DamageMultiplikatorData(1,1,1,1,1,1,1);

    public HurtBoxManager HurtBoxManager { get => m_hurtboxManager; }

    //this might cause issues if there are alot of chars with many hurtboxes
    public void SetValues(LayerMask triggerLayer, HurtBoxManager hurtBoxManager, DamageMultiplikatorData damageMultiplikator) 
    {
        m_triggerLayer = triggerLayer;
        m_hurtboxManager = hurtBoxManager;
        m_damageMultiplikator = damageMultiplikator;

        return;
    }

    public void HurtBoxWasHit(DamageData damageData)
    {
        m_hurtboxManager.GetHitted(m_damageMultiplikator, damageData);

    }






}
