using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
public class PlayerSpriteFlip : MonoBehaviour
{
    private InputManager inputManager;

    private SpriteRenderer spriteRenderer;
    private PlayerState playerState;

    private float moveInput;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        inputManager.MoveInput
            .Where(input => input != 0)
            .Subscribe(input => moveInput = input)
            .AddTo(this);
    }

    private void FixedUpdate()
    {
        if (playerState.IsActionLocked())
            return;
        
        if (moveInput != 0f)
            OnFlip(moveInput);
    }

    private void OnFlip(float input)
    {
        if (input < 0) spriteRenderer.flipX = true;
        else if (input > 0) spriteRenderer.flipX = false;
    }
}
