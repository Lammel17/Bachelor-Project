using EditorAttributes;
using GD.MinMaxSlider;
using UnityEngine;

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
    public float crossfadeBeginn = 0.2f;

    [Header("-> Interruptability")]
    public AnimationInterruptableType CustomInterruptability = AnimationInterruptableType.SetByButton;
    [Space]
    [Tooltip("If 0, its ignored")]
    public float InterruptabilityChangeBeforeEndTime = 0;
    public AnimationInterruptableType ChangedInterruptability = AnimationInterruptableType.Easily_Interruptable;
    [Space]
    [Header("-> MainActionMoment in Relative Time ")]
    [Tooltip("Used mostly for the moment the actionCosts are paid, or the weapon is swaped...")]
    [Range(0,1)] public float MainActionMomentTime = 0;
    [Space]
    [Header("-> Pause Anim when is not Grounded in Relative Time ")]
    public bool IsPausingMidAir = false;
    [Range(0, 1)] public float PauseMidAirTime = 0;
    [Space]
    [Header("-> Pause Gravity in Relative Time ")]
    public bool IsPausingGravity = false;
    [GD.MinMaxSlider.MinMaxSlider(0,1)] public Vector2 PauseGravityTime = new Vector2(0,0);
    [Space]
    [Space]
    [Space]
    [Space]
    [Header("-> CorrectSpineRotations for Non Base Layer Animations")]
    [Tooltip("For Action-Animations only! This makes the action use the Look At Target correction. Usually Actions do not do this, since it was made for walking while looking at target.")]
    public bool actionUsesLookAtTargetData = false;
    [Tooltip("Works only with Action-Animations! Corrects weird looking Upper Body Animations")]
    public bool useLookAtForwardData = false;
    public LookAtData lookAtData;

    //[Header("Invincibility")]
    //public Invincibility invincibilitySettings;

    //[Space]
    //[SerializeField] public Effects[] effectsList;

    public enum BodyParts
    {
        WholeBody = 0,
        UpperBody = 1,
        Arms = 2,
    }






    [System.Serializable]
    public class Invincibility
    {
        public bool hasInvincibilityFrames = false;
        [Range(0, 1)] public float invincibilityStart = 0;
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
        [Range(0, 1)] public float EffectStart = 0;
        float effectDuration = 0;


    }




}
