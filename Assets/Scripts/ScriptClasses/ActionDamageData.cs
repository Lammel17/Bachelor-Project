using UnityEngine;

    [System.Serializable]
public class ActionDamageData
{

        public float PhysicalFactor = 1;
        public float ElementalFactor = 1;
        public float AlimentFactor = 1;

        public float PoiseDamageFactor = 1;

        public ActionDamageData(float pp, float ep, float ap, float pd)
        {
            PhysicalFactor = pp;
            ElementalFactor = ep;
            AlimentFactor = ap;
            PoiseDamageFactor = pd;
        }
}
