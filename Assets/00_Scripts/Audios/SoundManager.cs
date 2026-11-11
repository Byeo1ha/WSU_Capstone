using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource audioSource;

    private Tween tween;
    private float originalValue;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalValue = audioSource.volume;
    }

    public void SetBGM(AudioClip audio)
    {
        audioSource.clip = audio;
        audioSource.Play();
    }

    public void FadeOut()
    {
        tween?.Kill();

        originalValue = audioSource.volume;
        tween = audioSource.DOFade(0f, 0.5f)
            .SetLink(gameObject);
    }

    public void FadeIn()
    {
        tween?.Kill();
        
        tween = audioSource.DOFade(originalValue, 0.5f)
            .SetLink(gameObject);
    }
}
