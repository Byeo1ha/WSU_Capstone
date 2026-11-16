using UnityEngine;

public abstract class BossPriorityPattern : MonoBehaviour
{
    public abstract bool IsPending { get; }
    public abstract bool IsRunning { get; }
    public abstract bool TryStartPriority();
    public abstract void CancelPriority();
}
