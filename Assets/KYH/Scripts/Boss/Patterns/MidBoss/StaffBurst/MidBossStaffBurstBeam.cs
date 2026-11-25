using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MidBossStaffBurstBeam : MonoBehaviour
{
    [Header("표시 오브젝트")]
    [SerializeField] private SpriteRenderer warningRenderer;
    [FormerlySerializedAs("startRenderer")]
    [SerializeField] private SpriteRenderer segmentTemplate;

    [Header("순서대로 반복할 레이저 그림")]
    [SerializeField] private Sprite[] laserSprites;

    [Header("표시 색상")]
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f, 0.35f);
    [SerializeField] private Color attackColor = Color.white;

    private readonly List<SpriteRenderer> segments = new();
    private readonly List<Sprite> segmentSprites = new();
    private readonly Vector2[] clipVertices = new Vector2[4];
    private readonly ushort[] clipTriangles = {0, 1, 2, 2, 3, 0};

    private float beamLength;
    private float beamWidth;
    private float cycleLength;
    private float travelDistance;
    private float emittedLength;
    private float moveSpeed;
    private bool hasGeometry;

    public bool IsReady { get; private set; }
    public bool IsRunning { get; private set; }
    public float VisibleStart { get; private set; }
    public float VisibleLength { get; private set; }

    private void Awake()
    {
        if (warningRenderer == null || warningRenderer.sprite == null ||
            segmentTemplate == null ||
            laserSprites == null || laserSprites.Length == 0)
        {
            Debug.LogError("경고, 레이저 원본 Renderer와 레이저 그림을 연결해 주세요.", this);
            enabled = false;
            return;
        }

        foreach (Sprite sprite in laserSprites)
        {
            if (sprite == null || sprite.rect.width <= 0f || sprite.rect.height <= 0f)
            {
                Debug.LogError("레이저 그림 배열에 비어 있는 항목이 있습니다.", this);
                enabled = false;
                return;
            }

            cycleLength += sprite.rect.width / sprite.pixelsPerUnit;
        }

        segmentTemplate.drawMode = SpriteDrawMode.Simple;
        segmentTemplate.maskInteraction = SpriteMaskInteraction.None;

        segments.Add(segmentTemplate);
        segmentSprites.Add(Instantiate(laserSprites[0]));
        IsReady = true;
        Hide();
    }

    public void ShowWarning(Vector3 start, Vector3 end, float width)
    {
        if (!isActiveAndEnabled)
            return;

        Hide();

        Vector2 direction = end - start;
        beamLength = direction.magnitude;
        beamWidth = width;

        if (beamLength <= 0.001f || beamWidth <= 0f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.position = start;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.localScale = Vector3.one;

        Sprite sprite = warningRenderer.sprite;
        Vector2 size = sprite.bounds.size;

        float scaleX = beamLength / size.x;
        float scaleY = beamWidth / size.y;

        warningRenderer.transform.localRotation = Quaternion.identity;
        warningRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        warningRenderer.transform.localPosition =
            new Vector3(-sprite.bounds.min.x * scaleX, -sprite.bounds.center.y * scaleY, 0f);

        warningRenderer.color = warningColor;
        warningRenderer.enabled = true;

        PrepareSegments();
        hasGeometry = true;
    }

    private void PrepareSegments()
    {
        float positionX = -cycleLength;
        int count = 0;

        while (positionX < beamLength)
        {
            Sprite sprite = laserSprites[count % laserSprites.Length];

            if (count >= segments.Count)
            {
                SpriteRenderer segment = Instantiate(segmentTemplate, transform);
                segment.name = "LaserSegment " + count;
                segment.enabled = false;
                segments.Add(segment);
                segmentSprites.Add(Instantiate(sprite));
            }

            positionX += sprite.rect.width / sprite.pixelsPerUnit;
            count++;
        }
    }

    public void ShowAttack(float emissionDuration, float speed)
    {
        if (!isActiveAndEnabled || !hasGeometry)
            return;

        warningRenderer.enabled = false;

        moveSpeed = Mathf.Max(0.01f, speed);
        emittedLength = moveSpeed * Mathf.Max(0.01f, emissionDuration);
        travelDistance = 0f;
        VisibleStart = 0f;
        VisibleLength = 0f;
        IsRunning = true;

        UpdateLaser();
    }

    public void Tick(float deltaTime)
    {
        if (!IsRunning || deltaTime <= 0f)
            return;

        travelDistance += moveSpeed * deltaTime;
        UpdateLaser();
    }

    private void UpdateLaser()
    {
        VisibleStart = Mathf.Clamp(travelDistance - emittedLength, 0f, beamLength);
        float visibleEnd = Mathf.Min(travelDistance, beamLength);
        VisibleLength = Mathf.Max(0f, visibleEnd - VisibleStart);

        if (VisibleStart >= beamLength)
        {
            IsRunning = false;
            HideSegments();
            return;
        }

        if (VisibleLength <= 0.001f)
        {
            HideSegments();
            return;
        }

        float positionX = Mathf.Repeat(travelDistance, cycleLength) - cycleLength;
        int count = 0;

        while (positionX < beamLength)
        {
            Sprite sprite = laserSprites[count % laserSprites.Length];
            SpriteRenderer segment = segments[count];

            float width = sprite.rect.width / sprite.pixelsPerUnit;
            float height = sprite.rect.height / sprite.pixelsPerUnit;
            Vector2 pivot = sprite.pivot / sprite.pixelsPerUnit;

            segment.sprite = segmentSprites[count];
            segment.color = attackColor;
            segment.transform.localScale = Vector3.one;
            segment.transform.localRotation = Quaternion.identity;
            segment.transform.localPosition = new Vector3(positionX + pivot.x, pivot.y - height * 0.5f, 0f);
            float left = Mathf.Max(0f, VisibleStart - positionX);
            float right = Mathf.Min(width, visibleEnd - positionX);
            segment.enabled = right > left;

            if (segment.enabled)
            {
                float leftPixel = left * sprite.pixelsPerUnit;
                float rightPixel = right * sprite.pixelsPerUnit;

                clipVertices[0] = new Vector2(leftPixel, 0f);
                clipVertices[1] = new Vector2(rightPixel, 0f);
                clipVertices[2] = new Vector2(rightPixel, sprite.rect.height);
                clipVertices[3] = new Vector2(leftPixel, sprite.rect.height);

                segmentSprites[count].OverrideGeometry(clipVertices, clipTriangles);
            }

            positionX += width;
            count++;
        }

        for (int i = count; i < segments.Count; i++)
            segments[i].enabled = false;

    }

    private void HideSegments()
    {
        foreach (SpriteRenderer segment in segments)
        {
            if (segment != null)
                segment.enabled = false;
        }
    }

    public void Hide()
    {
        hasGeometry = false;
        IsRunning = false;
        VisibleStart = 0f;
        VisibleLength = 0f;

        if (warningRenderer != null)
            warningRenderer.enabled = false;

        HideSegments();
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in segmentSprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }
    }

    private void OnDisable()
    {
        Hide();
    }
}