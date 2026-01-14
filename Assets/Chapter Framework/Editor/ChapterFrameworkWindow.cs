using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRG.ChapterFramework.Editor;
using VRG.ChapterFramework; 
using System;   

public class ChapterFrameworkWindow : EditorWindow
{

    [MenuItem("Chapters Framework/Create Chapters Setup")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ChapterFrameworkWindow));

        ChapterFrameworkWindow wnd = GetWindow<ChapterFrameworkWindow>();
        wnd.titleContent = new GUIContent("Chapters Framework");
    }

    public void CreateGUI()
    {
       
    }

    public void OnGUI()
    {
       
    }
}
