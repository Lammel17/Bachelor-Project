using System;
using UnityEngine;
using static WeaponData;

[CreateAssetMenu(fileName = "ShieldData", menuName = "Scriptable Objects/ShieldData")]
public class ShieldData : ScriptableObject
{
    public string Description = "";

    public DamageTableData DamageNegationTable;


    [Header("Shielding")]
    public SimpleShieldAction shieldIdle;
    public SimpleShieldAction shieldingUpperBody;
    public ShieldAction m_STILL_UNKNOWN; //????????????????
    public SimpleShieldAction ShiledAlmostStanceBreak;
    public SimpleShieldAction ShiledStanceBreak;

    [Header("ShieldActions")]
    public DamageTableData DamageTable;
    [Space]
    public ShieldAction ShiledSpecialLight1 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_1"));
    public ShieldAction ShiledSpecialLight2 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_2"));

    public ShieldAction ShiledSpecialHeavy1 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_3"));
    public ShieldAction ShiledSpecialHeavy2 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_4"));




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
        public DamageMultiplikatorData DamageData = new DamageMultiplikatorData(1, 1, 1, 1, 1, 1, 1);
        public int EnergyCost = 20;
        public float SpecialEnergyCost = 0;
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
