using System;
using UnityEngine;

namespace Script.OnBoard.ObstacleFolder
{
    public class Obstacles : MonoBehaviour
    {
        [SerializeField] Obstacle[] obstacles;

        public void Open()
        {
            foreach (var obstacle in obstacles)
            {
                obstacle.Open();
            }
        }
        
        public void Close()
        {
            foreach (var obstacle in obstacles)
            {
                obstacle.Close();
            }
        }
    }
}