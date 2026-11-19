using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PotionEFX : MonoBehaviour
{
    [SerializeField] private float increaseSpeed = 1f;
    private Light2D light2D;

    private bool lightFade;

    private void Awake()
    {
        light2D = GetComponentInChildren<Light2D>();
    }

    private void OnEnable()
    {
        lightFade = true;
        light2D.pointLightOuterRadius = 0f;
        light2D.intensity = 1f;
    }

    private void FixedUpdate()
    {
        if (lightFade)
        {
            light2D.pointLightOuterRadius += increaseSpeed * Time.fixedDeltaTime;
            return;
        }
        FadeOut();
    }

    private void FadeOut()
    {
        DOTween.To(
            () => light2D.intensity,
            value => light2D.intensity = value,
            0f,
            0.5f
        )
        .SetLink(light2D.gameObject);
    }

    public void DisableToLightFade()
    {
        lightFade = false;
    }

    public void DeActive()
    {
        gameObject.SetActive(false);
    }
}
