using UnityEngine;

namespace Script.OnBoard
{
    public class Energy : OnBoardEntity
    {
        private Board _board;
        public override Vector2Int Size =>  new Vector2Int(1, 1);
        public override EntityType GetEntityType()
        {
            return EntityType.Energy;
        }

        public void Init(Board board)
        {
            _board = board;
        }
        
        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            if (other.GetEntityType() == EntityType.Player)
            {
                if (_board != null)
                {
                    _board.RemoveEntity(this);
                }
                
                Destroy(gameObject);
            }
            return true;
        }
    }
}