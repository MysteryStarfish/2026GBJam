using DG.Tweening;
using Script.Line;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Enemy
{
    public class Enemy : MonoBehaviour, LaserHitable, IOnBoardable
    {
        private IEnemyMovement _enemyMovement;
        [SerializeField] private Board board;
        [SerializeField] private int forwardGrids;
        [SerializeField] private int backwardGrids; 
        private int moveGrids => forwardGrids + backwardGrids + 1;
        private bool isFirstMove = true;
        [SerializeField] private Vector2Int initPosition;
        [SerializeField] private Vector2Int size;
        [SerializeField] private Vector2Int direction;
        [SerializeField] private float stepInterval;
        [SerializeField] private EnemyState state;
        private Tween moveTween;
        
        [System.Serializable]
        struct EnemyState
        {
            public int Hp;
            public int Atk;
            public int CurrentMoveGrids;
        }

        void Start()
        {
            board.InitEntityPosition(this, initPosition, size);
            transform.position = board.CellToWorldPosition(initPosition);

            moveTween = DOVirtual.DelayedCall(stepInterval, Move)
                .SetLoops(-1, LoopType.Restart);
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
            moveTween?.Kill();
            
            board.RemoveEntity(this);
            Destroy(gameObject);
        }

        private void Move()
        {
            var pos = board.GetEntityPosition(this);
            board.TryMove(this, pos + direction);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this));
            state.CurrentMoveGrids += 1;
            if ((isFirstMove && state.CurrentMoveGrids > forwardGrids) || (!isFirstMove && state.CurrentMoveGrids >= moveGrids))
            {
                state.CurrentMoveGrids = 0;
                direction = -direction;
                isFirstMove = false;
            }
        }

        public EntityType GetEntityType()
        {
            return EntityType.Enemy;
        }

        public void OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, IOnBoardable other)
        {
            board.ForceMove(this, targetPos);
        }
    }

    public interface IEnemyMovement
    {
        void Move();
    }
    
    public class YoyoMove : IEnemyMovement
    {
        public void Move()
        {
            
        }
    }
}