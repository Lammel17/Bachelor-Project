using UnityEngine;

[CreateAssetMenu(fileName = "MovesetData", menuName = "Scriptable Objects/MovesetData")]
public class CharacterMovesetData : ScriptableObject
{

    public string Description = "";
    [Header("AnimationData Files")]
    public AnimationData idle;
    [Space]
    public AnimationData turningLeft;
    public AnimationData turningRight;
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
    public AnimationData turningRunningLeft;
    public AnimationData turningRunningRight;
    [Space]
    [Space]
    public AnimationData evadeForward;
    public AnimationData evadeLeft;
    public AnimationData evadeRight;
    public AnimationData evadeBackwards;














}
