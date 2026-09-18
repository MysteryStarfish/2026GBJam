using System.Collections.Generic;

public class BulletSlot
{
    private BulletFactory bulletFactory = new();

    private List<Bullet> bullets = new List<Bullet>();
    public void GenerateBullet()
    {
        var bullet = BulletFactory.CreateRandomBullet();
        bullets.Add(bullet);
    }

    public Bullet GetBullet()
    {
        return bullets[0];
    }
    public Bullet GetBullet(int idx)
    {
        return bullets[idx];
    }
    public void UseBullet()
    {
        bullets.RemoveAt(0);
    }

    public void Clear()
    {
        bullets.Clear();
    }

    public bool Empty => bullets.Count <= 0;
}