using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace VRG.ChapterFramework
{
    public abstract class Component : MonoBehaviour
    {
        public virtual void SetupForChapter(int chapterIndex)
        {
            OnApplyComponentState(chapterIndex);
        }

        protected abstract void OnApplyComponentState(int chapterIndex);
    }
}