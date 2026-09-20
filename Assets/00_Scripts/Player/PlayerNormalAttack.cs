using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
public class PlayerNormalAttack : MonoBehaviour
{
    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 1.5f;

    private InputManager inputManager;
    private NormalAttackPool normalAttackPool;

    private PlayerState playerState;
    private SpriteRenderer spriteRenderer;

    private bool upInput;

    [Inject]
    public void Construct(
        InputManager inputManager,
        NormalAttackPool normalAttackPool)
    {
        this.inputManager = inputManager;
        this.normalAttackPool = normalAttackPool;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        inputManager.OnNormalAttack
            .Where(_ => !playerState.IsActionLocked())
            .Subscribe(_ => Attack())
            .AddTo(this);
        
        inputManager.UpInput
            .Subscribe(upInput => this.upInput = upInput)
            .AddTo(this);
    }

    private void Attack()
    {
        playerState.StartNormalAttack();

        if (upInput) HighAttack();
        else HorizontalAttack();
    }

    private void HorizontalAttack()
    {
        NormalAttack attack = normalAttackPool.Get();
        
        float x = spriteRenderer.flipX ? transform.position.x - offset : transform.position.x + offset;

        attack.transform.position = new Vector3(x, transform.position.y, transform.position.z);
        attack.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;

        TestEndAttack().Forget();
    }

    private void HighAttack()
    {
        Debug.Log("상단 공격 수행");
    }

    //테스트 전용 함수
    //캐릭터 공격 애니메이션을 받으면 마지막 프레임에 EndAttack함수를 실행시키게 할 것
    private async UniTask TestEndAttack()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.6f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());
        
        playerState.StopNormalAttack();
    }

    public void EndAttack()
    {
        playerState.StopNormalAttack();
    }
}
