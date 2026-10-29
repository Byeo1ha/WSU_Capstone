using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Subject<Unit> onJump = new();
    public Observable<Unit> OnJump => onJump;

    private Subject<Unit> onNormalAttack = new();
    public Observable<Unit> OnNormalAttack => onNormalAttack;

    private Subject<Unit> onDodge = new();
    public Observable<Unit> OnDodge => onDodge;

    private Subject<Unit> onInteract = new();
    public Observable<Unit> OnInteract => onInteract;

    private ReactiveProperty<float> moveInput = new(0f);
    public ReadOnlyReactiveProperty<float> MoveInput => moveInput;

    private ReactiveProperty<bool> upInput = new(false);
    public ReadOnlyReactiveProperty<bool> UpInput => upInput;

    private ReactiveProperty<bool> downInput = new(false);
    public ReadOnlyReactiveProperty<bool> DownInput => downInput;

    private bool isGameplayInputBlocked;
    public bool IsGameplayInputBlocked => isGameplayInputBlocked;

    public void SetGameplayInputBlocked(bool blocked)
    {
        isGameplayInputBlocked = blocked;

        if (blocked)
        {
            moveInput.Value = 0f;
            upInput.Value = false;
            downInput.Value = false;
        }
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        moveInput.Value = context.ReadValue<float>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        if (context.performed) onJump?.OnNext(Unit.Default);
    }

    public void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        if (context.performed) onDodge?.OnNext(Unit.Default);
    }

    public void OnNormalAttackInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        if (context.performed) onNormalAttack?.OnNext(Unit.Default);
    }
    
    public void OnUpInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        if (context.started) upInput.Value = true;
        else if (context.canceled) upInput.Value = false;
    }

    public void OnDownInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;
        
        if (context.started) downInput.Value = true;
        else if (context.canceled) downInput.Value = false;
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (isGameplayInputBlocked) return;

        if (context.performed) onInteract.OnNext(Unit.Default);
    }
}
