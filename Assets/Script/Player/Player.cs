using UnityEngine;

namespace Script.Player
{
    public class Player : MonoBehaviour
    {
        [SerializeField] PlayerInputReader playerInputReader;
        [SerializeField] Board board;
        [SerializeField] Vector2 startCellPosition;
        private IPlayerMovement playerMovement;
        
        struct PlayerState
        {
            public Vector2 cellPosition;
            public Vector2 direction;
        }
        
        private PlayerState state;
        
        public Vector2 CellPosition => state.cellPosition;

        void Awake()
        {
            playerMovement = new BoardMovement(this);
        }

        void Start()
        {
            state = new PlayerState
            {
                cellPosition = startCellPosition
            };
            transform.position = board.CellToWorldPosition(state.cellPosition);
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
            playerMovement.Move(direction);
        }
        
        public void SetPlayerPosition(Vector2 newCellPosition)
        {
            state.cellPosition = newCellPosition;
            transform.position = board.CellToWorldPosition(state.cellPosition);
        }
    }

    public interface IPlayerMovement
    {
        public void Move(Vector2 direction);
    }
    
    public class BoardMovement : IPlayerMovement
    {
        private Player _player;
        
        public BoardMovement(Player player)
        {
            this._player = player;
        }

        public void Move(Vector2 direction)
        {
            var position = _player.CellPosition;
            var newPosition = position + direction;
            _player.SetPlayerPosition(newPosition);
        }
    }
}