using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

public class AttackHitboxPool : MonoBehaviour
{
    [Header("풀링할 객체")]
    [SerializeField] private AttackHitbox attackHitbox; //풀링할 대상

    [Header("피격 이펙트 (필요한 공격에만 연결)")]
    [SerializeField] private HitEffectPool hitEffectPool;
    
    private ObjectPool<AttackHitbox> pool;
    private IObjectResolver resolver;

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        this.resolver = resolver;
    }

    private void Awake()
    {
        pool = new ObjectPool<AttackHitbox>(
            CreateAttack,
            OnGetAttack,
            OnReleaseAttack,
            OnDestroyAttack
        );
    }

    public AttackHitbox Get()
    {
        return pool.Get();
    }

    public void Release(AttackHitbox normalAttack)
    {
        pool.Release(normalAttack);
    }

    private AttackHitbox CreateAttack()
    {
        AttackHitbox attack = resolver.Instantiate(attackHitbox, gameObject.transform);
        attack.SetPool(pool);
        attack.SetHitEffectPool(hitEffectPool);

        return attack;
    }

    private void OnGetAttack(AttackHitbox normalAttack)
    {
        normalAttack.gameObject.SetActive(true);
    }

    private void OnReleaseAttack(AttackHitbox normalAttack)
    {
        normalAttack.gameObject.SetActive(false);
    }

    private void OnDestroyAttack(AttackHitbox normalAttack)
    {
        Destroy(normalAttack.gameObject);
    }
}
