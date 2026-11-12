using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnim : MonoBehaviour
{
    [SerializeField] private Sprite[] frame;
    [SerializeField] private float frameDelay = 0.1f;
    [SerializeField] private Image targetImage;

    private Coroutine coroutine;

    private void OnEnable()
    {
        coroutine = StartCoroutine(LoadingScreenAsync());
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator LoadingScreenAsync()
    {
        while (true)
        {
            for (int i = 0; i < frame.Length; i++)
            {
                targetImage.sprite = frame[i];
                yield return new WaitForSecondsRealtime(frameDelay);
            }
        }
    }
}
