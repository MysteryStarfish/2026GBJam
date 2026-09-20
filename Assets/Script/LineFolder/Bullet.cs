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