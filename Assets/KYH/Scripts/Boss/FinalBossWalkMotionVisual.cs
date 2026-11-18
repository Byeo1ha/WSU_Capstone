using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FinalBossWalkMotionVisual : MonoBehaviour
{
    [SerializeField] private BossApproach approach;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (approach != null)
            return;

        Debug.LogError("보스 접근 컴포넌트를 연결해 주세요.", this);
        enabled = false;
    }

    private void Update()
    {
        if (!animator.isActiveAndEnabled)
            return;

        animator.SetBool("isWalk", approach.IsApproaching);
    }
}
