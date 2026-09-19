using System;
using DG.Tweening;
using Script.Line;
using UnityEngine;

namespace Script.Enemy
{
    public class Enemy : OnBoardEntity, LaserHitable
    {
        private IEnemyMovement _enemyMovement;
        [SerializeField] private Board board;
        private int moveGrids => moveInfo.moveDistance;
        private bool isFirstMove = true;
        [SerializeField] private MoveInfo moveInfo;
        [SerializeField] private EnemyState state;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private IEnemyMovement enemyMovement;

        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip[] monster;
        private AnimationClip animation;
        private Tween moveTween;
        public event Action EnemyDeadEvent;
        private bool isDead = false;

        [System.Serializable]
        public struct MoveInfo
        {
            public Vector2Int initPosition;
            public Vector2Int size;
            public Vector2Int direction;
            public bool FaceLeft => direction.x > 0;
            public int firstMoveDistance;
            public int moveDistance;
            public float stepInterval;
        }

        [System.Serializable]
        public struct EnemyState
        {
            public int Hp;
            public int Atk;
            public int CurrentMoveGrids;
            public int type;
        }

        public void Init(Board board, MoveInfo moveInfo, EnemyState state)
        {
            this.board = board;
            this.moveInfo = moveInfo;
            this.state = state;

            animation = monster[state.type];
            animator.Play(animation.name);

            enemyMovement = GetMovement(state.type);

            moveTween = DOVirtual.DelayedCall(moveInfo.stepInterval, Move)
                .SetLoops(-1, LoopType.Restart);
        }

        public void SetMoveSpeedMultiplier(float multiplier)
        {
            if (moveTween != null)
            {
                moveTween.timeScale = multiplier;
            }
        }

        public void ChangeMoveInterval(float newInterval)
        {
            moveTween?.Kill();
            moveTween = DOVirtual.DelayedCall(newInterval, Move).SetLoops(-1, LoopType.Restart);
        }

        private void OnDestroy()
        {
            moveTween?.Kill();
        }

        public void OnHit()
        {
            state.Hp--;
            Debug.Log(state.Hp);
            if (state.Hp <= 0)
            {
                OnDead();
            }
        }

        private void OnDead()
        {
            if (isDead) return;
            EnemyDeadEvent?.Invoke();
            isDead = true;

            moveTween?.Kill();

            board.RemoveEntity(this);
            Destroy(gameObject);
        }

        public void Dead()
        {
            OnDead();
        }

        private bool _hasBouncedThisTurn = false;

        private void Move()
        {
            // 2. 只有由 DOTween 定時器正常觸發的移動，才解開這個鎖
            _hasBouncedThisTurn = false;
            enemyMovement.Move();
        }

        private IEnemyMovement GetMovement(int type)
        {
            if (type == 0) return new YoyoMove(board, moveInfo, transform, state, this, moveGrids, spriteRenderer);
            if (type == 1)
                return new DashMove(board, moveInfo, transform, state, this, moveGrids, spriteRenderer,
                    board.Player.transform);
            return new YoyoMove(board, moveInfo, transform, state, this, moveGrids, spriteRenderer);
        }

        public override Vector2Int Size => moveInfo.size;
        public SpriteRenderer Sprite => spriteRenderer;

        public override EntityType GetEntityType()
        {
            return EntityType.Enemy;
        }

        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            if (other.GetEntityType() == EntityType.Obstacle || (other.GetEntityType() == EntityType.Enemy && originPos != targetPos))
            {
                ChangeDirection(); // 先轉向

                // 3. 檢查這回合是否已經反彈過了
                if (!_hasBouncedThisTurn)
                {
                    _hasBouncedThisTurn = true; // 鎖上，防止再次觸發
                    enemyMovement.Move(); // 立刻往反方向走一步
                }

                return false;
            }

            return true;
        }

