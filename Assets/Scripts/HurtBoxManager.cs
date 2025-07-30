using NUnit.Framework;
using UnityEngine;


[RequireComponent(typeof(CharacterStatus))]
public class HurtBoxManager : MonoBehaviour
{
    [SerializeField] private LayerMask m_triggerLayer;
    [SerializeField] private CharacterStatus m_characterStatus;
    [SerializeField] private HurtBoxCollection[] m_hurtBoxCollection;

    [System.Serializable]
    public class HurtBoxCollection
    {
        public DamageMultiplikatorData bodyPartDamageMultiplikator = new DamageMultiplikatorData(1,1,1,1,1,1,1);
        public HurtBox[] hurtBoxes;
    }

    public void Start()
    { 

        foreach (HurtBoxCollection collection in m_hurtBoxCollection)//this might cause issues if there are alot of chars with many hurtboxes
        {
            foreach (HurtBox hurtbox in collection.hurtBoxes)
            {
                hurtbox.SetValues(m_triggerLayer, this, collection.bodyPartDamageMultiplikator);
            }
        }
    }


    public void GetHitted(DamageMultiplikatorData damageMultiplikator, DamageData damageData)
    {
        if (m_characterStatus == null)
            return;

        m_characterStatus.TakeDamageByDamageData(CombatUtils.CalculateMultiplicatedDamageData(damageMultiplikator, damageData));
    }


}
