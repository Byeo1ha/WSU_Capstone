using UnityEngine;

public class TargetCamera : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("카메라 추적 속도")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("카메라 최대값 / 최솟값")]
    [SerializeField] private bool xMaxEnabled = false;
    [SerializeField] private float xMaxValue = 0;
    [SerializeField] private bool xMinEnabled = false;
    [SerializeField] private float xMinValue = 0;

    [SerializeField] private bool yMaxEnabled = false;
    [SerializeField] private float yMaxValue = 0;
    [SerializeField] private bool yMinEnabled = false;
    [SerializeField] private float yMinValue = 0;

    [Header("카메라 흔들림 연출 값")]
    [SerializeField] private float maxShakingValue = 0.2f;
    [SerializeField] private float minShakingValue = -0.2f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate() 
    {
        Vector3 targetPos = target.position;

        if (yMinEnabled && yMaxEnabled)
            targetPos.y = Mathf.Clamp(
                target.position.y, 
                yMinValue, 
                yMaxValue);

        else if (yMinEnabled)
            targetPos.y = Mathf.Clamp(
                target.position.y, 
                yMinValue, 
                target.position.y);

        else if (yMaxEnabled)
            targetPos.y = Mathf.Clamp(
                target.position.y, 
                target.position.y, 
                yMaxValue);

        if (xMinEnabled && xMaxEnabled)
            targetPos.x = Mathf.Clamp(
                target.position.x, 
                xMinValue, 
                xMaxValue);

        else if (xMinEnabled)
            targetPos.x = Mathf.Clamp(
                target.position.x, 
                xMinValue, 
                target.position.x);

        else if (xMaxEnabled)
            targetPos.x = Mathf.Clamp(
                target.position.x, 
                target.position.x, 
                xMaxValue);

        targetPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPos, 
            ref velocity, 
            smoothTime);
    }

    public void Shaking()
    {
        Debug.Log("화면");
        float randomX = Random.Range(minShakingValue, maxShakingValue);
        float randomY = Random.Range(minShakingValue, maxShakingValue);
        float randomZ = Random.Range(minShakingValue, maxShakingValue);

        transform.position = new Vector3(
            transform.position.x + randomX,
            transform.position.y + randomY,
            transform.position.z + randomZ
        );
    }
}
