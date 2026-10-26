using UnityEngine;

public class MidBossBlinkArea : MonoBehaviour
{
    [Header("보스 중ㅅ힘이 이동할 수 있는 양 끝")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("최소 가로 거리 - 임시값")]
    [SerializeField, Min(0f)] private float MinimumPlayerDistance = 3f;
    [SerializeField, Min(0f)] private float minimumMoveDistance = 2f;

    [Header("무작위 위치 선택 횟수")]
    [SerializeField, Min(1)] private int maxAttempts = 32;

    public bool TryGetRandomX(float currentX, float playerX, out float destinationX)
    {
        destinationX = currentX;

        if(!isActiveAndEnabled || leftLimit == null || rightLimit == null)
            return false;

        float minX = Mathf.Min(leftLimit.position.x, rightLimit.position.x);

        float maxX = Mathf.Max(leftLimit.position.x, rightLimit.position.x);

        if(maxX - minX <= 0.001f)
            return false;

        for (int i = 0; i< maxAttempts; i++)
        {
            float candidateX = Random.Range(minX, maxX);

            if(!IsValidX(candidateX, currentX, playerX))
                continue;

            destinationX = candidateX;
            return true;
        }

        bool canUseLeft = IsValidX(minX, currentX, playerX);
        bool canUseRight = IsValidX(maxX, currentX, playerX);

        if(!canUseLeft && !canUseRight)
            return false;

        destinationX = canUseLeft && canUseRight ? (Random.value < 0.5f ? minX : maxX) : (canUseLeft ? minX : maxX);

        return true;
    }

    private bool IsValidX(float candidateX, float currentX, float playerX)
    {
        return Mathf.Abs(candidateX - playerX) >= MinimumPlayerDistance && Mathf.Abs(candidateX - currentX) >= minimumMoveDistance;
    }

    private void OnDrawGizmosSelected()
    {
        if(leftLimit == null || rightLimit == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(leftLimit.position, rightLimit.position);

        Gizmos.DrawWireSphere(leftLimit.position, 0.15f);
        Gizmos.DrawWireSphere(rightLimit.position, 0.15f);
    }
}
