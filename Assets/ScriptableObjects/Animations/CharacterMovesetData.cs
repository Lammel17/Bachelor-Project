using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MovesetData", menuName = "Scriptable Objects/MovesetData")]
public class CharacterMovesetData : ScriptableObject
{


    public string Description = "";
    public AnimationData emptyFallbackAnimation;
    [Header("Action Animations")]

    [Header("Non-Action Animations")]
    public AnimationData idle;
    public AnimationData[] idle1;
    [Space]
    public AnimationData slowWalkingForward;
    public AnimationData slowWalkingLeft;
    public AnimationData slowWalkingRight;
    public AnimationData slowWalkingBackwards;
    [Space]
    public AnimationData walkingForward;
    public AnimationData walkingLeft;
    public AnimationData walkingRight;
    public AnimationData walkingBackwards;
    [Space]
    public AnimationData running;
    [Space]
    [Header("Action Animations")]
    [Space]
    public AnimationData turningLeft;
    public AnimationData turningRight;
    public AnimationData turningRunningLeft;
    public AnimationData turningRunningRight;
    [Space]
    public AnimationData evadeForward;
    public AnimationData evadeLeft;
    public AnimationData evadeRight;
    public AnimationData evadeBackwards;
    [Space]
    [Space]
    public WeaponData weapon;
    [Space]
    public ShieldData shield;
    [Space]
    //public ItemData healing;
    public ItemData item;
    [Space]
    public AnimationClip getHit;













}
