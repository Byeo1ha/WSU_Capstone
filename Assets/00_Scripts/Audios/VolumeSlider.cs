using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string parameterName;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        float savedVolume = PlayerPrefs.GetFloat(parameterName, 1f);

        slider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);

        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        ApplyVolume(volume);
        PlayerPrefs.SetFloat(parameterName, volume);
    }

    private void ApplyVolume(float volume)
    {
        float dB = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(parameterName, dB);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(SetVolume);
    }
}
