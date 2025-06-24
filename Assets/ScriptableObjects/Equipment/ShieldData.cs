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
    public ShieldAction ShiledSpecial1 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_1"));
    public ShieldAction ShiledSpecial2 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_1"));

    public ShieldAction ShiledSpecial3 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_1"));
    public ShieldAction ShiledSpecial4 = new ShieldAction(ShieldSpecialLight.end, ShieldSpecialHeavy.end, Animator.StringToHash("Special_Shield_1"));




    public enum ShieldSpecialLight
    {
        Special_Shield_Attack_1,
        Special_Shield_Attack_2,
        end
    }
    public enum ShieldSpecialHeavy
    {
        Special_Shield_Attack_3,
        Special_Shield_Attack_4,
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
        public ActionDamageData DamageData = new ActionDamageData(1, 1, 1, 1);
        public float EnergyCost = 20;
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
