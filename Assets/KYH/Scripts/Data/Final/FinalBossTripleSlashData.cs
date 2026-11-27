using UnityEngine;

[CreateAssetMenu( fileName = "FinalBossTripleSlashData",
    menuName = "Boss/Final Boss Triple Slash Data")]
public class FinalBossTripleSlashData : ScriptableObject
{
    [Header("참격 이동")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 8f;

    [Header("타격별 설정")]
    [SerializeField] private SlashSettings firstSlash = new SlashSettings(8);
    [SerializeField] private SlashSettings secondSlash = new SlashSettings(10);
    [SerializeField] private SlashSettings thirdSlash = new SlashSettings(14);

    public float MoveSpeed => moveSpeed;

    public SlashSettings GetSlash(int hitNumber)
    {
        switch (hitNumber)
        {
            case 1:
                return firstSlash;

            case 2:
                return secondSlash;

            case 3:
                return thirdSlash;

            default:
                return null;
        }
    }

    [System.Serializable]
    public class SlashSettings
    {
        [SerializeField] private Sprite sprite;
        [SerializeField, Min(0)] private int damage;

        [Header("피해 판정")]
        [SerializeField] private Vector2 colliderSize = Vector2.one;
        [SerializeField] private Vector2 colliderOffset;

        public Sprite Sprite => sprite;
        public int Damage => damage;
        public Vector2 ColliderSize => colliderSize;
        public Vector2 ColliderOffset => colliderOffset;

        public SlashSettings(int damage)
        {
            this.damage = damage;
        } 
    }
}
