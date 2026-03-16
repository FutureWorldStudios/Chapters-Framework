using UnityEngine;

namespace VRG.ChapterFramework.Core
{
    public static class Utils
    {
        public static ChapterData GetChapterData(int chapterIndex = 0, int phaseIndex = 0, int milestoneIndex = 0)
        {
            ChapterData chapterData = new ChapterData
            {
                Index = chapterIndex,
                PhaseIndex = phaseIndex,
                MilestoneIndex = milestoneIndex
            };

            return chapterData;
        }
    }
}