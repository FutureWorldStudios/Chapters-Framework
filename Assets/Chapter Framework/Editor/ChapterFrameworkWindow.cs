using UnityEditor;
using UnityEngine;

public class ChapterFrameworkWindow : EditorWindow
{
    [MenuItem("Chapters Framework/Create Chapters Setup")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ChapterFrameworkWindow));
    }

    void OnGUI()
    {
        // The actual window code goes here
    }
}
