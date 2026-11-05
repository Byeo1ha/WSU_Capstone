using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnim : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float textDelay = 0.5f;
    [SerializeField] private string[] frameText;

    private Coroutine coroutine;

    private void Awake()
    {
        coroutine = StartCoroutine(LoadingScreenTextAsync());
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator LoadingScreenTextAsync()
    {
        while (true)
        {
            for (int i = 0; i < frameText.Length; i++)
            {
                targetText.SetText(frameText[i]);
                yield return new WaitForSecondsRealtime(textDelay);
            }
        }
    }
}
