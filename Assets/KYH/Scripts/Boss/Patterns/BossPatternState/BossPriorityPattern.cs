using UnityEngine;

public abstract class BossPriorityPattern : MonoBehaviour
{
    public abstract bool IsPending { get; }
}
