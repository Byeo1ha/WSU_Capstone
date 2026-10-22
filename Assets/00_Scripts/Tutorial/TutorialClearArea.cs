using UnityEngine;

public class TutorialClearArea : MonoBehaviour
{
    [SerializeField] private TutorialEvent tutorialEvent;
    private int targetLayer;

    private void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == targetLayer)
        {
            tutorialEvent.FinishMoveTutorial();
            gameObject.SetActive(false);
        }
    }
}
