using UnityEngine;
using VContainer;

public class BGMStarter : MonoBehaviour
{
    [SerializeField] private AudioClip bgm;
    private SoundManager soundManager;

    [Inject]
    public void Construct(SoundManager soundManager)
    {
        this.soundManager = soundManager;
    }

    private void Awake()
    {
        if (soundManager == null) return;

        soundManager.SetBGM(bgm);
    }

    private void OnEnable()
    {
        if (soundManager == null) return;

        soundManager.FadeIn();
    }

    private void OnDisable()
    {
        if (soundManager == null) return;

        soundManager.FadeOut();
    }
}
