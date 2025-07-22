using UnityEngine;

public class ChangeAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    //public AnimatorOverrideController overrideController;
    public static void InitializeAnimationOverrideController(Animator animator, CharacterMovesetData moveset)
    {
        // Make a runtime instance to avoid modifying the shared asset
        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        // Override specific clip
        if (animator == null || moveset == null)
            return;

        //IDLE
        if (moveset.idle != null)
        {
            if (moveset.idle.animationClip != null)
            {
                newOverrideController["EmptyIdle"] = moveset.idle.animationClip;
                if (moveset.idle1.Length == 0) newOverrideController["EmptyIdle1"] = moveset.idle.animationClip;
            }
        }
        if (moveset.idle1.Length != 0)
        {
            foreach (AnimationData animData in moveset.idle1)
            {
                if (animData != null && animData.animationClip != null)
                {
                    newOverrideController["EmptyIdle1"] = animData.animationClip;

                }
                //
                //
                // bla bla if more idles
            }
        }

        //TURNING
        CheckClipAndChange(newOverrideController, moveset.turningLeft, "EmptyTurningLeft");
        CheckClipAndChange(newOverrideController, moveset.turningRight, "EmptyTurningRight");

        //SLOWWALKING
        CheckClipAndChange(newOverrideController, moveset.slowWalkingForward, "EmptySlowWalkingForward");
        CheckClipAndChange(newOverrideController, moveset.slowWalkingLeft, "EmptySlowWalkingLeft");
        CheckClipAndChange(newOverrideController, moveset.slowWalkingRight, "EmptySlowWalkingRight");
        CheckClipAndChange(newOverrideController, moveset.slowWalkingBackwards, "EmptySlowWalkingBackwards");

        //WALKING
        CheckClipAndChange(newOverrideController, moveset.walkingForward, "EmptyWalkingForward");
        CheckClipAndChange(newOverrideController, moveset.walkingLeft, "EmptyWalkingLeft");
        CheckClipAndChange(newOverrideController, moveset.walkingRight, "EmptyWalkingRight");
        CheckClipAndChange(newOverrideController, moveset.walkingBackwards, "EmptyWalkingBackwards");

        //RUNNING
        CheckClipAndChange(newOverrideController, moveset.running, "EmptyRunning");

        //TurningRunning
        CheckClipAndChange(newOverrideController, moveset.turningRunningLeft, "EmptyTurningRunningLeft");
        CheckClipAndChange(newOverrideController, moveset.turningRunningRight, "EmptyTurningRunningRight");

        //EVADE
        CheckClipAndChange(newOverrideController, moveset.evadeForward, "EmptyEvadeForward");
        CheckClipAndChange(newOverrideController, moveset.evadeLeft, "EmptyEvadeLeft");
        CheckClipAndChange(newOverrideController, moveset.evadeRight, "EmptyEvadeRight");
        CheckClipAndChange(newOverrideController, moveset.evadeBackwards, "EmptyEvadeBackwards");

        //Weapons
        ChangeWeaponAnimations(newOverrideController, moveset.weapon);

        //Shield
        ChangeShieldAnimations(newOverrideController, moveset.shield);

        //Item
        ChangeItemAnimations(newOverrideController, moveset.item);
        //ChangeItemAnimations(newOverrideController, moveset.healing);

        //Damage
        if (moveset.getHit != null) newOverrideController["EmptyGetHit"] = moveset.getHit;
        else Debug.Log($"ERROR: no animation clip in AnimationData found in {"EmptyGetHit"}");


        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;




    }



    public static void ChangeWeapon(Animator animator, WeaponData weaponMoveset)
    {
        if (animator == null || weaponMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        ChangeWeaponAnimations(newOverrideController, weaponMoveset);

        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;

    }


    private static void ChangeWeaponAnimations( AnimatorOverrideController newOverrideController, WeaponData weaponMoveset)
    {
        if(weaponMoveset == null)
            return ;

        //LightAttack
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack1.AnimData, "EmptyLightAttack1");
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack2.AnimData, "EmptyLightAttack2");
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack3.AnimData, "EmptyLightAttack3");
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack4.AnimData, "EmptyLightAttack4");
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack5.AnimData, "EmptyLightAttack5");
        CheckClipAndChange(newOverrideController, weaponMoveset.LightAttack6.AnimData, "EmptyLightAttack6");
        CheckClipAndChange(newOverrideController, weaponMoveset.SprintLightAttack.AnimData, "EmptySprintLightAttack");
        CheckClipAndChange(newOverrideController, weaponMoveset.EvadeLightAttack.AnimData, "EmptyEvadeLightAttack");
        CheckClipAndChange(newOverrideController, weaponMoveset.SpecialLightAttack1.AnimData, "EmptySpecialLightAttack1");
        CheckClipAndChange(newOverrideController, weaponMoveset.SpecialLightAttack2.AnimData, "EmptySpecialLightAttack2");

        //HeavyAttack
        CheckClipAndChange(newOverrideController, weaponMoveset.HeavyAttack1.AnimData, "EmptyHeavyAttack1");
        CheckClipAndChange(newOverrideController, weaponMoveset.HeavyAttack2.AnimData, "EmptyHeavyAttack2");
        CheckClipAndChange(newOverrideController, weaponMoveset.HeavyAttack3.AnimData, "EmptyHeavyAttack3");
        CheckClipAndChange(newOverrideController, weaponMoveset.HeavyAttack4.AnimData, "EmptyHeavyAttack4");
        CheckClipAndChange(newOverrideController, weaponMoveset.SprintHeavyAttack.AnimData, "EmptySprintHeavyAttack");
        CheckClipAndChange(newOverrideController, weaponMoveset.EvadeHeavyAttack.AnimData, "EmptyEvadeHeavyAttack");
        CheckClipAndChange(newOverrideController, weaponMoveset.SpecialHeavyAttack1.AnimData, "EmptySpecialHeavyAttack1");
        CheckClipAndChange(newOverrideController, weaponMoveset.SpecialHeavyAttack2.AnimData, "EmptySpecialHeavyAttack2");

    }

    public static void ChangeShield(Animator animator, ShieldData shieldMoveset)
    {
        if (animator == null || shieldMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        ChangeShieldAnimations(newOverrideController, shieldMoveset);

        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;

    }

    private static void ChangeShieldAnimations(AnimatorOverrideController newOverrideController, ShieldData shieldMoveset)
    {
        if (shieldMoveset == null)
            return;

        CheckClipAndChange(newOverrideController, shieldMoveset.shieldIdle.AnimData, "EmptyShieldIdle");
        CheckClipAndChange(newOverrideController, shieldMoveset.shieldingUpperBody.AnimData, "EmptyShieldingUpperBody");

        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledSpecialLight1.AnimData, "EmptyShieldSpecial1");
        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledSpecialLight2.AnimData, "EmptyShieldSpecial2");
        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledSpecialHeavy1.AnimData, "EmptyShieldSpecial3");
        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledSpecialHeavy2.AnimData, "EmptyShieldSpecial4");

        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledAlmostStanceBreak.AnimData, "EmptyShieldAlmostStanceBreak");
        CheckClipAndChange(newOverrideController, shieldMoveset.ShiledStanceBreak.AnimData, "EmptyShieldStanceBreak");


    }


    public static void ChangeItem(Animator animator, ItemData itemMoveset)
    {
        if (animator == null || itemMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        ChangeItemAnimations(newOverrideController, itemMoveset);

        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;

    }

    private static void ChangeItemAnimations(AnimatorOverrideController newOverrideController, ItemData ItemMoveset)
    {
        if (ItemMoveset == null)
            return;

        CheckClipAndChange(newOverrideController, ItemMoveset.ItemUse.AnimData, "EmptyUseItem");
        CheckClipAndChange(newOverrideController, ItemMoveset.ItemUseHold.AnimData, "EmptyuseItemHold");

    }




    private static void CheckClipAndChange(AnimatorOverrideController newOverrideController, AnimationData animData, string animName)
    {
        if (animData != null)
        {
            if (animData.animationClip != null)
            {
                newOverrideController[animName] = animData.animationClip;
                return;
            }
            Debug.Log($"ERROR: no animation clip in AnimationData found in {animData.name}");
        }

        //newOverrideController[animName] = animData.animationClip;
        return;

    }



}
