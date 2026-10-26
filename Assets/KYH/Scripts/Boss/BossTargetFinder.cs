using UnityEngine;

public class BossTargetFinder : MonoBehaviour
{
    [Header("대상 탐색")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField, Min(0.1f)] private float targetSearchRadius = 30f;

    public bool TryGetTargetPosition(out Vector2 targetPosition)
    {
        targetPosition = Vector2.zero;

        if(!isActiveAndEnabled)
            return false;

        Collider2D found = Physics2D.OverlapCircle(transform.position, targetSearchRadius, targetLayer);

        if(found == null)
            return false;

        targetPosition = found.bounds.center;
        return true;
    }

    [ContextMenu("테스트/대상 위치 확인")]
    private void TestFindTarget()
    {
        if(!Application.isPlaying)
            return;

        if(TryGetTargetPosition(out Vector2 position))
            Debug.Log($"대상 위치: {position}", this);
        else
            Debug.Log("탐색 범위에 대상이 없습니다.", this);
    }
}
