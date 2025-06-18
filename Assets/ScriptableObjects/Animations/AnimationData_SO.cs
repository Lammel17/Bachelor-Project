using UnityEngine;

[CreateAssetMenu(fileName = "AnimationData", menuName = "Scriptable Objects/AnimationData")]
public class AnimationData : ScriptableObject
{

    public string ActionDescription = "";

    [Space]
    [Header("Animation Data")]
    public AnimationClip animationClip = null;
    public AnimationMovementData AnimationMovementData;
    public BodyParts bodyParts = BodyParts.WholeBody;
    [Space]
    public float crossfadeInTime = 0.2f;
    public float crossfadeOutTime = 0.2f;
    [Tooltip("how long before the end of animation should the crossfade begin ")]
    public float crossfadeBeginn = 0.2f;

    [Header("Interruptability")]
    public AnimationInterruptableType CustomInterruptability = AnimationInterruptableType.SetByButton;
    [Space]
    [Tooltip("If 0, its ignored")]
    public float InterruptabilityChangeBeforeEndTime = 0;
    public AnimationInterruptableType ChangedInterruptability = AnimationInterruptableType.Easily_Interruptable;


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
