using UnityEngine;

public class FinalBossPhaseController : MonoBehaviour
{
    private const float Phase2HealthRatio = 0.75f;
    private const float Phase3HealthRatio = 0.35f;

    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;
    [Header("현재 페이즈")]
    [SerializeField] private FinalBossPhase currentPhase = FinalBossPhase.Phase1;

    public FinalBossPhase CurrentPhase => currentPhase;

    public bool IsTransforming => isTransforming;

    [Header("변신 설정")]
    [SerializeField, Min(0f)] private float transformDuration = 1.4f;

    private bool isTransforming;

    private float transformTimeRemaining;

    private void Awake()
    {
        currentPhase = FinalBossPhase.Phase1;
    }
    
    private void Start()
    {
        int minimumHealth = Mathf.CeilToInt(bossHealth.MaxHealth * Phase2HealthRatio);

        bossHealth.SetMinimumHealth(minimumHealth);
    }

    private void Update()
    {
        if(!isTransforming)
            return;

        transformTimeRemaining -= Time.deltaTime;

        if(transformTimeRemaining <= 0f)
        {
            EndTransformation();
        }
    }

    private void OnEnable()
    {
        bossHealth.HealthChanged += HandleHealthChanged;        
    }

    private  void OnDisable()
    {
        bossHealth.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        int phase2Health = Mathf.CeilToInt(maxHealth * Phase2HealthRatio);
        int phase3Health = Mathf.CeilToInt(maxHealth * Phase3HealthRatio);

        if (currentPhase == FinalBossPhase.Phase1 && currentHealth <= phase2Health)
        {
            ChangePhase(FinalBossPhase.Phase2);
        }
        else if (currentPhase == FinalBossPhase.Phase2 && currentHealth <= phase3Health)
        {
            ChangePhase(FinalBossPhase.Phase3);
        }
    }

    private void ChangePhase(FinalBossPhase nextPhase)
    {
        if (isTransforming)
            return;

        bool canChange =
        (currentPhase == FinalBossPhase.Phase1 && nextPhase == FinalBossPhase.Phase2) ||
        (currentPhase == FinalBossPhase.Phase2 && nextPhase == FinalBossPhase.Phase3);

        if(!canChange)
            return;

        currentPhase = nextPhase;
        BeginTransformation();

        if(currentPhase == FinalBossPhase.Phase2)
        {
            int minimumHealth = Mathf.CeilToInt(bossHealth.MaxHealth * Phase3HealthRatio);

            bossHealth.SetMinimumHealth(minimumHealth);
        }
        else if(currentPhase == FinalBossPhase.Phase3)
        {
            bossHealth.SetMinimumHealth(0);
        }

        Debug.Log($"보스 페이즈 변경: {currentPhase}", this);
    }

    private void BeginTransformation()
    {
        isTransforming = true;
        transformTimeRemaining = transformDuration;
        bossHealth.SetInvincible(true);

        // 변신 애니메이션 시작
        // 애니메이션이 끝나면 EndTransform() 호출
    }

    private void EndTransformation()
    {
        transformTimeRemaining = 0f;
        isTransforming = false;
        bossHealth.SetInvincible(false);
    }

    [ContextMenu("테스트/페이즈 2로 변경")]
    private void TestChangeToPhase2()
    {
        if(!Application.isPlaying)
            return;

        ChangePhase(FinalBossPhase.Phase2);
    }

    [ContextMenu("테스트/페이즈 3로 변경")]
    private void TestChangeToPhase3()
    {
        if(!Application.isPlaying)
            return;

        ChangePhase(FinalBossPhase.Phase3);
    }
}
