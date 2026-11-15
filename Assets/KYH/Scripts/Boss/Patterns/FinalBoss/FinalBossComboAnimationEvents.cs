using UnityEngine;

public class FinalBossComboAnimationEvents : MonoBehaviour
{
    [SerializeField] private FinalBossComboPattern comboPattern;

    public void OnComboHit(int hitNumber)
    {
        if (comboPattern != null)
            comboPattern.HandleComboHit(hitNumber);
    }

    public void OnComboAnimationFinished()
    {
        if (comboPattern != null)
            comboPattern.HandleComboAnimationFinished();
    }
}
