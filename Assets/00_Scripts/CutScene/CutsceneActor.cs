using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CutsceneActor : MonoBehaviour
{
    [SerializeField] private CutsceneSpeakerData speaker;

    public CutsceneSpeakerData Speaker => speaker;

    public async UniTask MoveXAsync(float destinationX, float duration)
    {
        await transform.DOMoveX(destinationX, duration)
            .SetLink(gameObject).AsyncWaitForCompletion();
    }

    public async UniTask MoveToAsync(Vector3 destination, float duration)
    {
        await transform.DOMove(destination, duration)
            .SetLink(gameObject).AsyncWaitForCompletion();
    }
}
