using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace VRG.ChapterFramework.Core
{
    public class Chapter : ChapterBase

    {
        #region Protected Properties
        [BoxGroup("Chapters Setup"), SerializeField] protected List<Phase> _phases = new List<Phase>();
        [BoxGroup("Chapters Setup"), SerializeField] private POV _userPOV;
        #endregion

        #region Private Properties

        [SerializeField] private bool _debug;
        [ShowIf("_debug"), SerializeField] private Phase _currentPhase;
        [ShowIf("_debug"), SerializeField] private int _currentPhaseIndex;
        [ShowIf("_debug"), SerializeField] private bool _alreadyRegisteredPhaseEvents = false;
        [ShowIf("_debug"), SerializeField] private int _index;

        #endregion

        public Phase CurrentPhase => _currentPhase;

        public string ChapterName;

        #region Events
        //Begin
        //Complete
        //ForceReset -> If previous chapter is replayed.

        public Action OnComplete;
        public Action OnForceReset;
        public Action OnBegun;
        #endregion

        #region Unity Methods
        protected virtual void Start()
        {
            _index = transform.GetSiblingIndex();
        }

        protected void OnDestroy()
        {
            UnRegisterPhaseEvents();
        }
        #endregion

        #region Public Methods

        #region Core Methods

        #region deprecated
        //public void Begin()
        //{
        //    BeginFirstPhase();
        //    RegisterPhaseEvents();

        //    if(_userPOV != POV.None)
        //        FWSPlayerController.Instance.SetPOV(_userPOV);

        //    OnBegun?.Invoke();
        //}

        //public  void Complete()
        //{
        //    OnComplete?.Invoke();
        //    UnRegisterPhaseEvents();
        //}

        //public  void ForceReset()
        //{
        //    OnForceReset?.Invoke();

        //    //Reset all phases
        //}  
        #endregion

        public override void Begin(ChapterData chapterData)
        {
            ResetAllPhases();
            BeginPhase(chapterData.PhaseIndex, chapterData.MilestoneIndex);
            RegisterPhaseEvents();

            FWSPlayerController.Instance.SetPOV(_userPOV);

            OnBegun?.Invoke();
        }

        public override void Complete()
        {
            UnRegisterPhaseEvents();
            OnComplete?.Invoke();
        }

        public override void ForceReset()
        {
            UnRegisterPhaseEvents();

            OnForceReset?.Invoke();
        }

        public override void ForceComplete()
        {
            UnRegisterPhaseEvents();
        }

        public override void BeginNextPhase()
        {
            if (_phases.Count == 0)
                return;

            _currentPhaseIndex++;
            _currentPhase = _phases[_currentPhaseIndex];

            if (_currentPhase is MilestonePhase)
                _currentPhase.Begin(0);
            else
                _currentPhase.Begin();

            BroadcastPhaseChange();
        }

        public override void BeginPhase(int phaseIndex = 0, int milestoneIndex = 0)
        {
            if (_phases.Count == 0)
                return;

            _currentPhaseIndex = phaseIndex;
            _currentPhase = _phases[phaseIndex];

            if (_currentPhase is MilestonePhase)
                _currentPhase.Begin(milestoneIndex);
            else
                _currentPhase.Begin();
        }
        #endregion

        public void RegisterPhase(Phase phase)
        {
            if (!_phases.Contains(phase))
            {
                _phases.Add(phase);
            }
        }

        public int GetPhaseIndex(Phase phase)
        {
            for (int i = 0; i < _phases.Count; i++)
            {
                if (_phases[i] == phase)
                    return i;
            }
            return -1;
        }
        #endregion

        #region Private Methods

        private void BroadcastPhaseChange()
        {
            ChapterData chapterData = Utils.GetChapterData(_index, _currentPhaseIndex, 0);

            ChaptersManager.OnChapterBegun(chapterData);
        }

        private void RegisterPhaseEvents()
        {
            if (_alreadyRegisteredPhaseEvents)
                return;

            //Debug.Log($"mRegistering Phase Events for Chapter: {gameObject.name}");
            foreach (var phase in _phases)
            {
                phase.OnComplete += HandlePhaseCompletion;
            }

            _alreadyRegisteredPhaseEvents = true;
        }

        private void UnRegisterPhaseEvents()
        {
            //Debug.Log($"[Chapter] Unregistering Phase Events for Chapter: {gameObject.name}");
            foreach (var phase in _phases)
            {
                phase.OnComplete -= HandlePhaseCompletion;
            }

            _alreadyRegisteredPhaseEvents = false;
        }

        private void ResetAllPhases()
        {
            foreach (Phase phase in _phases)
                phase.ForceReset();
        }

        private void HandlePhaseCompletion(int index)
        {
            if (index == _phases.Count - 1 && ChaptersManager.Instance.ActiveChapter == this)
            {
                Debug.Log("[MyChapter] Phases Completed. Chapter complete: " + _currentPhase.gameObject.name);
                Complete();
            }
            else
            {
                BeginNextPhase();
            }
        }



        #endregion
    }
}