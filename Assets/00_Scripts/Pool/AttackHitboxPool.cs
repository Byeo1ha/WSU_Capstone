using UnityEngine;
using UnityEngine.Pool;

public class AttackHitboxPool : MonoBehaviour
{
    [Header("풀링할 객체")]
    [SerializeField] private AttackHitbox attackHitbox; //풀링할 대상
    
    private ObjectPool<AttackHitbox> pool;

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
        AttackHitbox attack = Instantiate(attackHitbox, gameObject.transform);
        attack.SetPool(pool);

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