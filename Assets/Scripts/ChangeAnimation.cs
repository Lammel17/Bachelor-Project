using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterActionAndMovementHandler))]
public class ChangeAnimation : MonoBehaviour
{
    [SerializeField][EditorAttributes.ReadOnly] public String info = "Script is only for the Player!";

    [SerializeField][EditorAttributes.ReadOnly] private Animator m_animator;
    private AnimatorOverrideController m_animatorOverrideController = null;
    private AnimationClipOverrides m_clipOverrides;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    m_animator = GetComponent<CharacterActionAndMovementHandler>().Animator;
    //    m_animatorOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);
    //    m_animator.runtimeAnimatorController = m_animatorOverrideController;

    //    m_clipOverrides = new AnimationClipOverrides(m_animatorOverrideController.overridesCount);
    //    m_animatorOverrideController.GetOverrides(m_clipOverrides);

    //}


    //public AnimatorOverrideController overrideController;
    public void InitializeAnimationOverrideController(CharacterMovesetData moveset)
    {
        m_animator = GetComponent<CharacterActionAndMovementHandler>().Animator;
        m_animatorOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);
        m_animator.runtimeAnimatorController = m_animatorOverrideController;

        m_clipOverrides = new AnimationClipOverrides(m_animatorOverrideController.overridesCount);
        m_animatorOverrideController.GetOverrides(m_clipOverrides);

        // Override specific clip
        if (m_animator == null || moveset == null)
            return;

        //IDLE
        if (moveset.idle != null)
        {
            if (moveset.idle.animationClip != null)
            {
                m_clipOverrides["EmptyIdle"] = moveset.idle.animationClip;
                if (moveset.idle1.Length == 0) m_clipOverrides["EmptyIdle1"] = moveset.idle.animationClip;
            }
        }
        if (moveset.idle1.Length != 0)
        {
            foreach (AnimationData animData in moveset.idle1)
            {
                if (animData != null && animData.animationClip != null)
                {
                    m_clipOverrides["EmptyIdle1"] = animData.animationClip;

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
        ChangeWeaponAnimations(moveset.weapon);

        //Shield
        ChangeShieldAnimations(moveset.shield);

        //Item
        ChangeItemAnimations(moveset.item);
        //ChangeItemAnimations(newOverrideController, moveset.healing);

        //Damage
        if (moveset.getHit != null) m_clipOverrides["EmptyGetHit"] = moveset.getHit;
        else Debug.Log($"ERROR: no animation clip in AnimationData found in {"EmptyGetHit"}");


        // Apply the override to the Animator
        //m_animator.runtimeAnimatorController = newOverrideController;
        m_animatorOverrideController.ApplyOverrides(m_clipOverrides);



    }



    public void ChangeWeapon( WeaponData weaponMoveset)
    {
        if (m_animator == null || weaponMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);

        ChangeWeaponAnimations(weaponMoveset);

        // Apply the override to the Animator
        m_animatorOverrideController.ApplyOverrides(m_clipOverrides);

    }


    private void ChangeWeaponAnimations( WeaponData weaponMoveset)
    {
        if(weaponMoveset == null)
            return ;

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

        //SwitchWeapon
        CheckClipAndChange(weaponMoveset.SwitchWeapon, "EmptySwitchWeapon");
    }

    public void ChangeShield(ShieldData shieldMoveset)
    {
        if (m_animator == null || shieldMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);

        ChangeShieldAnimations(shieldMoveset);

        // Apply the override to the Animator
        m_animatorOverrideController.ApplyOverrides(m_clipOverrides);

    }

    private void ChangeShieldAnimations(ShieldData shieldMoveset)
    {
        if (shieldMoveset == null)
            return;

        CheckClipAndChange(shieldMoveset.shieldIdle.AnimData, "EmptyShieldIdle");
        CheckClipAndChange(shieldMoveset.shieldingUpperBody.AnimData, "EmptyShieldingUpperBody");

        CheckClipAndChange(shieldMoveset.ShiledSpecialLight1.AnimData, "EmptyShieldSpecial1");
        CheckClipAndChange(shieldMoveset.ShiledSpecialLight2.AnimData, "EmptyShieldSpecial2");
        CheckClipAndChange(shieldMoveset.ShiledSpecialHeavy1.AnimData, "EmptyShieldSpecial3");
        CheckClipAndChange(shieldMoveset.ShiledSpecialHeavy2.AnimData, "EmptyShieldSpecial4");

        CheckClipAndChange(shieldMoveset.ShiledAlmostStanceBreak.AnimData, "EmptyShieldAlmostStanceBreak");
        CheckClipAndChange(shieldMoveset.ShiledStanceBreak.AnimData, "EmptyShieldStanceBreak");

        //SwitchShield
        CheckClipAndChange(shieldMoveset.SwitchShield, "EmptySwitchShield");


    }


    public void ChangeItem( ItemData itemMoveset)
    {
        if (m_animator == null || itemMoveset == null)
            return;

        AnimatorOverrideController newOverrideController = new AnimatorOverrideController(m_animator.runtimeAnimatorController);

        ChangeItemAnimations(itemMoveset);

        // Apply the override to the Animator
        m_animatorOverrideController.ApplyOverrides(m_clipOverrides);

    }

    private void ChangeItemAnimations(ItemData ItemMoveset)
    {
        if (ItemMoveset == null)
            return;

        CheckClipAndChange( ItemMoveset.ItemUse.AnimData, "EmptyUseItem");
        CheckClipAndChange( ItemMoveset.ItemUseHold.AnimData, "EmptyuseItemHold");

    }




    private void CheckClipAndChange(AnimationData animData, string animName)
    {
        if (animData != null)
        {
            if (animData.animationClip != null)
            {
                m_clipOverrides[animName] = animData.animationClip;
                return;
            }
            Debug.Log($"ERROR: no animation clip in AnimationData found in {animData.name}");
        }

        //newOverrideController[animName] = animData.animationClip;
        return;

    }



}

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
