using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MidBossFireFieldPillar : MonoBehaviour
{
    [SerializeField] private GameObject warningVisual;

    [Header("피해 대상")]
    [SerializeField] private LayerMask damageLayer;

    private readonly List<Collider2D> foundColliders = new();
    private readonly Dictionary<IDamageable, float> nextDamageTimes = new();

    private ContactFilter2D damageFilter;
    private Vector2 damageSize;
    private Vector2 damageOffset;
    private Vector3 groundPosition;
    private int attackDamage;
    private float damageInterval;

    private Animator animator;
    private SpriteRenderer fieldRenderer;
    private Coroutine lifetimeCoroutine;

    private bool isFieldActive;

    public event Action<MidBossFireFieldPillar> Finished;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        fieldRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isFieldActive = false;
        fieldRenderer.enabled = false;

        if (warningVisual != null)
            warningVisual.SetActive(false);
    }

    public bool Begin(MidBossFireFieldData data, Vector3 groundPoint)
    {
        if (!isActiveAndEnabled || data == null || warningVisual == null ||
            !animator.isActiveAndEnabled)
            return false;

        Stop();

        attackDamage = data.Damage;
        damageInterval = Mathf.Max(0.01f, data.DamageInterval);
        damageSize = data.DamageSize;
        damageOffset = data.DamageOffset;
        
        nextDamageTimes.Clear();

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        animator.Rebind();
        animator.Update(0f);

        groundPosition = groundPoint;

        transform.position = groundPosition + Vector3.up * data.VisualOffsetY;

        warningVisual.transform.position = groundPosition;

        lifetimeCoroutine = StartCoroutine(FieldSequence(data));
        return true;
    }

    private IEnumerator FieldSequence(MidBossFireFieldData data)
    {
        warningVisual.SetActive(true);

        if (data.WarningDuration > 0f)
            yield return new WaitForSeconds(data.WarningDuration);

        warningVisual.SetActive(false);
        fieldRenderer.enabled = true;
        isFieldActive = true;

        animator.SetTrigger("PlayField");

        yield return new WaitForSeconds(data.ActiveDuration);

        lifetimeCoroutine = null;
        Stop();

        Finished?.Invoke(this);
    }

    private void Update()
    {
        if (!isFieldActive || Time.timeScale <= 0f)
            return;

        CheckDamage();
    }

    private void CheckDamage()
    {
        Vector2 center = (Vector2)groundPosition + damageOffset;

        foundColliders.Clear();

        Physics2D.OverlapBox(
            center,
            damageSize,
            0f,
            damageFilter,
            foundColliders);

            foreach (Collider2D other in foundColliders)
            {
                if (!isFieldActive || Time.timeScale <= 0f)
                    return;

                if (other == null || !other.TryGetComponent<IDamageable>(out var target))
                    continue;

                if (nextDamageTimes.TryGetValue(target, out float nextTime) && Time.time < nextTime)
                    continue;

                nextDamageTimes[target] = Time.time + damageInterval;
                target.TakeDamage(attackDamage);
            }
    }

    public void Stop()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        isFieldActive = false;
        nextDamageTimes.Clear();
        fieldRenderer.enabled = false;

        if (warningVisual != null)
            warningVisual.SetActive(false);
    }

    private void OnDisable()
    {
        Stop();
    }
}
