using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AttackHitboxSpawner : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
