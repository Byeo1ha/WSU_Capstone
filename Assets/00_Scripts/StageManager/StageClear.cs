using UnityEngine;

public class StageClear : MonoBehaviour
{
    [SerializeField] private GameObject TargetText;
    [SerializeField] private Portal portal;
    [SerializeField] private RectTransform uiArrow;

    private void Awake()
    {
        TargetText.SetActive(false);
        portal.gameObject.SetActive(false);
        uiArrow.gameObject.SetActive(false);
    }
    
    public void EventStart()
    {
        TargetText.SetActive(true);
        portal.gameObject.SetActive(true);
        uiArrow.gameObject.SetActive(true);
    }
}
