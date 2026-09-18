using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    public bool IsJumping { get; private set; }

    public void SetGrounded(bool value)
    {
        IsGrounded = value;

        if (value)
            IsJumping = false;
    }

    public void StartJump()
    {
        IsJumping = true;
        IsGrounded = false;
    }
}
