using UnityEngine;
using UnityEngine.Pool;

public class NormalAttackHitboxPool : MonoBehaviour
{
    [Header("풀링할 객체")]
    [SerializeField] private NormalAttackHitbox normalAttackHitbox; //풀링할 대상
    
    private ObjectPool<NormalAttackHitbox> pool;

    private void Awake()
    {
        pool = new ObjectPool<NormalAttackHitbox>(
            CreateAttack,
            OnGetAttack,
            OnReleaseAttack,
            OnDestroyAttack
        );
    }

    public NormalAttackHitbox Get()
    {
        return pool.Get();
    }

    public void Release(NormalAttackHitbox normalAttack)
    {
        pool.Release(normalAttack);
    }

    private NormalAttackHitbox CreateAttack()
    {
        NormalAttackHitbox attack = Instantiate(normalAttackHitbox, gameObject.transform);
        attack.SetPool(pool);

        return attack;
    }

    private void OnGetAttack(NormalAttackHitbox normalAttack)
    {
        normalAttack.gameObject.SetActive(true);
    }

    private void OnReleaseAttack(NormalAttackHitbox normalAttack)
    {
        normalAttack.gameObject.SetActive(false);
    }

    private void OnDestroyAttack(NormalAttackHitbox normalAttack)
    {
        Destroy(normalAttack.gameObject);
    }
}