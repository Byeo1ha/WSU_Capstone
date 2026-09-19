using R3;
using UnityEngine;
using VContainer;

public class PlayerNormalAttack : MonoBehaviour
{
    [SerializeField] private float offset = 1.5f;

    private InputManager inputManager;
    private NormalAttackPool normalAttackPool;

    private SpriteRenderer spriteRenderer;

    [Inject]
    public void Construct(
        InputManager inputManager,
        NormalAttackPool normalAttackPool)
    {
        this.inputManager = inputManager;
        this.normalAttackPool = normalAttackPool;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        inputManager.OnNormalAttack
            .Subscribe(_ => Attack())
            .AddTo(this);
    }

    private void Attack()
    {
        NormalAttack attack = normalAttackPool.Get();
        
        float x = spriteRenderer.flipX ? transform.position.x - offset : transform.position.x + offset;

        attack.transform.position = new Vector3(x, transform.position.y, transform.position.z);
        attack.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
    }
}
