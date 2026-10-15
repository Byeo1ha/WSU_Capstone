using UnityEngine;

public enum PlayerActionState
{
    None,
    NormalAttack,
    HighAttack,
    AirAttack,
    LowAttack,
    Dodge,
    Damaged
}

public class PlayerState : MonoBehaviour
{
    public PlayerActionState CurrentAction {get; private set; }

    public bool IsNormalAttacking => CurrentAction == PlayerActionState.NormalAttack;
    public bool IsAirAttacking => CurrentAction == PlayerActionState.AirAttack;
    public bool IsLowAttacking => CurrentAction == PlayerActionState.LowAttack;
    public bool IsDodging => CurrentAction == PlayerActionState.Dodge;
    public bool IsDamaging => CurrentAction == PlayerActionState.Damaged;
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

    public void StopAction(PlayerActionState action)
    {
        if (CurrentAction != action) return;

        CurrentAction = PlayerActionState.None;
    }

    public void StartNormalAttack() 
        => TryStartAction(PlayerActionState.NormalAttack);

    public void StartHighAttack()
        => TryStartAction(PlayerActionState.HighAttack);

    public void StartAirAttack()
        => TryStartAction(PlayerActionState.AirAttack);

    public void StartDodge()
        => TryStartAction(PlayerActionState.Dodge);

    public void StartLowAttack()
        => TryStartAction(PlayerActionState.LowAttack);

    public void StartDamaged()
        => TryStartAction(PlayerActionState.Damaged);

}
