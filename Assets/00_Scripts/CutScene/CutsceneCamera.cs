using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CutsceneCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset;
    [Min(0.01f)] [SerializeField] private float moveSpeed = 5f;

    [Header("맵 가장자리의 월드 좌표")]
    [SerializeField] private bool xMinEnabled;
    [SerializeField] private float xMinValue = -20f;
    [SerializeField] private bool xMaxEnabled;
    [SerializeField] private float xMaxValue = 20f;
    [SerializeField] private bool yMinEnabled;
    [SerializeField] private float yMinValue = -10f;
    [SerializeField] private bool yMaxEnabled;
    [SerializeField] private float yMaxValue = 10f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            transform.position = GetFocusPosition(target);
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.01f, speed);
    }

    public async UniTask FocusAsync(Transform newTarget)
    {
        target = newTarget;
        Vector3 destination = GetFocusPosition(target);
        float duration = Vector3.Distance(transform.position, destination) / moveSpeed;
        await transform.DOMove(destination, duration)
            .SetLink(gameObject).AsyncWaitForCompletion();
    }

    private Vector3 GetFocusPosition(Transform focusTarget)
    {
        Vector3 position = focusTarget.position + targetOffset;
        position.z = transform.position.z;
        return ClampPosition(position);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float halfHeight = cam.orthographicSize;
        if (xMinEnabled && xMaxEnabled)
            halfHeight = Mathf.Min(halfHeight, (xMaxValue - xMinValue) / (2f * cam.aspect));
        if (yMinEnabled && yMaxEnabled)
            halfHeight = Mathf.Min(halfHeight, (yMaxValue - yMinValue) * 0.5f);

        cam.orthographicSize = Mathf.Max(0.01f, halfHeight);
        float halfWidth = cam.orthographicSize * cam.aspect;
        halfHeight = cam.orthographicSize;
        if (xMinEnabled) position.x = Mathf.Max(position.x, xMinValue + halfWidth);
        if (xMaxEnabled) position.x = Mathf.Min(position.x, xMaxValue - halfWidth);
        if (yMinEnabled) position.y = Mathf.Max(position.y, yMinValue + halfHeight);
        if (yMaxEnabled) position.y = Mathf.Min(position.y, yMaxValue - halfHeight);
        return position;
    }

    private void LateUpdate()
    {
        transform.position = ClampPosition(transform.position);
    }
}
