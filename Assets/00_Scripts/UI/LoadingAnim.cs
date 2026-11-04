using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAnim : MonoBehaviour
{
    [SerializeField] private Sprite[] frame;
    [SerializeField] private float frameDelay = 0.1f;
    [SerializeField] private float textDelay = 0.1f;

    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private string[] loadingText;

    private Coroutine coroutine;
    private Coroutine textCoroutine;

    private void OnEnable()
    {
        coroutine = StartCoroutine(LoadingScreenAsync());
        textCoroutine = StartCoroutine(LoadingScreenTextAsync());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        StopCoroutine(textCoroutine);
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

    private IEnumerator LoadingScreenTextAsync()
    {
        while (true)
        {
            for (int i = 0; i < loadingText.Length; i++)
            {
                targetText.SetText(loadingText[i]);
                yield return new WaitForSecondsRealtime(textDelay);
            }
        }
    }
}
