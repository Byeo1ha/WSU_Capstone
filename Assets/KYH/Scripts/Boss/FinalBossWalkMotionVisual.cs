using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FinalBossWalkMotionVisual : MonoBehaviour
{
    [SerializeField] private BossApproach approach;

    private Animator animator;
    private bool wasApproaching;

    private readonly int idleState = Animator.StringToHash("Base Layer.FinalBoss_Idle");

    private readonly int walkState = Animator.StringToHash("Base Layer.FinalBoss_Walk");

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (approach != null && animator.HasState(0, idleState) && animator.HasState(0, walkState))
            return;

        Debug.LogError("보스 접근 컴포넌트와 대기,걷기 상태를 확인해 주세요.", this);
        enabled = false;
    }

    private void LateUpdate()
    {
        bool isApproaching = approach.IsApproaching;

        if (isApproaching == wasApproaching)
            return;

        wasApproaching = isApproaching;

        if (isApproaching)
        {
            animator.Play(walkState, 0, 0f);
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == walkState)
            animator.Play(idleState, 0, 0f);
    }
}
