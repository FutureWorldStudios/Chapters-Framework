using UnityEngine;

namespace VRG.ChapterFramework
{
    public class UIPhase : Phase
    {
        [SerializeField] private bool _hideAtStart = true;
        [SerializeField] private CanvasGroup _canvasGroup;

        protected virtual void Start()
        {
            if (TryGetComponent(out _canvasGroup))
            {
                if (_hideAtStart)
                    _canvasGroup.Hide();
            }
        }

        private void ShowCanvasGroup()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.FadeIn();
            }
        }

        private void HideCanvasGroup()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.FadeOut();
            }
        }
    }
}