using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace VRG.ChapterFramework.Editor
{ 
    public class EditorUtility 
    {
        private static string _coreBasePath = Path.Combine(Application.dataPath, "Chapter Framework", "Core", "Bases");
        private const string _appManagerName = "AppManager.cs";

        [MenuItem("Chapter Framework/Refresh Scene IDs")]
        private static void RefreshSceneIDs()
        {
            string fileData = ReadFile(Path.Combine(_coreBasePath,_appManagerName));

            List<string> sceneNames = new List<string>();


            string filePath = Path.Combine(_coreBasePath, _appManagerName);

            List<string> scenes = GetSceneNamesFromBuildSettings();

            fileData = InsertSceneIds(fileData, scenes);

            File.WriteAllText(filePath, fileData);

            AssetDatabase.Refresh();


            File.WriteAllText(filePath, fileData);

            AssetDatabase.Refresh();

        }

        private static string InsertSceneIds(string fileContent, List<string> sceneNames)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("public enum SceneId");
            sb.AppendLine("    {");

            foreach (string scene in sceneNames)
            {
                sb.AppendLine("          " + scene + ",");
            }

            sb.AppendLine("    }");

            return Regex.Replace(
                fileContent,
                @"public\s+enum\s+SceneId\s*\{[\s\S]*?\}",
                sb.ToString()
            );
        }

        private static string ReadFile(string path)
        {
            Debug.Log("Reading file from path: " + path);
            try
            {
                return File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to read file: " + e.Message);
                return string.Empty;
            }
        }
        public static List<string> GetSceneNamesFromBuildSettings()
        {
            List<string> sceneNames = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) 
                {
                    string name = Path.GetFileNameWithoutExtension(scene.path);
                    sceneNames.Add(name);
                }
            }
            return sceneNames;
        }
    }
}