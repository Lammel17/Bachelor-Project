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
        CheckClipAndChange(moveset.turningLeft, "EmptyTurningLeft");
        CheckClipAndChange(moveset.turningRight, "EmptyTurningRight");

        //SLOWWALKING
        CheckClipAndChange(moveset.slowWalkingForward, "EmptySlowWalkingForward");
        CheckClipAndChange(moveset.slowWalkingLeft, "EmptySlowWalkingLeft");
        CheckClipAndChange(moveset.slowWalkingRight, "EmptySlowWalkingRight");
        CheckClipAndChange(moveset.slowWalkingBackwards, "EmptySlowWalkingBackwards");

        //WALKING
        CheckClipAndChange(moveset.walkingForward, "EmptyWalkingForward");
        CheckClipAndChange(moveset.walkingLeft, "EmptyWalkingLeft");
        CheckClipAndChange(moveset.walkingRight, "EmptyWalkingRight");
        CheckClipAndChange(moveset.walkingBackwards, "EmptyWalkingBackwards");

        //RUNNING
        CheckClipAndChange(moveset.running, "EmptyRunning");

        //TurningRunning
        CheckClipAndChange(moveset.turningRunningLeft, "EmptyTurningRunningLeft");
        CheckClipAndChange(moveset.turningRunningRight, "EmptyTurningRunningRight");

        //EVADE
        CheckClipAndChange(moveset.evadeForward, "EmptyEvadeForward");
        CheckClipAndChange(moveset.evadeLeft, "EmptyEvadeLeft");
        CheckClipAndChange(moveset.evadeRight, "EmptyEvadeRight");
        CheckClipAndChange(moveset.evadeBackwards, "EmptyEvadeBackwards");

        //Weapons
        ChangeWeaponAnimations(newOverrideController, moveset.weapon);

        //Shield
        ChangeShieldAnimations(newOverrideController, moveset.shield);




        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;



        void CheckClipAndChange(AnimationData animData, string animName)
        {
            if (animData != null)
            {
                if (animData.animationClip != null)
                {
                    newOverrideController[animName] = animData.animationClip;

                }
            }
        }


    }



    public static void ChangeWeapon(Animator animator, WeaponData weaponMoveset)
    {
        // Make a runtime instance to avoid modifying the shared asset
        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        // Override specific clip
        if (animator == null || weaponMoveset == null)
            return;

        ChangeWeaponAnimations(newOverrideController, weaponMoveset);

        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;

    }


    private static void ChangeWeaponAnimations( AnimatorOverrideController newOverrideController, WeaponData weaponMoveset)
    {


        //LightAttack
        CheckClipAndChange(weaponMoveset.LightAttack1.AnimData, "EmptyLightAttack1");
        CheckClipAndChange(weaponMoveset.LightAttack2.AnimData, "EmptyLightAttack2");
        CheckClipAndChange(weaponMoveset.LightAttack3.AnimData, "EmptyLightAttack3");
        CheckClipAndChange(weaponMoveset.LightAttack4.AnimData, "EmptyLightAttack4");
        CheckClipAndChange(weaponMoveset.LightAttack5.AnimData, "EmptyLightAttack5");
        CheckClipAndChange(weaponMoveset.LightAttack6.AnimData, "EmptyLightAttack6");
        CheckClipAndChange(weaponMoveset.SprintLightAttack.AnimData, "EmptySprintLightAttack");
        CheckClipAndChange(weaponMoveset.EvadeLightAttack.AnimData, "EmptyEvadeLightAttack");
        CheckClipAndChange(weaponMoveset.SpecialLightAttack1.AnimData, "EmptySpecialLightAttack1");
        CheckClipAndChange(weaponMoveset.SpecialLightAttack2.AnimData, "EmptySpecialLightAttack2");

        //HeavyAttack
        CheckClipAndChange(weaponMoveset.HeavyAttack1.AnimData, "EmptyHeavyAttack1");
        CheckClipAndChange(weaponMoveset.HeavyAttack2.AnimData, "EmptyHeavyAttack2");
        CheckClipAndChange(weaponMoveset.HeavyAttack3.AnimData, "EmptyHeavyAttack3");
        CheckClipAndChange(weaponMoveset.HeavyAttack4.AnimData, "EmptyHeavyAttack4");
        CheckClipAndChange(weaponMoveset.SprintHeavyAttack.AnimData, "EmptySprintHeavyAttack");
        CheckClipAndChange(weaponMoveset.EvadeHeavyAttack.AnimData, "EmptyEvadeHeavyAttack");
        CheckClipAndChange(weaponMoveset.SpecialHeavyAttack1.AnimData, "EmptySpecialHeavyAttack1");
        CheckClipAndChange(weaponMoveset.SpecialHeavyAttack2.AnimData, "EmptySpecialHeavyAttack2");




        void CheckClipAndChange(AnimationData animData, string animName)
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

    private static void ChangeShieldAnimations(AnimatorOverrideController newOverrideController, ShieldData shield)
    {


        //LightAttack
        CheckClipAndChange(shield.shieldIdle.AnimData, "EmptyShieldIdle");
        CheckClipAndChange(shield.shieldingUpperBody.AnimData, "EmptyShieldingUpperBody");

        CheckClipAndChange(shield.ShiledSpecial1.AnimData, "EmptyShieldSpecial1");
        CheckClipAndChange(shield.ShiledSpecial2.AnimData, "EmptyShieldSpecial2");
        CheckClipAndChange(shield.ShiledSpecial3.AnimData, "EmptyShieldSpecial3");
        CheckClipAndChange(shield.ShiledSpecial4.AnimData, "EmptyShieldSpecial4");

        CheckClipAndChange(shield.ShiledAlmostStanceBreak.AnimData, "EmptyShieldAlmostStanceBreak");
        CheckClipAndChange(shield.ShiledStanceBreak.AnimData, "EmptyShieldStanceBreak");





        void CheckClipAndChange(AnimationData animData, string animName)
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









}
