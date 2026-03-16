using UnityEngine;

namespace VRG.ChapterFramework.Core
{
    public abstract class ChapterBase : MonoBehaviour
    {
        public abstract void Begin(ChapterData chapterData);

        public abstract void Complete();

        public abstract void ForceReset();

        public abstract void ForceComplete();

        public abstract void BeginNextPhase();

        public abstract void BeginPhase(int phaseIndex, int milestoneIndex = 0);
    }
}