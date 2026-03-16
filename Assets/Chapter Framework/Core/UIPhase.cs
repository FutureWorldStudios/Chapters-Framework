using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.UI;

namespace VRG.ChapterFramework.Core
{
    public class UIPhase : Phase
    {
        [SerializeField] private bool _hideAtStart = true;
        [SerializeField] private CanvasGroup _canvasGroup;
        [BoxGroup("Base Narration Setup"), SerializeField] private bool _playNarrationAtStart;

        [BoxGroup("Base Narration Setup"), ShowIf("_playNarrationAtStart"), SerializeField] public AudioClip _narrationClip;
        [BoxGroup("Base Narration Setup"), ShowIf("_playNarrationAtStart"), SerializeField] public UnityEvent NarationBegun;
        [BoxGroup("Base Narration Setup"), ShowIf("_playNarrationAtStart"), SerializeField] public UnityEvent NarationComplete;

        protected override void Start()
        {
            base.Start();
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
            //Debug.Log("[vivek] Hiding Canvas Group called on " + gameObject.name);
            if (_canvasGroup != null)
            {
                _canvasGroup.FadeOut();
            }
        }

        #region Core - Base Methods
        public override void Begin()
        {
            base.Begin();
            ShowCanvasGroup();

            if (_playNarrationAtStart && _narrationClip != null)
            {
                NarationBegun?.Invoke();
            }
        }

        public override void Complete()
        {
            base.Complete();
            HideCanvasGroup();
        }

        public override void ForceReset()
        {
            base.ForceReset();
            if (_hideAtStart)
                HideCanvasGroup();
        }

        public override void ForceCompletion()
        {
            base.ForceCompletion();
            HideCanvasGroup();


        }

        protected virtual void OnNarrationEnded()
        {
            NarationComplete?.Invoke();
        }
        #endregion
    }
}