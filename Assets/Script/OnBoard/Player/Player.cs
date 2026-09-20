using System;
using DG.Tweening;
using UnityEngine;

namespace Script.OnBoard.Player
{
    public class Player : OnBoardEntity
    {
        [SerializeField] private int initialHp;
        [SerializeField] PlayerInputReader playerInputReader;
        [SerializeField] Board board;
        [SerializeField] Vector2Int startCellPosition;
        [SerializeField] Animator animator;
        [SerializeField] AnimationClip playerMove;
        [SerializeField] AnimationClip playerHurt;
        [SerializeField] private HealthShower healthShower;
        public event Action OnDeadEvent;
        
        struct PlayerState
        {
            public int Hp;
            public Vector2 direction;
            public int Energy;
            public bool CanMove;
        }
        
        private PlayerState state;
        
        public Vector2 CellPosition => board.GetEntityPosition(this);
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float invincibilityDuration = 1.5f;
        [SerializeField] private float flashInterval = 0.1f;
        [SerializeField] private int maxEnergy = 6;
        
        public bool IsInvincible { get; private set; }

        void Start()
        {
            PlayerInit();
        }

        public void PlayerInit()
        {
            state = new PlayerState
            {
                Hp = initialHp,
                CanMove = true
            };
            board.PlaceEntity(this, startCellPosition);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this), Size);
            PlayMove();
        }

        public void PlayerResetState()
        {
            state = new PlayerState
            {
                Hp = initialHp,
                CanMove = true
            };

            _invincibleSequence?.Kill();
            IsInvincible = false;
    
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            if (healthShower != null) 
            {
                healthShower.UpdateHealthShow(state.Hp);
            }
            PlayMove();
        }
        public void PlayerPlaceInWorld(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }
        
        void OnEnable()
        {
            playerInputReader.OnMove += HandleMove;
            playerInputReader.OnTest += HandleTest;
        }

        private void HandleTest()
        {
            state.Energy = 5;
            GetEnergy();
        }

        void OnDisable()
        {
            playerInputReader.OnMove -= HandleMove;
            playerInputReader.OnTest -= HandleTest;
        }

        public void PlayerStop()
        {
            state.CanMove = false;
            animator.speed = 0f;
        }
        
        public void PlayerCanMove()
        {
            state.CanMove = true;
            animator.speed = 1f;
        }

        private void HandleMove(Vector2 direction)
        {
            if (!state.CanMove) return;
            if (direction.x <= 0) spriteRenderer.flipX = true;
            else spriteRenderer.flipX = false;
            Vector2Int targetPos = board.GetEntityPosition(this) + Vector2Int.RoundToInt(direction);
            board.TryMove(this, targetPos);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this), Size);
        }

        public void OnHit()
        {
            state.Hp--;
            healthShower.UpdateHealthShow(state.Hp);
            if (state.Hp <= 0)
            {
                OnDead();
            }
            Debug.Log("Hit" + state.Hp);
        }

        private void OnDead()
        {
            OnDeadEvent?.Invoke();
        }

        private Sequence _invincibleSequence;
        public event Action TimeStopEvent;

        private void Invincible()
        {
            if (IsInvincible) return;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _invincibleSequence?.Kill();
            IsInvincible = true;
            
            _invincibleSequence = DOTween.Sequence();

            var blinkTween = ShowInvincibleAnimation();

            _invincibleSequence.Append(blinkTween);

            _invincibleSequence.OnComplete(ResetInvincibility);
            
        }

        private Tween ShowInvincibleAnimation()
        {
            // int loops = Mathf.RoundToInt(invincibilityDuration / (flashInterval * 2f)) * 2;
            //
            // Tween blinkTween = spriteRenderer.DOFade(0.2f, flashInterval)
            //     .SetLoops(loops, LoopType.Yoyo)
            //     .SetEase(Ease.Linear);
            
            PlayHurt();
            Tween blinkTween = DOVirtual.DelayedCall(invincibilityDuration, () => { PlayMove(); }, false);
            return blinkTween;
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

        public override Vector2Int Size => Vector2Int.one;

        public override EntityType GetEntityType()
        {
            return EntityType.Player;
        }

        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            if (other.GetEntityType() == EntityType.Enemy)
            {
                if (!IsInvincible)
                {
                    Invincible();
                    OnHit();
                }
            }

            if (other.GetEntityType() == EntityType.Obstacle)
            {
                if (other is Obstacle { IsTrigger: false })
                {
                    return false;
                }
            }

            if (other.GetEntityType() == EntityType.Energy)
            {
                GetEnergy();
            }
            return true;
        }

        private void GetEnergy()
        {
            state.Energy++;
            Debug.Log("Energy" + state.Energy);
            if (state.Energy >= maxEnergy)
            {
                TimeStopEvent?.Invoke();
                state.Energy = 0;
            }
        }

        private void PlayMove()
        {
            animator.Play(playerMove.name);
        }

        private void PlayHurt()
        {
            animator.Play(playerHurt.name);
        }

        public void ResetInfo()
        {
            state = new PlayerState
            {
                Hp = initialHp,
                CanMove = true
            };
        }
    }
}