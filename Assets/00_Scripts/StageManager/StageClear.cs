using UnityEngine;

public class StageClear : MonoBehaviour
{
    [SerializeField] private GameObject TargetText;

    private void Awake()
    {
        TargetText.SetActive(false);
    }
    
    public void EventStart()
    {
        TargetText.SetActive(true);
    }
}
