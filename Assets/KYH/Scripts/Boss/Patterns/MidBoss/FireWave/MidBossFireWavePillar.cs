using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MidBossFireWavePillar : MonoBehaviour
{
    [SerializeField] private GameObject warningVisual;

    [Header("피해 대상")]
    [SerializeField] private LayerMask damageLayer;

    private readonly List<Collider2D> foundColliders = new();
    private ContactFilter2D damageFilter;
    private Vector2 damageSize;
    private Vector2 damageOffset;
    private bool isDamageActive;

    private Animator animator;
    private SpriteRenderer pillarRenderer;
    private Coroutine warningCoroutine;

    private bool isEffectPlaying;
    private bool hasFinished;

    public event Action<MidBossFireWavePillar> EffectStarted;
    public event Action<MidBossFireWavePillar> Finished;
    public event Action<MidBossFireWavePillar, IDamageable> DamageTargetDetected;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pillarRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isDamageActive = false;
        isEffectPlaying = false;
        hasFinished = false;

        pillarRenderer.enabled = false;

        if (warningVisual != null)
            warningVisual.SetActive(false);
    }

    public void SetWidth(float width)
    {
        if (pillarRenderer.sprite == null)
            return;

        float parentScaleX = transform.parent != null
            ? Mathf.Abs(transform.parent.lossyScale.x)
            : 1f;

        float originalWidth = pillarRenderer.sprite.bounds.size.x;

        if (originalWidth <= 0f || parentScaleX <= 0f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Max(0.1f, width) / (originalWidth * parentScaleX);
        transform.localScale = scale;

        if (warningVisual == null ||
            !warningVisual.TryGetComponent<SpriteRenderer>(out var warningRenderer) ||
            warningRenderer.sprite == null)
            return;

        float warningWidth = warningRenderer.sprite.bounds.size.x;
        float pillarScaleX = Mathf.Abs(transform.lossyScale.x);

        if (warningWidth <= 0f || pillarScaleX <= 0f)
            return;

        Vector3 warningScale = warningVisual.transform.localScale;
        warningScale.x = width / (warningWidth * pillarScaleX);
        warningVisual.transform.localScale = warningScale;
    }

    public void ConfigureDamage(Vector2 size, Vector2 offset)
    {
        damageSize = size;
        damageOffset = offset;

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;
    }

    public void BeginWarning(float duration)
    {
        if (!isActiveAndEnabled || warningVisual == null)
            return;

        Stop();
        
        hasFinished = false;

        animator.Rebind();
        animator.Update(0f);

        warningCoroutine = StartCoroutine(WarningSequence(duration));
    }

    private IEnumerator WarningSequence(float duration)
    {
        warningVisual.SetActive(true);

        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        warningVisual.SetActive(false);
        pillarRenderer.enabled = true;
        isEffectPlaying = true;

        animator.SetTrigger("PlayEffect");
        warningCoroutine = null;

        EffectStarted?.Invoke(this);
    }

    public void EnableDamage()
    {
        if (!isActiveAndEnabled || !isEffectPlaying || hasFinished)
            return;

        isDamageActive = true;
        CheckDamage();
    }

    public void DisableDamage()
    {
        isDamageActive = false;
    }

    private void Update()
    {
        if (isDamageActive)
            CheckDamage();
    }

    private void CheckDamage()
    {
        if (!isDamageActive || !isEffectPlaying)
            return;

        Vector2 center = (Vector2)transform.position + damageOffset;

        foundColliders.Clear();

        Physics2D.OverlapBox(
            center,
            damageSize,
            0f,
            damageFilter,
            foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if (!isDamageActive || !isEffectPlaying)
                return;

            if (other == null ||
                !other.TryGetComponent<IDamageable>(out var target))
                continue;

            DamageTargetDetected?.Invoke(this, target);
        }
    }

    public void OnFinishFrame()
    {
        if (!isActiveAndEnabled || !isEffectPlaying || hasFinished)
            return;

        hasFinished = true;
        isEffectPlaying = false;
        pillarRenderer.enabled = false;
        isDamageActive = false;

        Finished?.Invoke(this);
    }

    public void Stop()
    {
        isDamageActive = false;
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        isEffectPlaying = false;
        pillarRenderer.enabled = false;

        if (warningVisual != null)
            warningVisual.SetActive(false);
    }

    private void OnDisable()
    {
        Stop();
    }
}
