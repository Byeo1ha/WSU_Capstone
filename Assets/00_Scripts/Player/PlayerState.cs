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

    // 플레이어의 지면 상태를 설정하는 함수
    public void SetGrounded(bool value)
    {
        IsGrounded = value;

        if (value)
            IsJumping = false;
    }

    // 점프 중임을 설정하는 함수
    public void StartJump()
    {
        IsJumping = true;
        IsGrounded = false;
    }

    // 외부에서 현재 다른 액션 상태 중인지 체크하는 함수
    public bool IsActionLocked()
    {
        return CurrentAction != PlayerActionState.None;
    }

    // 특정 액션 상태로 설정해주는 함수
    private void TryStartAction(PlayerActionState nextAction)
    {
        if (CurrentAction != PlayerActionState.None)
            return;

        CurrentAction = nextAction;
    }

    // 특정 액션 상태임을 확인하고 액션 상태를 종료하는 함수
    public void StopAction(PlayerActionState action)
    {
        if (CurrentAction != action) return;

        CurrentAction = PlayerActionState.None;
    }

    /// <summary>
    /// 액션 상태를 설정하는 함수
    /// </summary>
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
