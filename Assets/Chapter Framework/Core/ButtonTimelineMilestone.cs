using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using System.Threading;
using UnityEngine.UI;
using System.Collections;

namespace VRG.ChapterFramework.Core
{
    public enum TimelineMode
    {
        Play,
        Pause,
        Stop
    }
    public class ButtonTimelineMilestone : Milestone
    {
        [BoxGroup("Timeline Milestone - Base Setup"), SerializeField] private FWSRayButton _button;
        [BoxGroup("Timeline Milestone - Base Setup"), SerializeField] private PlayableDirector _director;
        [BoxGroup("Timeline Milestone - Base Setup"), SerializeField] private AudioClip _voiceOver;

        private Coroutine _buttonShowRoutine;

        private bool _cancelled = false;

        protected override void Start()
        {
            base.Start();
            _button.ButtonElement.onClick.AddListener(HandleClick);
            HideButton(animateBtn: false);
            _director.stopped += HandleTimelineComplete;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _button.ButtonElement.onClick.RemoveListener(HandleClick);
            _director.stopped -= HandleTimelineComplete;
        }

        #region Core - Base Methods
        public override void Begin()
        {
            base.Begin();
            if (_voiceOver != null)
            {
                DelayShowButton(delay: (int)_voiceOver.length - 1);
                _cancelled = false;
            }
            else
            {
                DelayShowButton(delay: 2);
            }
        }

        public override void Complete()
        {
            base.Complete();


            HideButton();
            CancelButtonRoutine();
        }

        public override void ForceReset()
        {

            base.ForceReset();
            HideButton();
            CancelButtonRoutine();
            ManageTimeline(TimelineMode.Stop);
            //ProcedureVoiceOver.Instance.Stop();    
        }

        #endregion
        private void ShowButton()
        {
            _button.ShowButton();
        }

        private void HideButton(bool animateBtn = true)
        {
            _button.HideButton(animate: animateBtn);   
        }

        [Button]
        public void HandleClick()
        {
            
            ManageTimeline(TimelineMode.Play);  

            HideButton();
        }

        private void ManageTimeline(TimelineMode mode)
        {
            if(_director == null)
                return;

            switch (mode)
            {
                case TimelineMode.Play:
                    _director.Play();
                    break;
                case TimelineMode.Pause:
                    _director.Pause();
                    break;
                case TimelineMode.Stop:

                    _cancelled = true;
                    CancelButtonRoutine();

                    if (_director.playableGraph.IsValid())
                        _director.playableGraph.Stop();
                
                    _director.Stop();
                    _director.time = 0;
                    _director.Evaluate();
                    break;
            }

        }

        private void CancelButtonRoutine()
        {
            if (_buttonShowRoutine != null)
            {
                StopCoroutine(_buttonShowRoutine);
            }
        }

        private void DelayShowButton(int delay = 2)
        {
            CancelButtonRoutine();

            _buttonShowRoutine = StartCoroutine(DelayShowButtonRoutine(delay));
        }



        IEnumerator DelayShowButtonRoutine(int delay = 2)
        {
            yield return new WaitForSeconds(delay);
            ShowButton();
        }


        [Button]
        private void StopTimeline()
        {
            ManageTimeline(TimelineMode.Stop);
           
        }

        private void HandleTimelineComplete(PlayableDirector director)
        {
            //Debug.Log("[LOG] " + director.state);

            if (!_cancelled)
            {
                Complete();
            }
            else
                _cancelled = false;
        }

    }
}