using System.Collections.Generic;
using UnityEngine;

namespace VRG.ChapterFramework
{
    public class MilestonePhase : Phase
    {
        #region Inspector Properties
        [SerializeField] private List<Milestone> _milestones;
        #endregion

        #region Private Properties
        #endregion

        #region Unity Methods
        protected virtual void Start()
        {
        }
        #endregion

        #region Public Methods

        public void RegisterMilestone(Milestone milestone)
        {
            if(_milestones == null)
            {
                _milestones = new List<Milestone>();
            }

            if (!_milestones.Contains(milestone))
            {
                _milestones.Add(milestone);
            }
        }   

        #endregion
    }
}