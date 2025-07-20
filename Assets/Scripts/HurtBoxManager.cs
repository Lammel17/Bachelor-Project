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
    { 

        foreach (HurtBoxCollection collection in m_hurtBoxCollection)//this might cause issues if there are alot of chars with many hurtboxes
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
            (int)(damageData.PhysicalSliceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalBluntDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            (int)(damageData.PhysicalPierceDamage * damageMultiplikator.PhysicalFactor * damageMultiplikator.OverallMultiplicator),
            new Vector2Int((int)(damageData.ThermicDamageAndBuildUp.x * damageMultiplikator.ThermicFactor * damageMultiplikator.OverallMultiplicator), damageData.ThermicDamageAndBuildUp.y),
            new Vector2Int((int)(damageData.ElectricDamageAndBuildUp.x * damageMultiplikator.ElectricFactor * damageMultiplikator.OverallMultiplicator), damageData.ElectricDamageAndBuildUp.y),
            new Vector2Int((int)(damageData.MetaphysicDamageAndBuildUp.x * damageMultiplikator.MetaphysicFactor * damageMultiplikator.OverallMultiplicator), damageData.MetaphysicDamageAndBuildUp.y),
            (int)(damageData.ContaminationBuildUpDamage * damageMultiplikator.AilmentsFactor),
            (int)(damageData.PoiseDamage * damageMultiplikator.PoiseDamageFactor),
            damageData.Direction);

        m_characterStatus.TakeDamageData(damage);
    }


}
