using DG.Tweening;
using Script.Line;
using UnityEngine;

namespace Script.Player
{
    public class Player : MonoBehaviour, LaserHitable, IOnBoardable
    {
        [SerializeField] private int initialHp;
        [SerializeField] PlayerInputReader playerInputReader;
        [SerializeField] Board board;
        [SerializeField] Vector2Int startCellPosition;
        
        struct PlayerState
        {
            public int Hp;
            public Vector2 direction;
        }
        
        private PlayerState state;
        
        public Vector2 CellPosition => board.GetEntityPosition(this);
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float invincibilityDuration = 1.5f;
        [SerializeField] private float flashInterval = 0.1f;
        
        public bool IsInvincible { get; private set; }

        void Awake()
        {
        }

        void Start()
        {
            state = new PlayerState
            {
                Hp = initialHp
            };
            board.InitEntityPosition(this, startCellPosition);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this));
        }
        
        void OnEnable()
        {
            playerInputReader.OnMove += HandleMove;
        }

        void OnDisable()
        {
            playerInputReader.OnMove -= HandleMove;
        }

        private void HandleMove(Vector2 direction)
        {
            Vector2Int targetPos = board.GetEntityPosition(this) + Vector2Int.RoundToInt(direction);
            board.TryMove(this, targetPos);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this));
        }

        public void OnHit()
        {
            state.Hp--;
            Debug.Log("Hit" + state.Hp);
        }

        private Sequence _invincibleSequence;
        private void Invincibale()
        {
            if (IsInvincible) return;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _invincibleSequence?.Kill();

            IsInvincible = true;

            int loops = Mathf.RoundToInt(invincibilityDuration / (flashInterval * 2f)) * 2;

            _invincibleSequence = DOTween.Sequence();

            Tween blinkTween = spriteRenderer.DOFade(0.2f, flashInterval)
                .SetLoops(loops, LoopType.Yoyo)
                .SetEase(Ease.Linear);

            _invincibleSequence.Append(blinkTween);

            _invincibleSequence.OnComplete(ResetInvincibility);
            
        }
        
        private void ResetInvincibility()
        {
            if (spriteRenderer != null)
            {
                // 還原為完全不透明
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            IsInvincible = false;
        }
        
        public EntityType GetEntityType()
        {
            return EntityType.Player;
        }

        public void OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, IOnBoardable other)
        {
            if (other.GetEntityType() == EntityType.Enemy)
            {
                if (!IsInvincible)
                {
                    Invincibale();
                    OnHit();
                }
            }
        }
    }
}