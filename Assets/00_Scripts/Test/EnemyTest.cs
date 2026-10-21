using UnityEngine;

[RequireComponent(typeof(HitFlashEffect))]
public class EnemyTest : MonoBehaviour, IDamageable
{
    //테스트 전용 Enemy이므로 밸런싱 요소를 위한 변수 및 함수들의 모임
    [SerializeField] private bool timeCheck;
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
    }
}
