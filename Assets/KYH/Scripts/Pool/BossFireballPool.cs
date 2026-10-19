using UnityEngine;
using System.Collections.Generic;

public class BossFireballPool : MonoBehaviour
{
    private GameObject testFireball;

    [Header("파이어볼 프리팹")]
    [SerializeField] private GameObject fireballPrefab;

    [Header("초기 생성 개수")]
    [SerializeField, Min(1)] private int initialCount = 10;

    private readonly Queue<GameObject> availableFireballs = new Queue<GameObject>();
    private readonly HashSet<GameObject> rentedFireballs = new HashSet<GameObject>(); 

    private void Awake()
    {
        if(fireballPrefab == null)
        {
            Debug.LogError("Fire ball Prefab을 연결해 주세요", this);
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
        GameObject fireball = Instantiate(fireballPrefab, transform);

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
        projectile.launch();

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
