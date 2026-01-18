using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRG.ChapterFramework
{
    public class Phase : MonoBehaviour
    {
        #region Events
        public Action OnComplete;
        #endregion

        [SerializeField] private int _chapterIndex;


        [SerializeField] private List<Component> _components = new List<Component>();   

        #region Unity Methods
        private void Start()
        {
            _components = transform.GetComponentsInChildren<Component>(true).ToList();

            ChaptersManager.OnChapterBegun += HandleChapterBegun;

            _chapterIndex = transform.GetComponentInParent<Chapter>() != null 
                ? transform.GetComponentInParent<Chapter>().transform.GetSiblingIndex() : -1;
        }

        private void OnDestroy()
        {
            ChaptersManager.OnChapterBegun -= HandleChapterBegun;
        }
        #endregion

        private void HandleChapterBegun(int chapterIndex)
        {
            if (_chapterIndex == -1)
                return;

            SetupComponents();

            if (chapterIndex < _chapterIndex)
                ForceReset();
            else
                ForceCompletion();
        }

        private void SetupComponents()
        {
            foreach (var component in _components)
            {
                component.SetupForChapter(_chapterIndex);
            }
        }


        #region Public Methods
        public virtual void Begin()
        {

        }

        public virtual void Complete()
        {
            OnComplete?.Invoke();
        }

        public virtual void ForceReset()
        {

        } 

        public virtual void ForceCompletion()
        {

        }

        public void RegisterComponent(Component component)
        {
            if(!_components.Contains(component))
            {
                _components.Add(component);
            }
        }

        #endregion
    }
}