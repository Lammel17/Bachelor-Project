using UnityEngine;

[CreateAssetMenu(fileName = "ActionData", menuName = "Scriptable Objects/ActionData")]
public class ActionData : ScriptableObject
{

    public string ActionDescription = "";

    [Space]
    [Header("Animation Data")]
    public AnimationClip animationClip = null;
    public float animationSpeed = 1;
    public AnimationMovementData AnimationMovementData;


    [Header("Invincibility")]
    public Invincibility invincibilitySettings;

    [Space]
    [SerializeField] public Effects[] effectsList;








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
