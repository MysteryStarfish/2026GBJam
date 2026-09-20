using Script.OnBoard.Player;
using UnityEngine;

namespace Script.LineFolder
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private PlayerInputReader playerInputReader;
        [SerializeField] private PlayerDetector playerDetector;
        [SerializeField] private Player player;
        [SerializeField] private BulletSlotShower bulletSlotShower;
    
        [SerializeField] private int bulletCount = 6;
        private BulletSlot bulletSlot;
    
        [SerializeField] private Vector2 startPos;
        [SerializeField] private float moveDistance;
        [SerializeField] private float moveSpeed;
        [SerializeField] private bool moveHorizontally;
        [SerializeField] private bool isMoveYoyo;
        [SerializeField] private bool isGridMove;
        private bool isMoveToRight;
    
        private bool isActivate;

        [SerializeField] private bool isCanvaMove;
        private RectTransform rectTransform;
    
        private void Start()
        {
            if (isCanvaMove) rectTransform = GetComponent<RectTransform>();
            bulletSlot = new BulletSlot();
        }
    
        public void RefillAll()
        {
            bulletSlot.Clear();
            for (int i = 0; i < bulletCount; i++)
            {
                bulletSlot.GenerateBullet();
            }
            bulletSlotShower.RefillAll(bulletSlot);
        }

        private void OnEnable()
        {
            playerInputReader.OnAttack += HandleAttack;
            playerInputReader.OnTest += HandleTest;
            player.TimeStopEvent += RefillAll; 
        }

        private void HandleTest()
        {
            RefillAll();
        }

        private void OnDisable()
        {
            playerInputReader.OnAttack -= HandleAttack;
            playerInputReader.OnTest -= HandleTest;
            player.TimeStopEvent -= RefillAll; 
        }

        private void FixedUpdate()
        {
            if (!isActivate) return; 
            if (!isCanvaMove) Movement();
            else CanvaMovement();
        }
    
        private float moveProgress = 0f;

        public void Activate()
        {
            isActivate = true;
            moveProgress = 0f; // 每次啟動時，進度歸零
        }

        private void Movement()
        {
            if (isGridMove) GridMovement();
            else SmoothMovement();
        }

        private void SmoothMovement()
        {
            float offset = 0;
        
            // 累加真正的移動進度 (因為在 FixedUpdate 執行，所以用 fixedDeltaTime)
            moveProgress += Time.fixedDeltaTime * moveSpeed;

            if (isMoveYoyo) 
            {
                offset = Mathf.PingPong(moveProgress, moveDistance);
            }
            else
            {
                if (!isMoveToRight) offset = Mathf.Lerp(0, moveDistance, moveProgress);
                else offset = Mathf.Lerp(moveDistance, 0, moveProgress);
            
                // 使用進度 >= 1 來判斷是否抵達終點，比判斷浮點數 (offset == 0) 更安全
                if (moveProgress >= 1f)
                {
                    isActivate = false;
                    isMoveToRight = !isMoveToRight;
                    // 注意：如果下次 Activate 希望接續反向移動，這裡的 moveProgress 會在 Activate() 中被重置，這是正確的
                }
            }

            var pos = moveHorizontally ? new Vector3(startPos.x + offset, startPos.y, 0) : new Vector3(startPos.x, startPos.y + offset, 0);
            transform.position = pos;

        }
        
        private void GridMovement()
        {
            float offset = 0;
        
            // 累加真正的移動進度 (因為在 FixedUpdate 執行，所以用 fixedDeltaTime)
            moveProgress += Time.fixedDeltaTime * moveSpeed;

            if (isMoveYoyo) 
            {
                offset = Mathf.PingPong(moveProgress, moveDistance);
            }
            else
            {
                if (!isMoveToRight) offset = Mathf.Lerp(0, moveDistance, moveProgress);
                else offset = Mathf.Lerp(moveDistance, 0, moveProgress);
            
                // 使用進度 >= 1 來判斷是否抵達終點，比判斷浮點數 (offset == 0) 更安全
                if (moveProgress >= 1f)
                {
                    isActivate = false;
                    isMoveToRight = !isMoveToRight;
                    // 注意：如果下次 Activate 希望接續反向移動，這裡的 moveProgress 會在 Activate() 中被重置，這是正確的
                }
            }

            var pos = moveHorizontally ? new Vector3(startPos.x + offset, startPos.y, 0) : new Vector3(startPos.x, startPos.y + offset, 0);
            transform.position = board.CellToWorldPosition(board.WorldToCellPosition(pos), Vector2Int.one);
        }
    
        private Vector2 initialPos = new Vector2(2.5f, 0);

        private void CanvaMovement()
        {
            // 這裡同樣替換為自定義的進度
            moveProgress += Time.fixedDeltaTime * moveSpeed;
        
            var _moveDistance = 155;  
            var offset = Mathf.Lerp(0, _moveDistance, moveProgress);

            rectTransform.anchoredPosition = moveHorizontally ? new Vector3(initialPos.x + offset, initialPos.y, 0) : new Vector3(initialPos.x, initialPos.y + offset, 0);

            // UI 移動完成後也自動關閉
            if (moveProgress >= 1f)
            {
                isActivate = false;
            }
        }
    
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? startPos : transform.position;
            Gizmos.color = Color.cyan;
            if (moveHorizontally)
            {
                Gizmos.DrawLine(origin, origin + Vector3.right * moveDistance);
                Gizmos.DrawWireSphere(origin, 0.1f);
                Gizmos.DrawWireSphere(origin + Vector3.right * moveDistance, 0.1f);
            }
            else
            {
                Gizmos.DrawLine(origin, origin + Vector3.up * moveDistance);
                Gizmos.DrawWireSphere(origin, 0.1f);
                Gizmos.DrawWireSphere(origin + Vector3.up * moveDistance, 0.1f);
            }
        }

        private void HandleAttack(BulletType attackType)
        {
            if (GetBullet().GetAttackType() != attackType)
            {
                ShotFail();
                return;
            };
        
            ShotSuccess();
        }

        private Bullet GetBullet()
        {
            return bulletSlot.GetBullet();
            // else return bloodSlot.GetBullet();
        }

        private void ShotSuccess()
        {
            if (isCanvaMove)
            {
                var screenPos = rectTransform.position;
                if (Camera.main == null) return;
                screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
                var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                worldPos.z = 0f;
                playerDetector.DetectedAndHit(worldPos);
            }
            else
            {
                playerDetector.DetectedAndHit();
            }

            UseBullet();
        }

        private void UseBullet()
        {
            bulletSlot.UseBullet();
            bulletSlotShower.UpdateBullet();
        }

        private void ShotFail()
        {
            UseBullet();
            Debug.Log("Failed!");
        }

        public void ClearBullet()
        {
            bulletSlot.Clear();
            bulletSlotShower.UpdateBullet();
        }
    }
}