using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRG.ChapterFramework.Core
{
    public class Phase : MonoBehaviour
    {
        #region Events
        public Action<int> OnComplete;
        #endregion

        [BoxGroup("IDs"), SerializeField] private int _currentChapterIndex;
        [BoxGroup("IDs"), SerializeField] private int _chapterIndex;
        [BoxGroup("IDs"), SerializeField] private int _index;

        [SerializeField] private List<ComponentEntity> _components = new List<ComponentEntity>();

        public int ChapterIndex => _chapterIndex;
        public int Index => _index;

        private ChapterData _currentChapterData;
        private Chapter _chapter;

        #region Unity Methods
        protected virtual void Start()
        {
            _components = transform.GetComponentsInChildren<ComponentEntity>(true).ToList();

            ChaptersManager.OnChapterBegun += HandleChapterBegun;

            _chapter = transform.GetComponentInParent<Chapter>() != null ? transform.GetComponentInParent<Chapter>() : null;

            _chapterIndex = _chapter != null ? _chapter.transform.GetSiblingIndex() : -1;

            _index = _chapter != null ? _chapter.GetPhaseIndex(this) : -1;
        }

        protected virtual void OnDestroy()
        {
            ChaptersManager.OnChapterBegun -= HandleChapterBegun;
        }
        #endregion

        protected virtual void HandleChapterBegun(ChapterData chapter)
        {
            if (_currentChapterIndex == -1)
                return;

            if (chapter.Index < _currentChapterIndex || (chapter.Index == 0 && _currentChapterIndex == 0))
            {
                if (chapter.PhaseIndex != _index || chapter.Index != _currentChapterIndex)
                {
                    ForceReset();
                }
            }
            else if (chapter.Index > _currentChapterIndex ||
                (chapter.Index == _currentChapterIndex && chapter.PhaseIndex > _index))
            {
                ForceCompletion();
            }
            _currentChapterIndex = chapter.Index;

            _currentChapterData = chapter;

            SetupComponents();
        }

        public void SetupComponents()
        {
            if (_components == null)
                return;

            foreach (var component in _components)
            {
                if (component != null)
                    component.SetupForChapter(_currentChapterData);
            }
        }

        #region Public Methods

        #region Core Methods

        //Recommended to be overridden in MilestonePhase class only.
        public virtual void Begin(int milestoneIndex = 0)
        {

        }

        public virtual void Begin()
        {

        }

        public virtual void Complete()
        {

            OnComplete?.Invoke(_index);
        }

        public virtual void ForceReset()
        {

        }

        public virtual void ForceCompletion()
        {

        }

        #endregion

        public void RegisterComponent(ComponentEntity component)
        {
            if (!_components.Contains(component))
            {
                _components.Add(component);
            }
        }

        #endregion
    }
}
