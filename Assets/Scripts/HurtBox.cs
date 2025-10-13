using UnityEngine;
using EditorAttributes;


[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour
{
    [SerializeField][Required] private Collider m_hurtBoxCollider;
    private HurtBoxManager m_hurtboxManager;
    private DamageMultiplikatorData m_damageMultiplikator = new DamageMultiplikatorData(1,1,1,1,1,1,1);
    private bool m_isBlockingBox = false;

    public HurtBoxManager HurtBoxManager { get => m_hurtboxManager; }

    //this might cause issues if there are alot of chars with many hurtboxes
    public void SetValues(HurtBoxManager hurtBoxManager, DamageMultiplikatorData damageMultiplikator, bool isBlockingBox = false) 
    {
        m_hurtboxManager = hurtBoxManager;
        m_damageMultiplikator = damageMultiplikator;
        m_isBlockingBox = isBlockingBox;

        return;
    }

    public void HurtBoxWasHit(DamageData damageData)
    {
        m_hurtboxManager.GetHitted(m_damageMultiplikator, damageData, m_isBlockingBox);

    }


    public void EnableHurtBox(bool enable)
    {
        m_hurtBoxCollider.enabled = enable;
    }



}
