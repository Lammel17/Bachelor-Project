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
                if (moveset.idle1.Length == 0)
                    newOverrideController["EmptyIdle1"] = moveset.idle.animationClip;

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
        if (moveset.turningLeft != null)
        {
            if (moveset.turningLeft.animationClip != null)
            {
                newOverrideController["EmptyTurningLeft"] = moveset.turningLeft.animationClip;

            }
        }
        if (moveset.turningRight != null)
        {
            if (moveset.turningRight.animationClip != null)
            {
                newOverrideController["EmptyTurningRight"] = moveset.turningRight.animationClip;

            }
        }

        //SLOWWALKING
        if (moveset.slowWalkingForward != null)
        {
            if (moveset.slowWalkingForward.animationClip != null)
            {
                newOverrideController["EmptySlowWalkingForward"] = moveset.slowWalkingForward.animationClip;

            }
        }
        if (moveset.slowWalkingLeft != null)
        {
            if (moveset.slowWalkingLeft.animationClip != null)
            {
                newOverrideController["EmptySlowWalkingLeft"] = moveset.slowWalkingLeft.animationClip;

            }
        }
        if (moveset.slowWalkingRight != null)
        {
            if (moveset.slowWalkingRight.animationClip != null)
            {
                newOverrideController["EmptySlowWalkingRight"] = moveset.slowWalkingRight.animationClip;

            }
        }
        if (moveset.slowWalkingBackwards != null)
        {
            if (moveset.slowWalkingBackwards.animationClip != null)
            {
                newOverrideController["EmptySlowWalkingBackwards"] = moveset.slowWalkingBackwards.animationClip;

            }
        }

        //WALKING
        if (moveset.walkingForward != null)
        {
            if (moveset.walkingForward.animationClip != null)
            {
                newOverrideController["EmptyWalkingForward"] = moveset.walkingForward.animationClip;

            }
        }
        if (moveset.walkingLeft != null)
        {
            if (moveset.walkingLeft.animationClip != null)
            {
                newOverrideController["EmptyWalkingLeft"] = moveset.walkingLeft.animationClip;

            }
        }
        if (moveset.walkingRight != null)
        {
            if (moveset.walkingRight.animationClip != null)
            {
                newOverrideController["EmptyWalkingRight"] = moveset.walkingRight.animationClip;

            }
        }
        if (moveset.walkingBackwards != null)
        {
            if (moveset.walkingBackwards.animationClip != null)
            {
                newOverrideController["EmptyWalkingBackwards"] = moveset.walkingBackwards.animationClip;

            }
        }

        //RUNNING
        if (moveset.running != null)
        {
            if (moveset.running.animationClip != null)
            {
                newOverrideController["EmptyRunning"] = moveset.running.animationClip;

            }
        }

        //TURNING RUNNING
        if (moveset.turningRunningLeft != null)
        {
            if (moveset.turningRunningLeft.animationClip != null)
            {
                newOverrideController["EmptyTurningRunningLeft"] = moveset.turningRunningLeft.animationClip;

            }
        }
        if (moveset.turningRunningRight != null)
        {
            if (moveset.turningRunningRight.animationClip != null)
            {
                newOverrideController["EmptyTurningRunningRight"] = moveset.turningRunningRight.animationClip;

            }
        }

        //EVADE
        if (moveset.evadeForward != null)
        {
            if (moveset.evadeForward.animationClip != null)
            {
                newOverrideController["EmptyEvadeForward"] = moveset.evadeForward.animationClip;

            }
        }
        if (moveset.evadeLeft != null)
        {
            if (moveset.evadeLeft.animationClip != null)
            {
                newOverrideController["EmptyEvadeLeft"] = moveset.evadeLeft.animationClip;

            }
        }
        if (moveset.evadeRight != null)
        {
            if (moveset.evadeRight.animationClip != null)
            {
                newOverrideController["EmptyEvadeRight"] = moveset.evadeRight.animationClip;

            }
        }
        if (moveset.evadeBackwards != null)
        {
            if (moveset.evadeBackwards.animationClip != null)
            {
                newOverrideController["EmptyEvadeBackwards"] = moveset.evadeBackwards.animationClip;

            }
        }


        // Apply the override to the Animator
        animator.runtimeAnimatorController = newOverrideController;





    }

}
