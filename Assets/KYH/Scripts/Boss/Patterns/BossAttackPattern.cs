using UnityEngine;

public abstract class BossAttackPattern : MonoBehaviour
{
    public virtual bool CanSelect => isActiveAndEnabled;
    public abstract bool TryStartAttack();
}

public interface IBossApproachRange
    {
        Vector2 ApproachStartRange { get;}
    }
