using System.Collections.Generic;
using UnityEngine;

public class FinalBossSlash : MonoBehaviour
{
    [Header("칼 공격 설정")]
    [SerializeField] private FinalBossComboData data;

    [Header("공격 기준 위치")]
    [SerializeField] private Transform attackOrigin;

    [Header("피해 대상")]
    [SerializeField] private LayerMask damageLayer;

    private readonly List<Collider2D> foundColliders = new List<Collider2D>();

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    private ContactFilter2D damageFilter;
    private Vector2 attackCenter;
    private Vector2 attackSize;
    private int attackDamage;
    private float endTime;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    public bool BeginSlash(int direction)
    {
        if(!isActiveAndEnabled || isAttacking)
            return false;

        if(data == null || attackOrigin == null)
        {
            Debug.LogError("칼 공격 데이터와 기준 위치를 연결해 주세요.", this);
            return false;
        }

        damagedTargets.Clear();

        int facing = direction < 0 ? -1 : 1;

        Vector2 offset = data.SlashOffset;
        offset.x *= facing;

        attackCenter = (Vector2)attackOrigin.position + offset;
        attackSize = new Vector2(Mathf.Max(0.01f, data.SlashSize.x), Mathf.Max(0.01f, data.SlashSize.y));

        attackDamage = Mathf.Max(0, data.SlashDamage);
        endTime = Time.time + Mathf.Max(0.01f, data.SlashDuration);

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        isAttacking = true;
        CheckDamage();

        return true;
    }

    private void Update()
    {
        if(isAttacking && Time.time >= endTime)
            StopSlash();
    }

    private void FixedUpdate()
    {
        if(!isAttacking)
            return;

        if(Time.time >=endTime)
        {
            StopSlash();
            return;
        }

        CheckDamage();
    }

    private void CheckDamage()
    {
        foundColliders.Clear();

        Physics2D.OverlapBox(attackCenter, attackSize, 0f, damageFilter, foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if(!isAttacking)
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

    public void StopSlash()
    {
        isAttacking = false;
        endTime = 0f;
        damagedTargets.Clear();
    }

    private void OnDisable()
    {
        StopSlash();
    }
    
    private void OnDrawGizmos()
    {
        if(!isAttacking)
            return;

        Vector3 center = new Vector3(attackCenter.x, attackCenter.y, transform.position.z);
        Vector3 size = new Vector3(attackSize.x, attackSize.y, 0.01f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }

    [ContextMenu("테스트/오른쪽 칼 공격")]
    private void TestSlashRight()
    {
        if(Application.isPlaying)
            BeginSlash(1);
    }

    [ContextMenu("테스트/왼쪽 칼 공격")]
    private void TestSlashLeft()
    {
        if(Application.isPlaying)
            BeginSlash(-1);
    }

    [ContextMenu("테스트/칼 공격 중단")]
    private void TestStopSlash()
    {
        if(Application.isPlaying)
            StopSlash();
    }
}
