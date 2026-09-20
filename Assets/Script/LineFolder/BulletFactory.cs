using System;
using UnityEngine;

public class BulletFactory
{
    private static Bullet CreateBullet(BulletType attackType)
    {
        return new Bullet(attackType);
    }
    public static Bullet CreateRandomBullet()
    {
        // var randomType = (BulletType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(BulletType)).Length);
        var randomType = (BulletType)0;
        Debug.Log(randomType);
        return CreateBullet(randomType);
    }
}