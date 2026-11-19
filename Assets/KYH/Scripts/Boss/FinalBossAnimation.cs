using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FinalBossAnimation : MonoBehaviour
{
    [SerializeField] private BossApproach approach;

    private Animator animator;

    private readonly int comboState = Animator.StringToHash("Base Layer.ComboPattern.FinalBoss_Combo");

    public bool CanPlayCombo => isActiveAndEnabled && animator != null && animator.isActiveAndEnabled && animator.HasState(0, comboState);

    public bool HasComboClipEnded
    {
        get
        {
            if (!CanPlayCombo)
                return false;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            return state.fullPathHash == comboState && state.normalizedTime >= 1f;
        }
    }
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

    public void StartComboAnim()
    {
        if (!animator.isActiveAndEnabled)
            return;

        animator.ResetTrigger("FinishCombo");
        animator.ResetTrigger("CancelCombo");
        animator.ResetTrigger("StartCombo");
        animator.SetTrigger("StartCombo");
    }

    public void FinishComboAnim()
    {
        if (!animator.isActiveAndEnabled)
            return;

        animator.ResetTrigger("StartCombo");
        animator.ResetTrigger("CancelCombo");
        animator.SetTrigger("FinishCombo");
    }

    public void CancelComboAnim()
    {
        animator.ResetTrigger("StartCombo");
        animator.ResetTrigger("FinishCombo");

        if (!animator.isActiveAndEnabled)
            return;

        animator.SetTrigger("CancelCombo");
    }

    public void StartDashAnim()
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        animator.ResetTrigger("FinishDash");
        animator.ResetTrigger("CancelDash");
        animator.ResetTrigger("StartDashWindup");
        animator.SetTrigger("StartDashWindup");
    }

    public void FinishDashAnim()
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return;
        
        animator.ResetTrigger("StartDashWindup");
        animator.ResetTrigger("CancelDash");
        animator.SetTrigger("FinishDash");
    }

    public void CancelDashAnim()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("StartDashWindup");
        animator.ResetTrigger("FinishDash");
        
        if (!animator.isActiveAndEnabled)
            return;

        animator.SetTrigger("CancelDash");
    }
}
