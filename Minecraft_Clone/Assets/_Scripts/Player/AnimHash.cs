using UnityEngine;

public static class AnimHash
{
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int Grounded = Animator.StringToHash("Grounded");
    public static readonly int Swinging = Animator.StringToHash("Swinging");
    public static readonly int SwingRolling = Animator.StringToHash("SwingRolling");
    public static readonly int Hanging = Animator.StringToHash("Hanging");
}