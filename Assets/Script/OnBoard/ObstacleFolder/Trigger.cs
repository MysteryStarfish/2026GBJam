using System;
using UnityEngine;

namespace Script.OnBoard.ObstacleFolder
{
    public class Trigger : OnBoardEntity
    {
        public event Action TriggerEvent;
        [SerializeField] private Board board;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private GameObject Level;
        public override Vector2Int Size => size;
        private bool _isUsed = false;

        public void Start()
        {
            Init(board);
        }

        public void Init(Board board)
        {
            var cellPosition = board.WorldToCellPosition(transform.position);
            board.PlaceEntity(this, cellPosition);
            var pos = board.GetEntityPosition(this);
            transform.position = board.CellToWorldPosition(pos, Size);
        }
        public override EntityType GetEntityType()
        {
            return EntityType.Trigger;
        }

        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            if (_isUsed) return true;
            if (other.GetEntityType() != EntityType.Player) return true;
            Level.SetActive(true);
            TriggerEvent?.Invoke();
            _isUsed = true;

            return true;
        }
    }
}