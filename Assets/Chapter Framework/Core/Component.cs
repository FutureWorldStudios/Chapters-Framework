using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;

namespace VRG.ChapterFramework
{
    public abstract class Component : MonoBehaviour
    {
        [SerializeField] private List<ComponentState> _componentState;

        public virtual void SetupForChapter(int chapterIndex)
        {
            OnApplyComponentState(chapterIndex);
        }

        protected abstract void OnApplyComponentState(int chapterIndex);
    }
}