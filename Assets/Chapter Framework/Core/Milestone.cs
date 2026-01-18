using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRG.ChapterFramework
{
    public class Milestone : MonoBehaviour
    {
        #region Events
        public Action OnComplete;
        #endregion

        private List<Component> _components = new List<Component>();

        public void RegisterComponent(Component component)
        {
            if (!_components.Contains(component))
            {
                _components.Add(component);
            }
        }
    }
}
