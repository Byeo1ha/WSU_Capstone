using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
public class PlayerHighAttack : MonoBehaviour
{
    [SerializeField] private float upperPower = 20f;

    private InputManager inputManager;
    private Rigidbody2D rigid;
    private PlayerState playerState;

    private bool upInput;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Start()
    {
        inputManager.UpInput
            .Subscribe(value => upInput = value)
            .AddTo(this);

        inputManager.OnNormalAttack
            .Where(_ => upInput)
            .Subscribe(_ => Attack())
            .AddTo(this);
    }

    private void Attack()
    {
        rigid.AddForce(Vector2.up * upperPower, ForceMode2D.Impulse);
        Debug.Log("수행");
    }

}
