using UnityEngine;

public enum PlayerActionState
{
    None,
    NormalAttack
}

public class PlayerState : MonoBehaviour
{
    public PlayerActionState CurrentAction {get; private set; }

    public bool IsAttacking => CurrentAction == PlayerActionState.NormalAttack;
    
    public bool IsGrounded { get; private set; }
    public bool IsJumping { get; private set; }

    public void SetGrounded(bool value)
    {
        IsGrounded = value;

        if (value)
            IsJumping = false;
    }

    public void StartJump()
    {
        IsJumping = true;
        IsGrounded = false;
    }
}
