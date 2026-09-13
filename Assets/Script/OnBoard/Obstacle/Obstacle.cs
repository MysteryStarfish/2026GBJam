using UnityEngine;

public class Obstacle : MonoBehaviour, IOnBoardable
{
    public EntityType GetEntityType()
    {
        return EntityType.Obstacle;
    }
    public void OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, IOnBoardable other)
    {
        
    }
}
