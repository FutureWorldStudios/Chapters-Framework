using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace VRG.ChapterFramework.Core
{
    public class Milestone : ExecutableBehaviour
    {
        #region Events
        public Action OnComplete;
        #endregion

        protected Phase _phase;

        [HideInInspector] private List<ComponentEntity> _component = new List<ComponentEntity>();

        public List<ComponentEntity> Components => _component;

        protected virtual void Start()
        {
            ChaptersManager.OnChapterBegun += HandleChapterBegun;

            _phase = GetComponentInParent<Phase>();
        }

        protected virtual void OnDestroy()
        {
            ChaptersManager.OnChapterBegun -= HandleChapterBegun;
        }

        private void HandleChapterBegun(ChapterData data)
        {

            //Debug.Log($"[Force Reset] Current Chapter Index: {data.Index} | My Chapter Index: {_phase.ChapterIndex}");

            if (_phase != null)
            {
                if (_phase.ChapterIndex != data.Index)
                {
                    //Manage Calculation for resetting milestones in previous phases
                    ForceReset();
                    //Debug.Log("[private] Force Reset on " + gameObject.name);
                }
                else { }
                //Debug.Log("[private] NO Force Reset on " + gameObject.name);

            }

        }

        public void RegisterComponent(ComponentEntity component)
        {
            if (!_component.Contains(component))
            {
                _component.Add(component);
            }
        }

        public override void Begin() //User driven
        {
            base.Begin();
        }

        public override void Complete() //User driven
        {
            base.Complete();
            Debug.Log($"[check] {gameObject.name} Completed");
            OnComplete?.Invoke();
        }

        public override void ForceCompletion() //Programmatic
        {
            base.ForceCompletion();
        }

        public override void ForceReset() //Programmatic
        {
            base.ForceReset();
        }

    }
}
