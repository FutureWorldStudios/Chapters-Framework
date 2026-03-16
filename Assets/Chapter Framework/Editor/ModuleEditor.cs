using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using VRG.ChapterFramework.Core;

namespace VRG.ChapterFramework.Editor
{
    public enum Module
    {
        Chapter,
        Phase,
        MilestonePhase,
        Milestone,
        Component
    }

    public class ModuleEditor : OdinEditorWindow
    {
        private static Module _module;

        [BoxGroup("Module Editor"), InlineButton(nameof(AddChapter)),
         ShowIf("@_module == Module.Chapter"), SerializeField]
        private string _chapterName;

        [BoxGroup("Module Editor"), InlineButton(nameof(AddPhase)),
         ShowIf("@_module == Module.Phase"), SerializeField]
        private string _phaseName;

        [BoxGroup("Module Editor"), InlineButton(nameof(AddMilestonePhase)),
         ShowIf("@_module == Module.MilestonePhase"), SerializeField]
        private string _milestonePhaseName;

        [BoxGroup("Module Editor"), ShowIf("@_module == Module.MilestonePhase")]
        [MinValue(0), OnValueChanged(nameof(SyncMilestones))]
        public int _milestoneCount;

        [BoxGroup("Module Editor"), ShowIf("@_module == Module.MilestonePhase && _milestoneCount > 0")]
        public List<string> _milestoneNames = new();

        [BoxGroup("Module Editor"), InlineButton(nameof(AddMilestone)),
         ShowIf("@_module == Module.Milestone"), SerializeField]
        private string _milestoneName;

        [BoxGroup("Module Editor"), InlineButton(nameof(AddComponent)),
         ShowIf("@_module == Module.Component"), SerializeField]
        private string _componentName;

        private void SyncMilestones()
        {
            if (_milestoneCount < 0)
                _milestoneCount = 0;

            _milestoneNames ??= new List<string>();

            while (_milestoneNames.Count < _milestoneCount)
                _milestoneNames.Add($"Milestone {_milestoneNames.Count + 1}");

            while (_milestoneNames.Count > _milestoneCount)
                _milestoneNames.RemoveAt(_milestoneNames.Count - 1);
        }

        private void AddComponent()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a Phase or Milestone.");
                return;
            }

            selected.TryGetComponent(out Phase phase);
            selected.TryGetComponent(out Milestone milestone);

            ChapterFrameworkEditor editor = CreateInstance<ChapterFrameworkEditor>();

            if (phase != null)
                editor.AddComponentToPhase(_componentName);
            else if (milestone != null)
                editor.AddComponentToMilestone(_componentName);
            else
                Debug.LogWarning("Selected object must have Phase or Milestone.");
        }

        private void AddChapter()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select ChaptersManager.");
                return;
            }

            selected.TryGetComponent(out ChaptersManager chManager);

            ChapterFrameworkEditor editor = CreateInstance<ChapterFrameworkEditor>();

            if (chManager != null)
                editor.AddChapter(_chapterName);
            else
                Debug.LogWarning("Selected object must have ChaptersManager.");
        }

        private void AddPhase()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a Chapter.");
                return;
            }

            selected.TryGetComponent(out Chapter chapter);

            ChapterFrameworkEditor editor = CreateInstance<ChapterFrameworkEditor>();

            if (chapter != null)
                editor.AddPhase(_phaseName);
            else
                Debug.LogWarning("Selected object must have Chapter.");
        }

        private void AddMilestone()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a MilestonePhase.");
                return;
            }

            MilestonePhase milestonePhase = selected.GetComponent<MilestonePhase>();
            ChapterFrameworkEditor editor = CreateInstance<ChapterFrameworkEditor>();

            if (milestonePhase != null)
                editor.AddMilestone(_milestoneName);
            else
                Debug.LogWarning("Selected object must have MilestonePhase.");
        }

        private void AddMilestonePhase()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a Chapter.");
                return;
            }

            selected.TryGetComponent(out Chapter chapter);

            ChapterFrameworkEditor editor = CreateInstance<ChapterFrameworkEditor>();

            if (chapter != null)
                editor.AddMilestonePhase(_milestonePhaseName, _milestoneNames);
            else
                Debug.LogWarning("Selected object must have Chapter.");
        }

        public static void OpenWindow(Module module)
        {
            var window = GetWindow<ModuleEditor>();
            window.titleContent = new GUIContent("Module Editor");
            window.Show();
            _module = module;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var window = GetWindow<ModuleEditor>();
            if (window != null)
                window.Close();
        }
    }
}