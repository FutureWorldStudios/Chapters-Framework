using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;    

namespace VRG.ChapterFramework.Editor
{
    public class ComponentEditor : OdinEditorWindow
    {

        [BoxGroup("Component Editor"), InlineButton("Add") , SerializeField]
        private string _componentName;

        private void Add()
        {
            UnityEditor.Selection.activeGameObject.TryGetComponent<Phase>(out Phase phase);
            UnityEditor.Selection.activeGameObject.TryGetComponent<Milestone>(out Milestone milestone);

            ChapterFrameworkEditor editor = new ChapterFrameworkEditor();

            if (phase != null)
                editor.AddComponentToPhase(_componentName);

            if(milestone != null)
                editor.AddComponentToMilestone(_componentName);
        }

       public static void OpenWindow()
       {
            var window = GetWindow<ComponentEditor>();
            window.titleContent = new GUIContent("Component Editor");
            window.Show();
       }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var window = GetWindow<ComponentEditor>();
            window.Close();
        }
    }
}