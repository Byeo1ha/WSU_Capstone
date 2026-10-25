using UnityEngine;

[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerAirAttack))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(HitFlashEffect))]
public class PlayerAttributes : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerAttributeData data;
    [SerializeField] private float upOffset = 5f;
    [SerializeField] private float backOffset = 5f;
    [SerializeField, Min(0.01f)] private float damageRecoveryTimeout = 0.5f;

    private PlayerAnimation playerAnimation;
    private PlayerAirAttack playerAirAttack;
    private PlayerState playerState;
    private HitFlashEffect hitFlashEffect;

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    private int hp;
    private float damageRecoveryTime;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerAirAttack = GetComponent<PlayerAirAttack>();
        playerState = GetComponent<PlayerState>();
        hitFlashEffect = GetComponent<HitFlashEffect>();

        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        hp = data.Hp;
    }

    private void Update()
    {
        // 애니메이션이 중단되어 종료 이벤트가 누락되어도 이동 제한을 해제한다.
        if (playerState.IsDamaging && Time.time >= damageRecoveryTime)
            StopDamageEvent();
    }

    private void OnDisable()
    {
        StopDamageEvent();
    }

    // 외부에서 플레이어에게 피해를 입히기 위한 함수
    public void TakeDamage(int damage)
    {
        if (hp < damage) 
        {
            hp = 0;
            return;
        }

        hp -= damage;
        Debug.Log($"{damage} 피해를 입었습니다! 남은 체력 {hp}");

        playerAirAttack.CancelAirAttack();
        playerState.StartDamaged();
        damageRecoveryTime = Time.time + damageRecoveryTimeout;

        rigid.linearVelocity = Vector2.zero;

        int direct;

        if (spriteRenderer.flipX) direct = 1;
        else direct = -1;

        rigid.AddForce(Vector2.left * direct * upOffset, ForceMode2D.Impulse);
        rigid.AddForce(Vector2.up * backOffset, ForceMode2D.Impulse);

        hitFlashEffect.PlayHitFlash();

        playerAnimation.StartDamagedAnim();
    }

    public void StopDamageEvent()
    {
        playerState.StopAction(PlayerActionState.Damaged);
    }
}
