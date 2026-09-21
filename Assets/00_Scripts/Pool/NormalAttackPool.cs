using UnityEngine;
using UnityEngine.Pool;

public class NormalAttackPool : MonoBehaviour
{
    [Header("기본 공격 Prefabs")]
    [SerializeField] private NormalAttack[] normalAttackPrefab; //풀링할 대상

    private ObjectPool<NormalAttack>[] pool;

    public int ComboCount => normalAttackPrefab == null ? 0 : normalAttackPrefab.Length;

    private void Awake()
    {
        pool = new ObjectPool<NormalAttack>[normalAttackPrefab.Length];

        for (int i = 0; i < normalAttackPrefab.Length; i++)
        {
            int index = i;

            pool[index] = new ObjectPool<NormalAttack>(
                () => CreateAttack(index),
                OnGetAttack,
                OnReleaseAttack,
                OnDestroyAttack
            );
        }
    }

    public NormalAttack Get(int index)
    {
        if (index < 0 || index >= pool.Length)
        {
            Debug.LogWarning($"존재하지 않는 기본 공격 인덱스. / 인덱스 번호 : {index}");
            return null;
        }

        if (normalAttackPrefab[index] == null)
        {
            Debug.LogWarning($"{index}번 기본 공격 Prefab이 지정되지 않음.");
            return null;
        }

        return pool[index].Get();
    }

    private NormalAttack CreateAttack(int index)
    {
        NormalAttack attack = Instantiate(normalAttackPrefab[index], gameObject.transform);
        attack.SetPool(pool[index]);

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