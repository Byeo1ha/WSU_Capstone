using DG.Tweening;
using UnityEngine;

public class CutsceneCameraShake : MonoBehaviour
{
    public void Shake(float duration, float strength)
    {
        transform.DOShakePosition(duration, new Vector3(strength, strength, 0f))
            .SetLink(gameObject);
    }
}
