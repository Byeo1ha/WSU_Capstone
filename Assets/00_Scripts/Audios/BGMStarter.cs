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
        soundManager.SetBGM(bgm);
    }

    private void OnEnable()
    {
        soundManager.FadeIn();
    }

    private void OnDisable()
    {
        soundManager.FadeOut();
    }
}
