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
        return bullets.Count > 0 ? bullets[0] : null;
    }
    
    public Bullet GetBullet(int idx)
    {
        if (idx >= 0 && idx < bullets.Count)
        {
            return bullets[idx];
        }
        return null;
    }
    public void UseBullet()
    {
        if (bullets.Count <= 0) return;
        bullets.RemoveAt(0);
    }

    public void Clear()
    {
        bullets.Clear();
    }

    public bool Empty => bullets.Count <= 0;
}