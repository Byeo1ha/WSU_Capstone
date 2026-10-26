using System;
using UnityEngine;


public class MidBossTotem : MonoBehaviour
{
    [Header("토템 체력")]
    [SerializeField] private BossHealth health;

    [Header("유지 시간")]
    [SerializeField, Min(0.01f)] private float lifetime = 4f;
    
    [Header("토템 발사기")]
    [SerializeField] private MidBossTotemShooter shooter;

    private float timeRemaining;
    private bool hasStarted;
    private bool hasEnded = true;

    public bool HasEnded => hasEnded;
    public event Action Ended;

    private void Awake()
    {
        if(health == null || shooter == null)
        {
            Debug.LogError("토템의 체력 컴포넌트를 연결해 주세요.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if(health == null)
            return;

        health.HealthChanged += HandleHealthChanged;

        if(hasStarted)
            BeginLife();
    }

    private void Start()
    {
        hasStarted = true;
        BeginLife();
    }

    private void BeginLife()
    {
        hasEnded = false;
        timeRemaining = Mathf.Max(0.01f, lifetime);

        if(!health.ResetHealth())
        {
            EndTotem();
            return;
        }

        if(!shooter.StartShooting())
            EndTotem();
    }

    private void Update()
    {
        if(hasEnded)
            return;

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

            if(timeRemaining <= 0f)
                EndTotem();
    } 

    private void OnDisable()
    {
        if(health != null)
            health.HealthChanged -= HandleHealthChanged;

        if(hasStarted && !hasEnded)
            EndTotem();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0)
            EndTotem();
    }

    public void EndTotem()
    {
        if(hasEnded)
            return;

        hasEnded = true;
        timeRemaining = 0f;

        if(shooter != null)
            shooter.StopShooting();

        gameObject.SetActive(false);
        Ended?.Invoke();
    }
}
