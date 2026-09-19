using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Subject<Unit> onJump = new();
    public Observable<Unit> OnJump => onJump;

    private Subject<Unit> onNormalAttack = new();
    public Observable<Unit> OnNormalAttack => onNormalAttack;

    private ReactiveProperty<float> moveInput { get; } = new(0f);
    public ReadOnlyReactiveProperty<float> MoveInput => moveInput;

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput.Value = context.ReadValue<float>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed) onJump?.OnNext(Unit.Default);
    }

    public void OnNormalAttackInput(InputAction.CallbackContext context)
    {
        if (context.performed) onNormalAttack?.OnNext(Unit.Default);
    }
}
