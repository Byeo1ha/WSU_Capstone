using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CutsceneCameraZoom : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public async UniTask ZoomToAsync(float size, float duration)
    {
        await cam.DOOrthoSize(Mathf.Max(0.01f, size), duration)
            .SetLink(gameObject).AsyncWaitForCompletion();
    }
}