        private void ChangeDirection()
        {
            enemyMovement.ChangeDirection();
        }
    }

    public interface IEnemyMovement
    {
        void Move();
        void ChangeDirection();
    }

    public abstract class BasicEnemyMove : IEnemyMovement
    {
        protected Board _board;
        protected Enemy.MoveInfo _moveInfo;
        protected Transform _transform;
        protected Enemy.EnemyState _state;
        protected Enemy _enemy;
        protected bool isFirstMove = true;
        protected int _moveGrids;
        protected SpriteRenderer _spriteRenderer;

        public BasicEnemyMove(Board board, Enemy.MoveInfo moveInfo, Transform transform, Enemy.EnemyState state,
            Enemy enemy, int moveGrids, SpriteRenderer spriteRenderer)
        {
            _board = board;
            _moveInfo = moveInfo;
            _transform = transform;
            _state = state;
            _enemy = enemy;
            _moveGrids = moveGrids;
            _spriteRenderer = spriteRenderer;
        }

        public virtual void ChangeDirection()
        {
            _moveInfo.direction = -_moveInfo.direction;
            _state.CurrentMoveGrids = 0;
            isFirstMove = false;
        }

        public abstract void Move();
    }

    public class YoyoMove : BasicEnemyMove
    {
        public YoyoMove(Board board, Enemy.MoveInfo moveInfo, Transform transform, Enemy.EnemyState state, Enemy enemy,
            int moveGrids, SpriteRenderer spriteRenderer)
            : base(board, moveInfo, transform, state, enemy, moveGrids, spriteRenderer)
        {
        }

        public override void Move()
        {
            var pos = _board.GetEntityPosition(_enemy);
            _board.TryMove(_enemy, pos + _moveInfo.direction);
            _transform.position = _board.CellToWorldPosition(_board.GetEntityPosition(_enemy), _moveInfo.size);
            _state.CurrentMoveGrids += 1;
            if ((isFirstMove && _state.CurrentMoveGrids > _moveInfo.firstMoveDistance) ||
                (!isFirstMove && _state.CurrentMoveGrids >= _moveGrids))
            {
                ChangeDirection();
            }

            _spriteRenderer.flipX = _moveInfo.FaceLeft;
        }
    }

    public class DashMove : BasicEnemyMove
    {
        private Transform _target;
        private bool _isDashing = false;
        private Tween _stunTween; // 記錄停頓的 Tween

        public DashMove(Board board, Enemy.MoveInfo moveInfo, Transform transform, Enemy.EnemyState state,
            Enemy enemy, int moveGrids, SpriteRenderer spriteRenderer, Transform target)
            : base(board, moveInfo, transform, state, enemy, moveGrids, spriteRenderer)
        {
            _target = target;
        }

        public override void Move()
        {
            var pos = _board.GetEntityPosition(_enemy);

            // 修正 1：直接透過 Board 取得玩家邏輯座標，完全避開 WorldToCellPosition 產生的捨入誤差
            Vector2Int targetPos = _board.GetEntityPosition(_board.Player);

            if (!_isDashing)
            {
                // 追蹤階段：尋找同 X 軸
                if (pos.x != targetPos.x)
                {
                    int dirX = targetPos.x > pos.x ? 1 : -1;
                    _moveInfo.direction = new Vector2Int(dirX, 0);
                }
                else // pos.x == targetPos.x，進入衝刺準備
                {
                    _isDashing = true;

                    _enemy.SetMoveSpeedMultiplier(0.0f); // 停頓

                    _stunTween?.Kill();

                    _enemy.Sprite.DOColor(Color.red, 0.5f).SetEase(Ease.Linear);
                    // 加入 SetLink，如果 _enemy 的 GameObject 被銷毀，Tween 會自動中斷
                    _stunTween = DOVirtual.DelayedCall(0.5f, () =>
                    {
                        if (_enemy != null)
                        {
                            _enemy.SetMoveSpeedMultiplier(2.0f); // 恢復並加速準備衝刺
                        }
                    }).SetLink(_enemy.gameObject);

                    // 修正：決定衝刺的 Y 軸方向
                    int dirY = (targetPos.y == pos.y) ? 0 : (targetPos.y > pos.y ? 1 : -1);
                    if (dirY != 0)
                    {
                        _moveInfo.direction = new Vector2Int(0, dirY);
                    }

                    // 修正 2：剛對齊 X 軸的瞬間應該在原地停頓，直接 return 不要往下執行 TryMove 偷跑
                    return;
                }
            }

            // 嘗試移動
            bool moveSuccess = _board.TryMove(_enemy, pos + _moveInfo.direction);

            // 如果移動失敗 (例如撞到地圖邊界)，觸發轉向與停頓
            if (!moveSuccess && _isDashing)
            {
                ChangeDirection();
                return; // 撞牆的該回合直接結束，不更新畫面位置
            }

            _transform.position = _board.CellToWorldPosition(_board.GetEntityPosition(_enemy), _moveInfo.size);

            if (_moveInfo.direction.x != 0)
            {
                _spriteRenderer.flipX = _moveInfo.FaceLeft;
            }
        }

        public override void ChangeDirection()
        {
            if (_isDashing)
            {
                _enemy.SetMoveSpeedMultiplier(0.0f); // 撞到東西，停頓
                _state.CurrentMoveGrids = 0;

                // 確保先殺死之前的計時器，避免多次觸發疊加
                _stunTween?.Kill();
                
                _enemy.Sprite.DOColor(Color.white, 1.0f).SetEase(Ease.Linear);
                _stunTween = DOVirtual.DelayedCall(1.0f, () =>
                {
                    if (_enemy != null)
                    {
                        _enemy.SetMoveSpeedMultiplier(1.0f); // 恢復正常左右尋找的速度
                    }
                    _isDashing = false;
                }).SetLink(_enemy.gameObject);
            }
            else
            {
                base.ChangeDirection();
            }
        }
    }
}