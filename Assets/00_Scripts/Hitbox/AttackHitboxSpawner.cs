using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AttackHitboxSpawner : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 외부에서 받은 풀링을 특정 좌표에 소환하는 함수
    public AttackHitbox Spawn(AttackHitboxPool pool, float offset)
    {
        AttackHitbox attack = pool.Get();

        float x = spriteRenderer.flipX
            ? transform.position.x - offset
            : transform.position.x + offset;

        attack.transform.position =
            new Vector3(x, transform.position.y, transform.position.z);

        attack.SetDirection(spriteRenderer.flipX);

        return attack;
    }
}
