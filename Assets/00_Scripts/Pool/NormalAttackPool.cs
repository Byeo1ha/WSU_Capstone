using UnityEngine;
using UnityEngine.Pool;

public class NormalAttackPool : MonoBehaviour
{
    [Header("풀링할 객체")]
    [SerializeField] private NormalAttack normalAttackPrefab; //풀링할 대상
    
    private ObjectPool<NormalAttack> pool;

    private void Awake()
    {
        pool = new ObjectPool<NormalAttack>(
            CreateAttack,
            OnGetAttack,
            OnReleaseAttack,
            OnDestroyAttack
        );
    }

    public NormalAttack Get()
    {
        return pool.Get();
    }

    public void Release(NormalAttack normalAttack)
    {
        pool.Release(normalAttack);
    }

    private NormalAttack CreateAttack()
    {
        NormalAttack attack = Instantiate(normalAttackPrefab, gameObject.transform);
        attack.SetPool(pool);

        return attack;
    }

    private void OnGetAttack(NormalAttack normalAttack)
    {
        normalAttack.gameObject.SetActive(true);
    }

    private void OnReleaseAttack(NormalAttack normalAttack)
    {
        normalAttack.gameObject.SetActive(false);
    }

    private void OnDestroyAttack(NormalAttack normalAttack)
    {
        Destroy(normalAttack.gameObject);
    }
}
