using UnityEngine;

public abstract class BossAttackPattern : MonoBehaviour
{
    public abstract bool TryStartAttack();
}

public interface IBossApproachRange
    {
        Vector2 ApproachStartRange { get;}
    }
