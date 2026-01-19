using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;    

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

        
        [BoxGroup("Module Editor"), InlineButton("AddComponent"),
         ShowIf("@_module==Module.Chapter"), SerializeField]
        private string _chapterName;

        [BoxGroup("Module Editor"), InlineButton("AddPhase"),
         ShowIf("@_module==Module.Phase"), SerializeField]
        private string _phaseName;

        [BoxGroup("Module Editor"), InlineButton("AddMilestonePhase"),
        ShowIf("@_module==Module.MilestonePhase"), SerializeField]
        private string _milestonePhaseName;

        [BoxGroup("Module Editor"), InlineButton("AddMilestone"),
       ShowIf("@_module==Module.Phase"), SerializeField]
        private string _milestoneName;

        [BoxGroup("Module Editor"), InlineButton("AddComponent"),
         ShowIf("@_module==Module.Component"), SerializeField]
        private string _componentName;

        private void AddComponent()
        {
            UnityEditor.Selection.activeGameObject.TryGetComponent<Phase>(out Phase phase);
            UnityEditor.Selection.activeGameObject.TryGetComponent<Milestone>(out Milestone milestone);

            ChapterFrameworkEditor editor = new ChapterFrameworkEditor();

            if (phase != null)
                editor.AddComponentToPhase(_componentName);

            if(milestone != null)
                editor.AddComponentToMilestone(_componentName);
        }

        private void AddChapter()
        {
            UnityEditor.Selection.activeGameObject.TryGetComponent<ChaptersManager>(out ChaptersManager chManager);

            ChapterFrameworkEditor editor = new ChapterFrameworkEditor();

            if (chManager != null)
                editor.AddChapter(_chapterName);
        }

        private void AddPhase()
        {

        }

        private void AddMilestone()
        {

        }

        private void AddMilestonePhase()
        {
           
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
            window.Close();
        }
    }
}