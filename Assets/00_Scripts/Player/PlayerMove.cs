using System;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
public class PlayerMove : MonoBehaviour
{
    [Header("플레이어의 이동 속도")]
    [SerializeField] private float maxSpeed = 8f; //최고 속도
    [SerializeField] private float acceleration = 45f; //목표 속도까지 빨라지는 속도
    [SerializeField] private float deceleration = 35f; //목표 속도까지 느려지는 속도
    [SerializeField] private float turnSpeed = 70f; //반대 방향으로 전환되는 속도

    private InputManager inputManager;

    private Rigidbody2D rigid;
    private PlayerState playerState;

    private float moveInput;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }
    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        inputManager.MoveInput
            .Subscribe(input => moveInput = input)
            .AddTo(this);
    }

    private void FixedUpdate()
    {
        float currentMoveInput = playerState.IsActionLocked() ? 0f : moveInput;
        Move(currentMoveInput);
    }

    private void Move(float input)
    {
        float targetSpeed = input * maxSpeed;
        float speedChangeRate;

        if (input == 0)
        {
            speedChangeRate = deceleration;
        }
        else if (Mathf.Sign(input) != Math.Sign(rigid.linearVelocity.x) 
            && Mathf.Abs(rigid.linearVelocity.x) > 0.01f)
        {
            speedChangeRate = turnSpeed;
        }
        else
        {
            speedChangeRate = acceleration;
        }

        float xSpeed = Mathf.MoveTowards(
            rigid.linearVelocity.x, 
            targetSpeed, 
            speedChangeRate * Time.fixedDeltaTime);

        rigid.linearVelocity = new Vector2(xSpeed, rigid.linearVelocity.y);
    }
}
