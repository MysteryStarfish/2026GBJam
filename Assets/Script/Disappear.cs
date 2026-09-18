using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Disappear : MonoBehaviour
    {
        [SerializeField] PlayerInputReader playerInput;
        [SerializeField] float flashInterval;
        
        private CanvasGroup _canvasGroup;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        void OnEnable()
        {
            playerInput.OnTest += playDisappear;
        }

        void OnDisable()
        {
            playerInput.OnTest -= playDisappear;
        }

        void playDisappear()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, flashInterval));
            sequence.Append(_canvasGroup.DOFade(0f, flashInterval)
                .SetEase(Ease.Linear));
        }
    }
}