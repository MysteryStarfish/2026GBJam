using UnityEngine;

namespace Script
{
    public class HealthShower : MonoBehaviour
    {
        [SerializeField] public RectTransform HealthSlide;
        [SerializeField] public RectTransform[] HealthPosition;

        public void UpdateHealthShow(int currentHealth)
        {
            var show = 3 - currentHealth;
            if (show > 3)  show = 3;
            if (show < 0) show = 0;
            HealthSlide.position = HealthPosition[show].position;
        }
    }
}