using UnityEngine;

public class AnimationTypes
{
    public static readonly int Reset                          = Animator.StringToHash("Reset");

    public static readonly int Empty                          = Animator.StringToHash("Empty");
    public static readonly int Idle_1                         = Animator.StringToHash("Idle_1");
    public static readonly int Locomotion                     = Animator.StringToHash("Locomotion");
    public static readonly int Running                        = Animator.StringToHash("Running");

    public static readonly int Running_Sliding                = Animator.StringToHash("Running_Sliding");
    public static readonly int Turning                        = Animator.StringToHash("Turning");
    public static readonly int Turning_Running                = Animator.StringToHash("Turning_Running");

    public static readonly int Evade_Forward                  = Animator.StringToHash("Evade_Forward");
    public static readonly int Evade_Left                     = Animator.StringToHash("Evade_Left");
    public static readonly int Evade_Right                    = Animator.StringToHash("Evade_Right");
    public static readonly int Evade_Backwards                = Animator.StringToHash("Evade_Backwards");

    public static readonly int Use_Item                       = Animator.StringToHash("Use_Item");
    public static readonly int Use_Item_Hold                  = Animator.StringToHash("Use_Item_Hold");
    public static readonly int Healing                        = Animator.StringToHash("Healing");

    public static readonly int Environment_Interaction        = Animator.StringToHash("Environment_Interaction");
    public static readonly int Pick_Up_Item_Low               = Animator.StringToHash("Pick_Up_Item_Low");
    public static readonly int Pick_Up_Item_Up                = Animator.StringToHash("Pick_Up_Item_Up");

    public static readonly int Switch_Weapon                  = Animator.StringToHash("Switch_Weapon");
    public static readonly int Switch_Shield                  = Animator.StringToHash("Switch_Shield");
    public static readonly int Ready_Weapon                   = Animator.StringToHash("Ready_Weapon");
    public static readonly int Ready_Shield                   = Animator.StringToHash("Ready_Shield");
    public static readonly int Remove_Weapon                  = Animator.StringToHash("Remove_Weapon");
    public static readonly int Remove_Shield                  = Animator.StringToHash("Remove_Shield");

    public static readonly int Light_Attack_1                 = Animator.StringToHash("Light_Attack_1");
    public static readonly int Light_Attack_2                 = Animator.StringToHash("Light_Attack_2");
    public static readonly int Light_Attack_3                 = Animator.StringToHash("Light_Attack_3");
    public static readonly int Light_Attack_4                 = Animator.StringToHash("Light_Attack_4");
    public static readonly int Light_Attack_5                 = Animator.StringToHash("Light_Attack_5");
    public static readonly int Light_Attack_6                 = Animator.StringToHash("Light_Attack_6");
    public static readonly int Sprint_Light_Attack            = Animator.StringToHash("Sprint_Light_Attack");
    public static readonly int Evade_Light_Attack             = Animator.StringToHash("Evade_Light_Attack");
    public static readonly int Special_Light_Attack_1         = Animator.StringToHash("Special_Light_Attack_1");
    public static readonly int Special_Light_Attack_2         = Animator.StringToHash("Special_Light_Attack_2");
    public static readonly int Heavy_Attack_1                 = Animator.StringToHash("Heavy_Attack_1");
    public static readonly int Heavy_Attack_2                 = Animator.StringToHash("Heavy_Attack_2");
    public static readonly int Heavy_Attack_3                 = Animator.StringToHash("Heavy_Attack_3");
    public static readonly int Heavy_Attack_4                 = Animator.StringToHash("Heavy_Attack_4");
    public static readonly int Sprint_Heavy_Attack            = Animator.StringToHash("Sprint_Heavy_Attack");
    public static readonly int Evade_Heavy_Attack             = Animator.StringToHash("Evade_Heavy_Attack");
    public static readonly int Special_Heavy_Attack_1         = Animator.StringToHash("Special_Heavy_Attack_1");
    public static readonly int Special_Heavy_Attack_2         = Animator.StringToHash("Special_Heavy_Attack_2");

    public static readonly int Shielding                      = Animator.StringToHash("Shielding");
    public static readonly int Special_Shield_1               = Animator.StringToHash("Special_Shield_1");
    public static readonly int Special_Shield_2               = Animator.StringToHash("Special_Shield_2");
    public static readonly int Special_Shield_3               = Animator.StringToHash("Special_Shield_3");
    public static readonly int Special_Shield_4               = Animator.StringToHash("Special_Shield_4");
    public static readonly int Almost_Stance_Break            = Animator.StringToHash("Almost_Stance_Break");
    public static readonly int Stance_Break                   = Animator.StringToHash("Stance_Break");

    public static readonly int Falling_Forward                = Animator.StringToHash("Falling_Forward");
    public static readonly int Standing_Up_Forward            = Animator.StringToHash("Standing_Up_Forward");
    public static readonly int Falling_Backward               = Animator.StringToHash("Falling_Backward");
    public static readonly int Standing_Up_Backward           = Animator.StringToHash("Standing_Up_Backward");

    public static readonly int Falling_Mid_Air                = Animator.StringToHash("Falling_Mid_Air");
    public static readonly int Landing                        = Animator.StringToHash("Landing");

    public static readonly int Get_Hit                        = Animator.StringToHash("Get_Hit");
}
