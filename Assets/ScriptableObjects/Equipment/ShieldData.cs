using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldData", menuName = "Scriptable Objects/ShieldData")]
public class ShieldData : ScriptableObject
{
    public string Description = "";

    public Sprite ShieldSprite;
    public GameObject ShieldModel;
    [Space]
    public ShieldDamageNegationData DamageNegationTable;
    [Space]
    public float ShieldingAngle = 0;
    public float ImpactAbsorbtionRecoveryDelay = 0;
    public float ImpactAbsorbtionPerfBlockTimeFrame = 0;


    [Header("Shielding")]
    public SimpleShieldAction shieldIdle;
    public SimpleShieldAction shieldingUpperBody;
    public ShieldAction m_STILL_UNKNOWN; //????????????????????????????????????????????
    public SimpleShieldAction ShiledAlmostStanceBreak;
    public SimpleShieldAction ShiledStanceBreak;

    [Header("ShieldActions")]
    public CombatUtils.PhysicalDamageType PhysicalType = CombatUtils.PhysicalDamageType.Slice;
    public DamageTableData DamageTabel;
    [Space]
    public ShieldAction ShiledSpecialLight1 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, AnimationTypes.Special_Shield_1);
    public ShieldAction ShiledSpecialLight2 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, AnimationTypes.Special_Shield_2);

    public ShieldAction ShiledSpecialHeavy1 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, AnimationTypes.Special_Shield_3);
    public ShieldAction ShiledSpecialHeavy2 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, AnimationTypes.Special_Shield_4);

    [Space]
    public AnimationData SwitchShield;
    public AnimationData ReadyShield;
    public AnimationData RemoveShield;




    public enum ShieldSpecialLight
    {
        Shield_Special_Light_Action_1,
        Shield_Special_Light_Action_2,
        end
    }
    public enum ShieldSpecialHeavy
    {
        Shield_Special_Heavy_Action_1,
        Shield_Special_Heavy_Action_2,
        end
    }



    [System.Serializable]
    public class SimpleShieldAction
    {
        public AnimationData AnimData;
    }


        [System.Serializable]
    public class ShieldAction
    {
        public AnimationData AnimData;

        [NonSerialized] public int ActionkHash;
        public CombatUtils.PhysicalDamageType PhysicalType;
        public DamageMultiplikatorData actionDamageData = new DamageMultiplikatorData(1, 1, 1, 1, 1, 1, 1);
        public StaggerType StaggerType = StaggerType.None;
        public int EnergyCost = 20;
        public int SpecialEnergyCost = 0;
        public ShieldSpecialLight nextSpecialLight;
        public ShieldSpecialHeavy nextSpecialHeavy;


        public ShieldAction(ShieldSpecialLight ssl, ShieldSpecialHeavy ssh, int hash)
        {
            nextSpecialLight = ssl;
            nextSpecialHeavy = ssh;

            ActionkHash = hash;

        }
    }







}
