using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace VRG.ChapterFramework
{
    public abstract class ComponentEntity : MonoBehaviour
    {
        [SerializeReference] // IMPORTANT: Unity polymorphic serialization
        [ListDrawerSettings(Expanded = true)]
        [HideReferenceObjectPicker]  private List<IComponentState> _componentState;

        public virtual void SetupForChapter(ChapterData chapterData)
        {
            OnApplyComponentState(chapterData);
        }

        protected abstract void OnApplyComponentState(ChapterData chapterData);
    }
}