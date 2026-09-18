using System;
using DG.Tweening;
using Script.Line;
using Unity.VisualScripting;
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
        private Tween moveTween;
        public event Action EnemyDeadEvent;
        private bool isDead = false;

        [System.Serializable]
        public struct MoveInfo
        {
            public Vector2Int initPosition;
            public Vector2Int size;
            public Vector2Int direction;
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
        }

        public void Init(Board board, MoveInfo moveInfo, EnemyState state)
        {
            this.board = board;
            this.moveInfo = moveInfo;
            this.state = state;
            
            moveTween = DOVirtual.DelayedCall(moveInfo.stepInterval, Move)
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

        private void Move()
        {
            var pos = board.GetEntityPosition(this);
            board.TryMove(this, pos + moveInfo.direction);
            transform.position = board.CellToWorldPosition(board.GetEntityPosition(this), Size);
            state.CurrentMoveGrids += 1;
            if ((isFirstMove && state.CurrentMoveGrids > moveInfo.firstMoveDistance) || (!isFirstMove && state.CurrentMoveGrids >= moveGrids))
            {
                state.CurrentMoveGrids = 0;
                moveInfo.direction = -moveInfo.direction;;
                isFirstMove = false;
            }
        }

        public override Vector2Int Size => moveInfo.size;

        public override EntityType GetEntityType()
        {
            return EntityType.Enemy;
        }

        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            if (other.GetEntityType() == EntityType.Obstacle)
            {
                moveInfo.direction = -moveInfo.direction;
                state.CurrentMoveGrids = 0;
                Move();
                return false;
            }

            return true;
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