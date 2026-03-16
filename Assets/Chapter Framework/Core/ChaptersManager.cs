using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRG.ChapterFramework.Core
{
    public class ChaptersManager : MonoBehaviour
    {
        [SerializeField] private List<Chapter> chapters = new List<Chapter>();

        [BoxGroup("Debug"), ShowIf("_debug"), SerializeField] private int _currentChapterIndex = -1;
        [BoxGroup("Debug"), ShowIf("_debug"), SerializeField] private ChapterData _currentChapterData;
        [BoxGroup("Debug"), ShowIf("_debug"), SerializeField] private Chapter _currentChapter;

        [BoxGroup("Debug"), SerializeField] private bool _debug;
        [BoxGroup("Debug"), ShowIf("_debug"), SerializeField] private bool _runPhaseStatus;

        public int GetChapterCount() { return chapters.Count; }
        public int ActiveChapterIndex => _currentChapterIndex;
        public bool RunPhaseActive => _runPhaseStatus;
        public Chapter ActiveChapter => _currentChapter;
        public static event Action OnAllChaptersCompleted;
        public static Action<ChapterData> OnChapterBegun;
        public static Action<bool> OnChapterBegunHideButtons;
        public static ChaptersManager Instance;

        public List<string> GetChapterNames()
        {
            List<string> names = new List<string>();

            foreach (Chapter chapter in chapters)
            {
                names.Add(chapter.ChapterName);
            }

            return names;
        }

        public ChapterData GetChapterData() { return _currentChapterData; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            RegisterChapterEvents();

            _currentChapterData.Index = -1;

        }

        private void OnDestroy()
        {
            UnRegisterChapterEvents();
        }

        public void RegisterChapter(Chapter chapter)
        {
            if (!chapters.Contains(chapter))
            {
                chapters.Add(chapter);
            }
        }

        public void BeginChapter(ChapterData chapterData)
        {
            if (chapterData.Index >= 0 && chapterData.Index < chapters.Count)
            {

                _currentChapterData = chapterData;

                _currentChapterIndex = _currentChapterData.Index;
                _currentChapter = chapters[_currentChapterData.Index];

                OnChapterBegun?.Invoke(_currentChapterData);

                if (chapterData.Index != 0)
                    ForceCompleteChapters(chapterData.Index - 1);

                if (chapterData.Index < chapters.Count)
                    ForceResetChapters(chapterData.Index);

                _currentChapter.Begin(_currentChapterData);
            }
        }

        private void ForceCompleteChapters(int maxIndex)
        {
            for (int i = 0; i < maxIndex; i++)
            {
                chapters[i].ForceComplete();
            }

        }

        private void ForceResetChapters(int fromIndex)
        {
            for (int i = fromIndex; i < chapters.Count; i++)
            {
                chapters[i].ForceReset();
            }
        }



        public void GoTo(ChapterData chapterData)
        {
            BeginChapter(chapterData);
        }

        public int GetCurrentMilestoneIndex()
        {
            if (_currentChapter.CurrentPhase is MilestonePhase)
            {
                MilestonePhase mPhase = _currentChapter.CurrentPhase as MilestonePhase;
                return mPhase.CurrentMilestoneIndex;
            }

            return -1;
        }

        private void RegisterChapterEvents()
        {
            foreach (Chapter chapter in chapters)
            {
                chapter.OnComplete += HandleChapterComplete;
            }
        }

        private void UnRegisterChapterEvents()
        {
            foreach (Chapter chapter in chapters)
            {
                chapter.OnComplete -= HandleChapterComplete;
            }
        }

        private void HandleChapterComplete()
        {
            Debug.Log("[MyChapter] Chapter Completed. Proceeding to next chapter.");
            BeginNextChapter();
        }

        private void BeginNextChapter()
        {
            if (_currentChapterIndex + 1 < chapters.Count)
            {
                ChapterData chapterData = Utils.GetChapterData(chapterIndex: _currentChapterIndex + 1);
                BeginChapter(chapterData);
            }
            else
            {
                OnAllChaptersCompleted?.Invoke();
            }
        }

        public void SetRunPhaseStatus(bool active) // Log here to understand where it's getting set.
        {
            _runPhaseStatus = active;
        }

        internal ChapterData GetChapterDataByID(int v)
        {
            throw new NotImplementedException();
        }
    }
}