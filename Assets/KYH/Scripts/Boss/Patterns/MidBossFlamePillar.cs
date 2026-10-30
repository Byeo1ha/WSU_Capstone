using System;
using System.Collections.Generic;
using UnityEngine;

public class MidBossFlamePillar : MonoBehaviour
{
    [Header("피해 설정")]
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private MidBossFlamePillarData data;

    [Header("같은 오브젝트의 표시와 Animator")]
    [SerializeField] private SpriteRenderer effectVisual;
    [SerializeField] private Animator effectAnimator;
    [SerializeField] private AnimationClip effectClip;

    private readonly int effectState =
        Animator.StringToHash("Base Layer.Effect");

    private readonly List<Collider2D> foundColliders =
        new List<Collider2D>();

    private readonly HashSet<IDamageable> damagedTargets =
        new HashSet<IDamageable>();

    private ContactFilter2D damageFilter;
    private Vector2 attackSize;
    private int attackDamage;

    private bool isPlaying;
    private bool isWarning;
    private bool isActive;
    private bool isDamageActive;

    public bool IsPlaying => isPlaying;
    public event Action Finished;

    private void Awake()
    {
        Stop();
    }

    private void OnDisable()
    {
        Stop();
    }

    public bool PrepareWarning(float duration)
    {
        Stop();

        if (!isActiveAndEnabled ||
            data == null ||
            effectVisual == null ||
            effectAnimator == null ||
            effectClip == null)
        {
            Debug.LogError(
                "화염기둥 데이터와 표시·Animator·클립을 연결해 주세요.",
                this);

            return false;
        }

        if (effectVisual.gameObject != gameObject ||
            effectAnimator.gameObject != gameObject ||
            !effectAnimator.isActiveAndEnabled ||
            !effectAnimator.HasState(0, effectState))
        {
            Debug.LogError(
                "같은 오브젝트에 표시와 Animator를 두고 " +
                "Base Layer의 Effect 상태를 확인해 주세요.",
                this);

            return false;
        }

        attackSize = new Vector2(Mathf.Max(0.01f, data.Size.x), Mathf.Max(0.01f, data.Size.y));

        attackDamage = Mathf.Max(0, data.Damage);

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        isPlaying = true;
        isWarning = true;
        effectVisual.enabled = true;

        effectAnimator.speed = 0.3f / Mathf.Max(0.001f, duration);
        effectAnimator.Play(effectState, 0, 0f);
        effectAnimator.Update(0f);

        return true;
    }

    public bool Activate()
    {
        if (!isActiveAndEnabled || !isPlaying || !isWarning)
            return false;

        isWarning = false;
        isActive = true;

        effectAnimator.speed =
            0.5f / Mathf.Max(0.01f, data.ActiveDuration);

        effectAnimator.Play(
            effectState,
            0,
            0.3f / Mathf.Max(0.001f, effectClip.length));

        effectAnimator.Update(0f);

        EnableDamage();

        return true;
    }

    public void HoldWarning()
    {
        if (isPlaying && isWarning)
            effectAnimator.speed = 0f;
    }

    public void EnableDamage()
    {
        if (!isPlaying || !isActive || isDamageActive)
            return;

        isDamageActive = true;
        CheckDamage();
    }

    public void DisableDamage()
    {
        isDamageActive = false;
    }

    public void FinishEffect()
    {
        if (!isPlaying || !isActive)
            return;

        Stop();
        Finished?.Invoke();
    }

    private void FixedUpdate()
    {
        if (isDamageActive)
            CheckDamage();
    }

    private void CheckDamage()
    {
        if (!isDamageActive)
            return;

        Vector2 center = (Vector2)transform.position + Vector2.up * (attackSize.y * 0.5f);

        foundColliders.Clear();

        Physics2D.OverlapBox(
            center,
            attackSize,
            0f,
            damageFilter,
            foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if (other == null ||
                other.gameObject.layer !=
                LayerMask.NameToLayer("Player"))
                continue;

            if (!other.TryGetComponent<IDamageable>(out var target))
                continue;

            if (!damagedTargets.Add(target))
                continue;

            target.TakeDamage(attackDamage);

            if (!isDamageActive)
                return;
        }
    }

    public void Stop()
    {
        isPlaying = false;
        isWarning = false;
        isActive = false;
        isDamageActive = false;

        damagedTargets.Clear();

        if (effectAnimator != null)
            effectAnimator.speed = 0f;

        if (effectVisual != null)
            effectVisual.enabled = false;
    }
}