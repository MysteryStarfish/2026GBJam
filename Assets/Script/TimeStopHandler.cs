using DG.Tweening;
using Script.Enemy;
using Script.OnBoard;
using Script.OnBoard.Player;
using UnityEngine;

namespace Script
{
    public class TimeStopHandler : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private Player _player;
        [SerializeField] private float delayTime;
        [SerializeField] private float laserDelayTime;
        [SerializeField] private LineFolder.Line line;
        [SerializeField] private TimeStopEffectController timeStopEffectController;
        [SerializeField] private EnergyGenerator energyGenerator;
        [SerializeField] private CameraHandler cameraHandler;
        private bool isTimeStopped = false;

        private void OnEnable()
        {
            _player.TimeStopEvent += ActivateStopTime;
            timeStopEffectController.StopFullScreenEvent += StopFullScreenEvent;
            timeStopEffectController.StopClearScreenEvent += StopClearScreen;
        }

        private void OnDisable()
        {
            _player.TimeStopEvent -= ActivateStopTime;
            timeStopEffectController.StopFullScreenEvent -= StopFullScreenEvent;
            timeStopEffectController.StopClearScreenEvent -= StopClearScreen;
        }

        private Tween _timeStopTween;

        private void ActivateStopTime()
        {
            StopTime();
    
            _timeStopTween?.Kill();
            
            _timeStopTween = DOVirtual.DelayedCall(delayTime, () =>
            {
                ResumeTime();
            }).SetLink(gameObject);
        }

        private void StopFullScreenEvent()
        {
            DOVirtual.DelayedCall(laserDelayTime, () =>
            {
                line.Activate();
            });
        }
        
        private void StopClearScreen()
        {
            if (GameState.Instance.IsLevelClear) return;
            energyGenerator.GenerateEnergy(6);
        }

        public void StopTime()
        {
            isTimeStopped = true;
            enemySpawner.StopEnemy();
            timeStopEffectController.PlayTimeStopEffect();
            _player.PlayerStop();
            cameraHandler.SwitchToCamera2();
        }

        public void ResumeTime()
        {
            isTimeStopped = false;
            enemySpawner.MoveEnemy();
            timeStopEffectController.PlayTimeMoveEffect();
            _player.PlayerCanMove();
            cameraHandler.SwitchToCamera1();
            line.ClearBullet();
        }

        public bool IsTimeStopped()
        {
            return isTimeStopped;
        }
    }
}