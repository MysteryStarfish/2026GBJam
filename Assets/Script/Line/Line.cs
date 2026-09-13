using System;
using System.Collections.Generic;
using Script.Line;
using UnityEngine;
using UnityEngine.Serialization;

public class Line : MonoBehaviour
{
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private PlayerDetector playerDetector;
    [SerializeField] private BulletSlot bulletSlot;
    
    [SerializeField] private Vector2 startPos;
    [SerializeField] private float moveDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool moveHorizontally;

    [SerializeField] private bool isCanvaMove;
    private RectTransform rectTransform;

    private void Start()
    {
        if (isCanvaMove) rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        playerInputReader.OnAttack += HandleAttack;
    }

    void OnDisable()
    {
        playerInputReader.OnAttack -= HandleAttack;
    }

    void FixedUpdate()
    {
        if (!isCanvaMove) Movement();
        else CanvaMovement();
    }

    void Movement()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);

        if (moveHorizontally)
        {
            transform.position = new Vector3(startPos.x + offset, startPos.y, 0);
        }
        else
        {
            transform.position = new Vector3(startPos.x, startPos.y + offset, 0);
        }
    }
    
    private Vector2 initialPos = new Vector2(2.5f, 0);  
    void CanvaMovement()
    {
        var _moveDistance = 155;  
        float offset = Mathf.PingPong(Time.time * moveSpeed, _moveDistance);

        if (moveHorizontally)
        {
            rectTransform.anchoredPosition = new Vector3(initialPos.x + offset, initialPos.y, 0);
        }
        else
        {
            rectTransform.anchoredPosition = new Vector3(initialPos.x, initialPos.y + offset, 0);
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
        if (isCanvaMove)
        {
            Vector3 screenPos = rectTransform.position;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            playerDetector.DetectedAndHit(worldPos);
        }
        else
        {
            playerDetector.DetectedAndHit();
        }
    }
}

public enum BulletType
{
    A,
    B
}

public class BulletSlot
{
    BulletFactory  bulletFactory;

    public BulletSlot()
    {
        bulletFactory = new BulletFactory();
    }
    List<Bullet> bullets = new List<Bullet>();
    public void GenerateBullet()
    {
        var bullet = bulletFactory.CreateRandomBullet();
        bullets.Add(bullet);
    }
}

public class BulletFactory
{
    public Bullet CreateBullet(BulletType attackType)
    {
        return new Bullet(attackType);
    }

    public Bullet CreateRandomBullet()
    {
        BulletType randomType = (BulletType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BulletType)).Length);
        return CreateBullet(randomType);
    }
}

public class Bullet
{
    private BulletType attackType;
    public Bullet(BulletType attackType)
    {
        this.attackType = attackType;
    }
    public BulletType GetAttackType()
    {
        return attackType;
    }
}
