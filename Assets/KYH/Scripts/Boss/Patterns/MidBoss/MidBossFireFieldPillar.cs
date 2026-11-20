using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MidBossFireFieldPillar : MonoBehaviour
{
    [SerializeField] private GameObject warningVisual;

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

    public bool Begin(MidBossFireFieldData data)
    {
        if (!isActiveAndEnabled || data == null || warningVisual == null ||
            !animator.isActiveAndEnabled)
            return false;

        Stop();

        animator.Rebind();
        animator.Update(0f);

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

    public void Stop()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        isFieldActive = false;
        fieldRenderer.enabled = false;

        if (warningVisual != null)
            warningVisual.SetActive(false);
    }

    private void OnDisable()
    {
        Stop();
    }
}
