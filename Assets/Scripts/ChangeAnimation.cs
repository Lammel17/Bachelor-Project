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


        WeaponData.WeaponActionCount weaponMoves = new WeaponData.WeaponActionCount();
        //LightAttack
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack1.AnimData, "EmptyLightAttack1", true);
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack2.AnimData, "EmptyLightAttack2", weaponMoves.LightAttacks == 1);
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack3.AnimData, "EmptyLightAttack3", weaponMoves.LightAttacks == 2);
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack4.AnimData, "EmptyLightAttack4", weaponMoves.LightAttacks == 3);
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack5.AnimData, "EmptyLightAttack5", weaponMoves.LightAttacks == 4);
        weaponMoves.LightAttacks += CheckClipAndChange(weaponMoveset.LightAttack6.AnimData, "EmptyLightAttack6", weaponMoves.LightAttacks == 5);
        weaponMoves.SprintLightAttacks += CheckClipAndChange(weaponMoveset.SprintLightAttack.AnimData, "EmptySprintLightAttack", true);
        weaponMoves.EvadeLightAttacks += CheckClipAndChange(weaponMoveset.EvadeLightAttack.AnimData, "EmptyEvadeLightAttack", true);
        weaponMoves.SpecialLightAttacks += CheckClipAndChange(weaponMoveset.SpecialLightAttack1.AnimData, "EmptySpecialLightAttack1", true);
        weaponMoves.SpecialLightAttacks += CheckClipAndChange(weaponMoveset.SpecialLightAttack2.AnimData, "EmptySpecialLightAttack2", weaponMoves.SpecialLightAttacks == 1);

        //HeavyAttack
        weaponMoves.HeavyAttacks += CheckClipAndChange(weaponMoveset.HeavyAttack1.AnimData, "EmptyHeavyAttack1", true);
        weaponMoves.HeavyAttacks += CheckClipAndChange(weaponMoveset.HeavyAttack2.AnimData, "EmptyHeavyAttack2", weaponMoves.HeavyAttacks == 1);
        weaponMoves.HeavyAttacks += CheckClipAndChange(weaponMoveset.HeavyAttack3.AnimData, "EmptyHeavyAttack3", weaponMoves.HeavyAttacks == 2);
        weaponMoves.HeavyAttacks += CheckClipAndChange(weaponMoveset.HeavyAttack4.AnimData, "EmptyHeavyAttack4", weaponMoves.HeavyAttacks == 3);
        weaponMoves.SprintheavyAttacks += CheckClipAndChange(weaponMoveset.SprintHeavyAttack.AnimData, "EmptySprintHeavyAttack", true);
        weaponMoves.EvadeHeavyAttacks += CheckClipAndChange(weaponMoveset.EvadeHeavyAttack.AnimData, "EmptyEvadeHeavyAttack", true);
        weaponMoves.SpecialHeavyAttacks += CheckClipAndChange(weaponMoveset.SpecialHeavyAttack1.AnimData, "EmptySpecialHeavyAttack1", true);
        weaponMoves.SpecialHeavyAttacks += CheckClipAndChange(weaponMoveset.SpecialHeavyAttack2.AnimData, "EmptySpecialHeavyAttack2", weaponMoves.SpecialHeavyAttacks == 1);

        weaponMoveset.weaponActionCount = weaponMoves;



        int CheckClipAndChange(AnimationData animData, string animName, bool applyAnim)
        {
            if (applyAnim && animData != null)
            {
                if (animData.animationClip != null)
                {
                    newOverrideController[animName] = animData.animationClip;
                    return 1;
                }
                Debug.Log($"ERROR: no animation clip in AnimationData found in {animData.name}");
            }

            //newOverrideController[animName] = animData.animationClip;
            return 0;
            
        }


    }




}
