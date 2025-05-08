using UnityEngine;

[CreateAssetMenu(fileName = "MovesetData", menuName = "Scriptable Objects/MovesetData")]
public class CharacterMovesetData : ScriptableObject
{

    public string Description = "";

    public AnimationClip idle;
    [Space]
    public AnimationClip slowWalkingForward;
    public AnimationClip slowWalkingLeft;
    public AnimationClip slowWalkingRight;
    public AnimationClip slowWalkingBack;
    [Space]
    public AnimationClip walkingForward;
    public AnimationClip walkingLeft;
    public AnimationClip walkingRight;
    public AnimationClip walkingBack;
    [Space]
    public AnimationClip Running;
    [Space]
    [Space]
    public ActionData evadeForward;
    public ActionData evadeLeft;
    public ActionData evadeRight;
    public ActionData evadeBack;














}
