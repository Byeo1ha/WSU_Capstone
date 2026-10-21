using R3;
using UnityEngine;

public class TutorialDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private bool timeCheck;

    private Subject<Unit> onDamaged = new Subject<Unit>();
    public Observable<Unit> OnDamaged => onDamaged;

    private HitFlashEffect hitFlashEffect;
    private int totalDamage;
    private float timer;
    private bool timerAlert;

    private void Awake()
    {
        hitFlashEffect = GetComponent<HitFlashEffect>();
        totalDamage = 0;
        if (timeCheck) Debug.LogWarning("시간 체크 기능이 활성화 되어있습니다.");
    }

    private void Update()
    {
        if (timeCheck && !timerAlert)
        {
            timer += Time.deltaTime;

            if (timer > 10f)
            {
                Debug.LogWarning("10초가 지났습니다.");
                timerAlert = true;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        totalDamage += damage;
        Debug.Log($"총 누적 피해량 : {totalDamage} || 받은 피해 : {damage}");
        hitFlashEffect.PlayHitFlash();
        onDamaged.OnNext(Unit.Default);
    }
}
