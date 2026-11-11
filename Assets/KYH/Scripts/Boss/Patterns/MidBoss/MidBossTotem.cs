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

    [Header("토템 애니메이션")]
    [SerializeField] private Animator totemAnimator;
    [SerializeField] private AnimationClip activeClip;

    [Header("토템 방향")]
    [SerializeField] private SpriteRenderer totemRenderer;

    private float timeRemaining;
    private bool hasEnded = true;
    private bool isPreparing;
    private bool isLifeTimeActive;
    private bool isEnding;

    public bool HasEnded => hasEnded;
    public event Action Ended;

    private readonly int ActiveState = Animator.StringToHash("Base Layer.Totem_Active");
    private readonly int LoopState = Animator.StringToHash("Base Layer.Totem_Loop");
    private readonly int DeadState = Animator.StringToHash("Base Layer.Totem_Dead");

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
        if(health != null)
            health.HealthChanged += HandleHealthChanged;
    }

    public bool PrepareAppearance(float duration)
    {
        if (!isActiveAndEnabled || !hasEnded || isEnding)
            return false;

        if (health == null ||
            shooter == null ||
            totemAnimator == null ||
            activeClip == null ||
            !totemAnimator.isActiveAndEnabled ||
            !totemAnimator.HasState(0, ActiveState) ||
            !totemAnimator.HasState(0, LoopState) ||
            !totemAnimator.HasState(0, DeadState))
        {
            Debug.LogError("토템 체력, 발사기와 애니메이션 연결을 확인해 주세요.", this);
            return false;
        }

        hasEnded = false;
        isPreparing = true;
        isLifeTimeActive = false;
        timeRemaining = 0f;

        health.SetInvincible(true);

        if (!health.ResetHealth())
        {
            EndTotem();
            return false;
        }

        float prepareDuration = Mathf.Max(0.01f, duration);

        totemAnimator.speed = activeClip.length / prepareDuration;
        totemAnimator.Play(ActiveState, 0, 0f);

        return true;
    }

    public bool ActiveCombat()
    {
        if (!isActiveAndEnabled || hasEnded || isEnding ||!isPreparing)
            return false;

        isPreparing = false;
        isLifeTimeActive = true;
        timeRemaining = Mathf.Max(0.01f, lifetime);

        health.SetInvincible(false);

        totemAnimator.speed = 1f;
        totemAnimator.Play(LoopState, 0, 0f);

        if (!shooter.StartShooting())
        {
            EndTotem();
            return false;
        }

        return true;
    }

    private void Update()
    {
        if(hasEnded || !isLifeTimeActive)
            return;

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

            if(timeRemaining <= 0f)
                EndTotem();
    } 

    private void OnDisable()
    {
        if(health != null)
            health.HealthChanged -= HandleHealthChanged;

        if(!hasEnded)
            FinishDisappearance();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0)
            EndTotem();
    }

    public void EndTotem()
    {
        if(hasEnded || isEnding)
            return;

        isEnding = true;
        isPreparing = false;
        isLifeTimeActive = false;
        timeRemaining = 0f;

        if(shooter != null)
            shooter.StopShooting();
        if(health != null)
            health.SetInvincible(true);

        if (!isActiveAndEnabled ||
            totemAnimator == null ||
            !totemAnimator.isActiveAndEnabled ||
            !totemAnimator.HasState(0, DeadState))
            {
                FinishDisappearance();
                return;
            }

        totemAnimator.speed = 1f;
        totemAnimator.Play(DeadState, 0, 0f);
    }

    public void OnDisappearanceFinished()
    {
        if(!isEnding)
            return;

        FinishDisappearance();
    }

    private void FinishDisappearance()
    {
        if(hasEnded)
            return;

        hasEnded = true;
        isEnding = false;
        isPreparing = false;
        isLifeTimeActive = false;
        timeRemaining = 0f;

        if (shooter != null)
            shooter.StopShooting();

        if (health != null)
            health.SetInvincible(false);

        if (totemAnimator != null)
            totemAnimator.speed = 1f;

        gameObject.SetActive(false);
        Ended?.Invoke();
    }

    public void SetFacing(bool isRightSpawn)
    {
        if (totemRenderer == null)
        {
            Debug.LogError("토템의 SpriteRenderer를 연결해 주세요.", this);
            return;
        }

        totemRenderer.flipX = isRightSpawn;
    }
}
