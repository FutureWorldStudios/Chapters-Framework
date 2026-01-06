using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRG.ChapterFramework
{
    public class Chapter : MonoBehaviour
    {
        #region Protected Properties
        [SerializeField] protected List<Phase> _phases = new List<Phase>(); 
        #endregion

        #region Private Properties
        private Phase _currentPhase; 
        #endregion

        #region Events
        //Begin
        //Complete
        //ForceReset -> If previous chapter is replayed.

        public Action OnComplete;
        public Action OnForceReset;
        public Action OnBegun; 
        #endregion

        #region Unity Methods
        private void Start()
        {
           
        }

        private void OnDestroy()
        {
            UnRegisterPhaseEvents();
        } 
        #endregion

        #region Public Methods
        public virtual void Begin()
        {
            OnBegun?.Invoke();

            //Start the first phase
        }

        public virtual void Complete()
        {
            OnComplete?.Invoke();

            //Report to some Chapter Manager that this chapter is complete
        }

        public virtual void ForceReset()
        {
            OnForceReset?.Invoke();

            //Reset all phases
        }

        public void AddPhase(Phase phase)
        {
            if (!_phases.Contains(phase))
            {
                _phases.Add(phase);
            }
        }
        #endregion

        #region Private Methods
        private void RegisterPhaseEvents()
        {
            foreach (var phase in _phases)
            {
                phase.OnComplete += HandlePhaseCompletion;
            }
        }

        private void UnRegisterPhaseEvents()
        {
            foreach (var phase in _phases)
            {
                phase.OnComplete += HandlePhaseCompletion;
            }
        }

        private void HandlePhaseCompletion()
        {

        } 
        #endregion
    }
}