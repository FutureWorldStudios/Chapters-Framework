using System.Collections.Generic;
using UnityEngine;

namespace VRG.ChapterFramework.Core
{
    public class MilestonePhase : Phase
    {
        #region Inspector Properties
        [SerializeField] protected List<Milestone> _milestones;
        #endregion

        #region Private Properties
        [SerializeField] private int _currentMilestoneIndex = 0;
        #endregion

        public int CurrentMilestoneIndex => _currentMilestoneIndex;

        private bool _alreadyRegisteredEvents;

        #region Unity Methods
        protected override void Start()
        {
            base.Start();
            AddReferencedComponents();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterMilestoneEvents();
        }
        #endregion

        #region Public Methods

        public void RegisterMilestone(Milestone milestone)
        {
            //Debug.Log("[exec order mphase] RegisterMilestone");

            if (_milestones == null)
            {
                _milestones = new List<Milestone>();
            }

            if (!_milestones.Contains(milestone))
            {
                _milestones.Add(milestone);

                if (milestone.Components != null)
                {
                    foreach (var component in milestone.Components)
                    {
                        RegisterComponent(component);
                    }
                }
            }
        }

        #region Core - Base Methods
        public override void Begin(int milestoneIndex = 0)
        {
            base.Begin(milestoneIndex);
            RegisterMilestoneEvents();
            BeginMilestone(milestoneIndex);
        }

        public override void Complete()
        {
            base.Complete();
            UnregisterMilestoneEvents();
        }

        public override void ForceReset()
        {
            base.ForceReset();
            UnregisterMilestoneEvents();
        }

        public override void ForceCompletion()
        {
            base.ForceCompletion();
            //UnregisterMilestoneEvents();
        }
        #endregion

        #endregion

        private void RegisterMilestoneEvents()
        {
            if (_alreadyRegisteredEvents)
                return;

            //Debug.Log("[MilestonePhase] RegisterMilestoneEvents on " + gameObject.name);
            foreach (var milestone in _milestones)
            {
                milestone.OnComplete += HandleMilestoneComplete;
            }

            Debug.Log("[milestone_regsitry] registered milestone events on " + gameObject.name);

            _alreadyRegisteredEvents = true;
        }

        private void UnregisterMilestoneEvents()
        {
            ChapterData chapter = ChaptersManager.Instance.GetChapterData();

            Debug.Log("[milestone_regsitry] unregistered milestone events on " + gameObject.name);

            if (chapter.Index != ChapterIndex || chapter.PhaseIndex != Index)
            {
                //Debug.Log($"PhaseName: {gameObject.name} MyChapterIndex: {ChapterIndex}, NewChapterIndex: {chapter.Index}. " +
                //           $"MyPhaseIndex: {Index}, NewPhaseIndex: {chapter.PhaseIndex}");
                //Debug.Log("[MilestonePhase] UnregisterMilestoneEvents on " + gameObject.name);
                foreach (var milestone in _milestones)
                {
                    milestone.OnComplete -= HandleMilestoneComplete;
                }

                _alreadyRegisteredEvents = false;
            }
        }

        private void BeginMilestone(int milestoneIndex)
        {
            if (_milestones != null && _milestones.Count > milestoneIndex && milestoneIndex >= 0)
            {
                _currentMilestoneIndex = milestoneIndex;
                _milestones[_currentMilestoneIndex].Begin();

                //BroadcastMilestoneChange();
            }
        }

        private void BroadcastMilestoneChange()
        {
            ChapterData chapterData = Utils.GetChapterData(ChapterIndex, Index, _currentMilestoneIndex);

            ChaptersManager.OnChapterBegun(chapterData);
        }

        //private void BeginFirstMilestone()
        //{

        //    if (_milestones != null && _milestones.Count > 0)
        //    {
        //        _currentMilestoneIndex = 0;
        //        _milestones[_currentMilestoneIndex].Begin();
        //    }
        //}

        private void AddReferencedComponents()
        {

            foreach (var milestone in _milestones)
            {
                if (milestone.Components != null)
                {
                    foreach (var component in milestone.Components)
                    {
                        RegisterComponent(component);
                    }
                }
            }
        }

        public int GetIndexOfMilestone(Milestone milestone)
        {
            if (milestone == null)
                return -1;

            return _milestones.IndexOf(milestone);
        }

        protected virtual void HandleMilestoneComplete()
        {
            Debug.Log("Milestone complete");
            BeginNextMilestone();
            Debug.Log("[INDIA] Begin next milestone called on " + gameObject.name);
        }

        private void BeginNextMilestone()
        {

            if (_currentMilestoneIndex < _milestones.Count - 1)
            {
                //Debug.Log("Next milestone");

                _currentMilestoneIndex++;
                _milestones[_currentMilestoneIndex].Begin();
                BroadcastMilestoneChange();

                Debug.Log("[ch tracker] milestone change to " + _currentMilestoneIndex);
            }
            else
            {
                Complete();
                Debug.Log("[ch tracker] Phase Complete " + gameObject.name);
            }
        }

        public Milestone GetCurrentMilestone()
        {
            return _milestones[_currentMilestoneIndex];
        }

    }
}
