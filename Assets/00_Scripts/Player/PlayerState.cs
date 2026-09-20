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

    public bool IsActionLocked()
    {
        return CurrentAction != PlayerActionState.None;
    }

    public bool StartNormalAttack()
    {
        if(!TryStartAction(PlayerActionState.NormalAttack))
            return false;

        return true;
    }

    public void StopNormalAttack()
    {
        if(!TryStopAction(PlayerActionState.NormalAttack))
            return;
    }

    private bool TryStartAction(PlayerActionState nextAction)
    {
        if (CurrentAction != PlayerActionState.None)
            return false;

        CurrentAction = nextAction;
        return true;
    }

    private bool TryStopAction(PlayerActionState action)
    {
        if (CurrentAction != action)
            return false;
        
        CurrentAction = PlayerActionState.None;
        return true;
    }
}
