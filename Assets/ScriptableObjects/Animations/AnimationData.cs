using EditorAttributes;
using GD.MinMaxSlider;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "AnimationData", menuName = "Scriptable Objects/AnimationData")]
public class AnimationData : ScriptableObject
{

    public string ActionDescription = "";

    [Space]
    [Header("-> Animation Data")]
    public AnimationClip animationClip = null;
    public AnimationMovementData AnimationMovementData;
    public BodyParts bodyParts = BodyParts.WholeBody;
    [Space]
    public float crossfadeInTime = 0.2f;
    public float crossfadeOutTime = 0.2f;
    [Tooltip("how long before the end of animation should the crossfade begin ")]
    public float crossfadeOutBeginn = 0.2f;

    [Header("-> Interruptability")]
    public AnimationInterruptableType CustomInterruptability = AnimationInterruptableType.SetByButton;
    [Space]
    [Tooltip("If 0, its ignored")]
    public float InterruptabilityChangeBeforeEndTime = 0;
    public AnimationInterruptableType ChangedInterruptability = AnimationInterruptableType.Easily_Interruptable;
    [Space]
    [Space]
    [Space]
    [Space]
    [Header("-> MainActionMoment in Relative Time ")]
    [Tooltip("Used mostly for the moment the actionCosts are paid, or the weapon is swaped...")]
    [UnityEngine.Range(0,1)] public float MainActionMomentTime = 0;
    [Space]
    [Header("-> HitBoxes Active in Relative Time ")]
    [Tooltip("Used if the action is part of a weapon with hitboxes.")]
    [SerializeField] public List<HitBoxActiveData> hitBoxActiveData;
    [Space]
    [Space]
    [Space]
    [Space]
    [Header("-> Pause Anim when is not Grounded in Relative Time ")]
    public bool IsPausingMidAir = false;
    [UnityEngine.Range(0, 1)] public float PauseMidAirTime = 0;
    [Space]
    [Header("-> Pause Gravity in Relative Time ")]
    public bool IsPausingGravity = false;
    [GD.MinMaxSlider.MinMaxSlider(0,1)] public Vector2 PauseGravityTime = new Vector2(0,0);
    [Space]
    [Space]
    [Space]
    [Space]
    [Header("-> CorrectSpineRotations for Non Base Layer Animations")]
    [Tooltip("Bool affects only Action-Animations! This makes the action use the Look At Target correction. Usually Actions do not do this, since it was made for walking while looking at target.")]
    public bool actionUsesLookAtTargetData = false;
    [Space]
    [Tooltip("Works only with Action-Animations! Corrects weird looking Upper Body Animations")]
    public bool useLookAtForwardData = false;
    public LookAtData lookAtData;

    [System.Serializable]
    public class HitBoxActiveData
    {
        [Tooltip("The hitBoxReference number is refernce to the hitboxCollection of the weapons hitboxes. (0 is the default)")]
        [SerializeField] public int CollectionRefNumber;
        [SerializeField][GD.MinMaxSlider.MinMaxSlider(0, 1)] public Vector2 activeTime = new Vector2(0, 0);

        //[SerializeField] public AnimationCurve activeTime = AnimationCurve.Linear(0, 0, 0, 0);
    }

    //[Header("Invincibility")]
    //public Invincibility invincibilitySettings;

    //[Space]
    //[SerializeField] public Effects[] effectsList;

    public enum BodyParts
    {
        WholeBody = 0,
        UpperBody = 1,
        Arms = 2,
        RightArm = 3,
        LeftArm = 4
    }






    [System.Serializable]
    public class Invincibility
    {
        public bool hasInvincibilityFrames = false;
        [UnityEngine.Range(0, 1)] public float invincibilityStart = 0;
        public float invincibilityDuration = 0;
    }

    public enum EffectType
    {
        None
    }

    [System.Serializable]
    public class Effects
    {
        public EffectType effect = EffectType.None;

        public bool ignore = false;
        public float value = 0;
        [UnityEngine.Range(0, 1)] public float EffectStart = 0;
        float effectDuration = 0;


    }




}
