using UnityEngine;

public enum PlayerActionState
{
    None,
    NormalAttack,
    HighAttack,
    AirAttack,
    DownAttack,
    Dodge
}

public class PlayerState : MonoBehaviour
{
    public PlayerActionState CurrentAction {get; private set; }

    public bool IsNormalAttacking => CurrentAction == PlayerActionState.NormalAttack;
    public bool IsAirAttacking => CurrentAction == PlayerActionState.AirAttack;
    public bool IsDownAttacking => CurrentAction == PlayerActionState.DownAttack;
    public bool IsDodging => CurrentAction == PlayerActionState.Dodge;
    public bool IsAttacking => IsNormalAttacking || IsAirAttacking;
    
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

    private void TryStartAction(PlayerActionState nextAction)
    {
        if (CurrentAction != PlayerActionState.None)
            return;

        CurrentAction = nextAction;
    }

    private void TryStopAction(PlayerActionState action)
    {
        if (CurrentAction != action)
            return;
        
        CurrentAction = PlayerActionState.None;
    }

    public void StartNormalAttack() 
        => TryStartAction(PlayerActionState.NormalAttack);

    public void StopNormalAttack()
        => TryStopAction(PlayerActionState.NormalAttack);

    public void StartHighAttack()
        => TryStartAction(PlayerActionState.HighAttack);

    public void StopHighAttack()
        => TryStopAction(PlayerActionState.HighAttack);

    public void StartAirAttack()
        => TryStartAction(PlayerActionState.AirAttack);

    public void StopAirAttack()
        => TryStopAction(PlayerActionState.AirAttack);

    public void StartDodge()
        => TryStartAction(PlayerActionState.Dodge);

    public void StopDodge()
        => TryStopAction(PlayerActionState.Dodge);

    public void StartDownAttack()
        => TryStartAction(PlayerActionState.DownAttack);

    public void StopDownAttack()
        => TryStopAction(PlayerActionState.DownAttack);
}
