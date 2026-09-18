using System;
using System.Collections.Generic;
using Unity.U2D.Physics;
using UnityEngine;

namespace Script.Line
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] private Vector2 boxSize;
        [SerializeField] private Transform checkPoint;
        [SerializeField] private ContactFilter2D lazerFilter;
        private bool IsDetected { get; set; }
        private bool wasDetected;
        
        private readonly List<Collider2D> hitResults = new List<Collider2D>(10);
        
        public void DetectedAndHit()
        {
            int hitCount = Physics2D.OverlapBox(checkPoint.position, boxSize, 0f, lazerFilter, hitResults);
            IsDetected = hitCount > 0;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitResults[i];
                var target = hit.GetComponentInParent<LaserHitable>();

                target?.OnHit();
            }
            
            wasDetected = IsDetected;
        }
        
        public void DetectedAndHit(Vector2 checkpoint)
        {
            int hitCount = Physics2D.OverlapBox(checkpoint, boxSize, 0f, lazerFilter, hitResults);
            IsDetected = hitCount > 0;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitResults[i];
                var target = hit.GetComponentInParent<LaserHitable>();

                target?.OnHit();
            }
            
            wasDetected = IsDetected;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsDetected ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, boxSize);
        }
    }
}