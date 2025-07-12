using NUnit.Framework;
using UnityEngine;

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
    { //this might cause issues if there are alot of chars with many hurtboxes

        foreach (HurtBoxCollection collection in m_hurtBoxCollection)
        {
            foreach (HurtBox hurtbox in collection.hurtBoxes)
            {
                hurtbox.SetValues(m_triggerLayer, this, collection.bodyPartDamageMultiplikator);
            }
        }
    }


    public void TriggerCollision(DamageMultiplikatorData damageMultiplikator, DamageData damageData)
    {
        if (m_characterStatus == null)
            return;

        DamageData damage = new DamageData(
            (int)(damageData.PoiseDamage * damageMultiplikator.PoiseDamageFactor),
            (int)(damageData.PhysicalSliceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalBluntDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalPierceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.ThermicDamage * damageMultiplikator.ThermicFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.ElectricDamage * damageMultiplikator.ElectricFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.MetaphysicDamage * damageMultiplikator.MetaphysicFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.ContaminationBuildUpDamage * damageMultiplikator.AilmentsFactor));
    }


}
