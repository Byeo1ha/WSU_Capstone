using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(SpriteRenderer))]
public class HitEffect : MonoBehaviour
{
    [Header("표시 시간")]
    [SerializeField, Min(0.01f)] private float duration = 0.3f;

    private IObjectPool<HitEffect> pool;
    private float elapsedTime;

    private void OnEnable()
    {
        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= duration && pool != null)
            pool.Release(this);
    }

    public void SetPool(IObjectPool<HitEffect> pool)
    {
        this.pool = pool;
    }

    public void SetDirection(bool value)
    {
        GetComponent<SpriteRenderer>().flipX = value;
    }
}
