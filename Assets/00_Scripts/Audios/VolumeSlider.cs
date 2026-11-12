using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string parameterName;

    private Slider slider;
    private float savedVolume;

    public bool HasUnsavedChanges
    {
        get
        {
            Initialize();
            return !Mathf.Approximately(slider.value, savedVolume);
        }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        ApplyVolume(slider.value);
    }

    private void Initialize()
    {
        if (slider != null) return;

        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        savedVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(parameterName, 1f));
        slider.SetValueWithoutNotify(savedVolume);
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        ApplyVolume(volume);
    }

    public void SaveVolume()
    {
        Initialize();
        savedVolume = slider.value;
        PlayerPrefs.SetFloat(parameterName, savedVolume);
    }

    public void RestoreVolume()
    {
        Initialize();
        slider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);
    }

    private void ApplyVolume(float volume)
    {
        float dB = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(parameterName, dB);
    }
}
