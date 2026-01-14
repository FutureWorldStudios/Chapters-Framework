using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;   
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace VRG.ChapterFramework.Editor
{
    public class MyCustomEditorWindow : OdinEditorWindow
    {
        private const string _fileName = "ChapterConfiguration.json";
        private string _directoryPath = Path.Combine(Application.dataPath, "Chapter Framework", "Engine");
        private string _scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");

        private const string _chapterTemplateName = "ChapterTemplate.cs.txt";
        private const string _phaseTemplateName = "PhaseTemplate.cs.txt";
        private const string _milestonePhaseTemplateName = "MilestonePhaseTemplate.cs.txt";
        private const string _milestoneTemplateName = "MilestoneTemplate.cs.txt";

        [BoxGroup("Control Panel"), HideLabel, EnumToggleButtons, OnValueChanged("LoadConfiguration")]
        public ChapterWindowMenu Menu;

        [ShowIf("Menu", ChapterWindowMenu.Configuration), BoxGroup("Control Panel")]
        public Chapters ChapterConfig;

        public enum ChapterWindowMenu
        {
            Edit,
            Configuration
        }

        #region Private Static Methods
        [MenuItem("My Game/My Editor")]
        private static void OpenWindow()
        {
            EditorWindow.GetWindow(typeof(MyCustomEditorWindow));

            MyCustomEditorWindow window = GetWindow<MyCustomEditorWindow>();
            window.titleContent = new GUIContent("Chapters Framework");
            window.Show();
        } 
        #endregion

        #region Unity Methods
        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();

            GUILayout.FlexibleSpace();

            GUI.enabled = (Menu == ChapterWindowMenu.Configuration);
            //if (SirenixEditorGUI.ToolbarButton("Save"))
            //{
            //    SaveConfiguration();
            //}
            //if (SirenixEditorGUI.ToolbarButton("Load"))
            //{
            //    LoadConfiguration();
            //}
            if (SirenixEditorGUI.ToolbarButton("Create Setup Now"))
            {
                GenerateScripts();
            }

            GUI.enabled = true;

            SirenixEditorGUI.EndHorizontalToolbar();
        }
        #endregion

        #region Private Methods

        private void GenerateScripts()
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");    

            string chapterTemplate = ReadFile(Path.Combine(templatesPath, _chapterTemplateName));
            string phaseTemplate = ReadFile(Path.Combine(templatesPath, _phaseTemplateName));
            string milestonePhaseTemplate = ReadFile(Path.Combine(templatesPath, _milestonePhaseTemplateName));
            string milestoneTemplate = ReadFile(Path.Combine(templatesPath, _milestoneTemplateName));

            Chapters chapters = ChapterConfig;

            if (chapters != null)
            {
                foreach (var chapterConfig in ChapterConfig.Data)
                {
                    if (chapterConfig != null)
                    {
                        string chapterContent = ReplaceClassName(chapterTemplate, chapterConfig.ChapterName);
                        CreateClassFile(scriptsPath, chapterConfig.ChapterName, chapterContent);

                        if (chapterConfig.Phases.Count > 0)
                        {
                            foreach (PhaseConfig phaseConfig in chapterConfig.Phases)
                            {
                                if (phaseConfig.UseMilestones)
                                {
                                    string milestonePhaseContent = ReplaceMilestonePhaseName(milestonePhaseTemplate, phaseConfig.PhaseName);
                                    CreateClassFile(scriptsPath, phaseConfig.PhaseName, milestonePhaseContent);
                                    if (phaseConfig.Milestones.Count > 0)
                                    {
                                        foreach (string milestoneName in phaseConfig.Milestones)
                                        {
                                            string milestoneContent = ReplaceMilestoneName(milestoneTemplate, milestoneName);
                                            CreateClassFile(scriptsPath, milestoneName, milestoneContent);
                                        }
                                    }
                                }
                                else
                                {
                                    string phaseContent = ReplacePhaseName(phaseTemplate, phaseConfig.PhaseName);
                                    CreateClassFile(scriptsPath, phaseConfig.PhaseName, phaseContent);
                                }
                            }
                        }
                    }
                }
            }
        }

        private string ReplaceClassName(string template, string className)
        {
            return template.Replace("#CHAPTER_NAME#", className.Replace(" ", ""));
        }

        private string ReplacePhaseName(string template, string phaseName)
        {
            return template.Replace("#PHASE_NAME#", phaseName.Replace(" ", ""));
        }

        private string ReplaceMilestoneName(string template, string milestoneName)
        {
            return template.Replace("#MILESTONE_NAME#", milestoneName.Replace(" ", ""));
        }

        private string ReplaceMilestonePhaseName(string template, string phaseName)
        {
            return template.Replace("#MILESTONEPHASE_NAME#", phaseName.Replace(" ", ""));
        }

        private void SaveConfiguration()
        {
            if (Menu == ChapterWindowMenu.Configuration)
            {
                if (ChapterConfig.Data.Count > 0)
                {
                    string json = JsonUtility.ToJson(ChapterConfig);
                    SaveJsonFile(json);
                }
            }
        }

        private void LoadConfiguration()
        {
            string path = Path.Combine(_directoryPath, _fileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                if (json != string.Empty)
                {
                    ChapterConfig = JsonUtility.FromJson<Chapters>(json);
                    Debug.Log("Successfully loaded configuration from: " + path);
                }
            }
        }

        private string ReadFile(string path)
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

        void SaveJsonFile(string json)
        {
            string path = Path.Combine(_directoryPath, _fileName);

            try
            {
                File.WriteAllText(path, json);

                AssetDatabase.Refresh();
                Debug.Log("Successfully saved file to: " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save file: " + e.Message);
            }
        }

        private void CreateClassFile(string folderAssetPath, string className, string content)
        {
            string folderFullPath = Path.GetFullPath(folderAssetPath);

            if (!Directory.Exists(folderFullPath))
                Directory.CreateDirectory(folderFullPath);

            string classNameText = className.Replace(" ", "");

            string fileFullPath = Path.Combine(folderFullPath, classNameText + ".cs");

            try
            {
                File.WriteAllText(fileFullPath, content);

                string assetPath = folderAssetPath.TrimEnd('/') + "/" + classNameText + ".cs";
                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();

                Debug.Log("Successfully created class file: " + assetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create class file: {e.Message}\nPath: {fileFullPath}");
            }
        }
        #endregion

    }

    [Serializable]
    public class Chapters
    {
        public List<ChapterConfig> Data;
    }

    [Serializable]
    public class ChapterConfig
    {
        public string ChapterName;

        [MinValue(0)]
        [OnValueChanged(nameof(SyncPhases))]
        public int PhaseCount;

        [ShowIf("@PhaseCount > 0")]
        public List<PhaseConfig> Phases = new();

        private void SyncPhases()
        {
            if (PhaseCount < 0)
                PhaseCount = 0;

            if (Phases == null)
                Phases = new List<PhaseConfig>();

            while (Phases.Count < PhaseCount)
            {
                Phases.Add(new PhaseConfig
                {
                    PhaseName = $"Phase {Phases.Count + 1}"
                });

            }
            while (Phases.Count > PhaseCount)
                Phases.RemoveAt(Phases.Count - 1);
        }
    }


    [Serializable]
    public class PhaseConfig
    {
        public string PhaseName;
        public bool UseMilestones;

        [ShowIf("UseMilestones")]
        [MinValue(0)]
        [OnValueChanged(nameof(SyncMilestones))]
        public int MilestoneCount;

        [ShowIf("@MilestoneCount > 0 && UseMilestones")]
        public List<string> Milestones;

        private void SyncMilestones()
        {
            if (MilestoneCount < 0)
                MilestoneCount = 0;

            if (Milestones == null)
                Milestones = new List<string>();

            while (Milestones.Count < MilestoneCount)
            {
                Milestones.Add($"Milestone {Milestones.Count + 1}");
            }
            while (Milestones.Count > MilestoneCount)
                Milestones.RemoveAt(Milestones.Count - 1);
        }

    }
}