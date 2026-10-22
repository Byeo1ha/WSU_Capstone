using UnityEngine;
using System.Collections.Generic;
using System;

public class BossFireballPool : MonoBehaviour
{
    private GameObject testFireball;

    [Header("파이어볼 프리팹")]
    [SerializeField] private GameObject fireballPrefab;
    [Header("파이어볼 보관 위치")]
    [SerializeField] private Transform storageRoot;

    [Header("초기 생성 개수")]
    [SerializeField, Min(1)] private int initialCount = 10;

    private readonly Queue<GameObject> availableFireballs = new Queue<GameObject>();
    private readonly HashSet<GameObject> rentedFireballs = new HashSet<GameObject>(); 

    public event Action<GameObject> FireballReturned;

    private void Awake()
    {
        if(fireballPrefab == null || storageRoot == null)
        {
            Debug.LogError("Fire ball Prefab과 Storage Root을 연결해 주세요", this);
            enabled = false;
            return;
        }

        if (!fireballPrefab.TryGetComponent<BossFireball>(out _))
        {
            Debug.LogError("파잉볼 프리팹의 루트에 BossFireball이 필요합니다.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < initialCount; i++)
        {
            GameObject fireball = CreateFireball();
            availableFireballs.Enqueue(fireball);
        }
    }
    private GameObject CreateFireball()
    {
        GameObject fireball = Instantiate(fireballPrefab, storageRoot);

        fireball.SetActive(false);

        BossFireball projectile = fireball.GetComponent<BossFireball>();
        projectile.Initialize(this);

        return fireball;
    }

    public GameObject Rent(Vector3 position, float lifetime, Vector2 direction, float speed)
    {
        if(!isActiveAndEnabled)
            return null;

        GameObject fireball;

        if(availableFireballs.Count > 0)
            fireball = availableFireballs.Dequeue();
        else
            fireball = CreateFireball();

        rentedFireballs.Add(fireball);

        fireball.transform.position = position;

        BossFireball projectile = fireball.GetComponent<BossFireball>();
        projectile.Prepare(lifetime, direction, speed);

        fireball.SetActive(true);

        return fireball;
    }

    public void Return(GameObject fireball)
    {
        if(fireball == null)
            return;

        if(!rentedFireballs.Remove(fireball))
            return;

        fireball.SetActive(false);
        availableFireballs.Enqueue(fireball);

        FireballReturned?.Invoke(fireball);
    }

    [ContextMenu("테스트/파이어볼 대여")]
    private void TestRent()
    {
        if(!Application.isPlaying || testFireball != null)
            return;

        testFireball = Rent(transform.position, 3f, Vector2.right, 5f);
    }

    [ContextMenu("테스트/ 파이어볼 반환")]
    private void TestReturn()
    {
        if(!Application.isPlaying || testFireball == null)
            return;

        Return(testFireball);
        testFireball = null;
    }
    
}
