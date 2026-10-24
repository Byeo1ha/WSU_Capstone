using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MidBossFlamePillar : MonoBehaviour
{
    [Header("피해 대상")]
    [SerializeField] private LayerMask damageLayer;
    [Header("공격 설정")]
    [SerializeField] private MidBossFlamePillarData data;

    [Header("임시 표시")]
    [SerializeField] private SpriteRenderer warningVisual;
    [SerializeField] private SpriteRenderer pillarVisual;

    private readonly List<Collider2D> foundColliders = new List<Collider2D>();
    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    private ContactFilter2D damageFilter;
    private bool isDamageActive;
    private Vector2 attackSize;
    private int attackDamage;

    private Coroutine sequence;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;
    public event Action Finished;

    

    private void Awake()
    {
        HideVisuals();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if(!isActiveAndEnabled)
            return;

        Stop();

        if(data == null||
            warningVisual == null||
            pillarVisual == null||
            warningVisual.sprite == null||
            pillarVisual.sprite == null)
        {
            Debug.LogError("공격 설정과 두 표시 이미지를 연결해 주세요.", this);
            return;
        }
        Vector2 size = data.Size;

        attackSize = size;
        attackDamage = data.Damage;

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        SetVisualSize(warningVisual, size.x, 0.1f);
        SetVisualSize(pillarVisual, size.x, size.y);

        warningVisual.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        pillarVisual.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);

        isPlaying = true;
        sequence = StartCoroutine(playSequence());
    }

    private IEnumerator playSequence()
    {
        warningVisual.enabled = true;

        yield return new WaitForSeconds(data.WarningDuration);

        warningVisual.enabled = false;
        pillarVisual.enabled = true;
        isDamageActive = true;

        CheckDamage();

        yield return new WaitForSeconds(data.ActiveDuration);

        isDamageActive = false;
        HideVisuals();

        isPlaying = false;
        sequence = null;

        Finished?.Invoke();
    }

    private void FixedUpdate()
    {
        if(isDamageActive)
            CheckDamage();
    }

    private void CheckDamage()
    {
        Vector2 center = (Vector2)transform.position + Vector2.up * (attackSize.y * 0.5f);

        foundColliders.Clear();

        Physics2D.OverlapBox(center, attackSize, 0f, damageFilter, foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if(!isDamageActive)
                return;

            if(other == null || other.gameObject.layer != LayerMask.NameToLayer("Player"))
                continue;

            if(!other.TryGetComponent<IDamageable>(out var target))
                continue;

            if(!damagedTargets.Add(target))
                continue;

            target.TakeDamage(attackDamage);
        }
    }

    public void Stop()
    {
        if(sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }

        isPlaying = false;
        isDamageActive = false;
        damagedTargets.Clear();
        
        HideVisuals();
        
    }
    
    private void HideVisuals()
    {
        if(warningVisual != null)
            warningVisual.enabled = false;

        if(pillarVisual != null)
            pillarVisual.enabled = false;
    }

    private void SetVisualSize(SpriteRenderer visual, float width, float height)
    {
        Vector2 originalSize = visual.sprite.bounds.size;

        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(width / originalSize.x, height / originalSize.y, 1f);
    }

    [ContextMenu("테스트/화염기둥 실행")]
    private void TestPlay()
    {
        if(Application.isPlaying)
            Play();
    }

    [ContextMenu("테스트/ 화염기둥 중단")]
    private void TestStop()
    {
        if(Application.isPlaying)
            Stop();
    }
}
