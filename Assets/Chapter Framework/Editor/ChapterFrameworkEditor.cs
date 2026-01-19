using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;  
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FWS;

namespace VRG.ChapterFramework.Editor
{
    public class ChapterFrameworkEditor : OdinEditorWindow
    {
        public enum ChapterWindowMenu
        {
            BaseSceneConfig,
            Configuration
        }

        #region Inspector Variables
        [BoxGroup("Control Panel"), HideLabel, EnumToggleButtons, OnValueChanged("LoadConfiguration")]
        public ChapterWindowMenu Menu;

        [ShowIf("Menu", ChapterWindowMenu.Configuration), BoxGroup("Control Panel")]
        public Chapters ChapterConfig;

        [ShowIf("Menu", ChapterWindowMenu.BaseSceneConfig), BoxGroup("Base Scene Configuration")]
        [ShowIf("Menu", ChapterWindowMenu.BaseSceneConfig),
         SerializeField, BoxGroup("Base Scene Configuration")] public int ApplicationFrameRate = 60;
        [ShowIf("Menu", ChapterWindowMenu.BaseSceneConfig), 
         SerializeField, BoxGroup("Base Scene Configuration")] public MsaaQuality MsaaQuality = MsaaQuality._2x;
        [ShowIf("Menu", ChapterWindowMenu.BaseSceneConfig), 
         SerializeField, BoxGroup("Base Scene Configuration")] public bool _boothMode = false;

        [ShowIf("_boothMode"), SerializeField, BoxGroup("Base Scene Configuration")] public double ResetTime = 60;
        #endregion


        #region Private Variables
        private const string _fileName = "ChapterConfiguration.json";
        private string _directoryPath = Path.Combine(Application.dataPath, "Chapter Framework", "Engine");
        private string _scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
        private string _coreBasePath = Path.Combine(Application.dataPath, "Chapter Framework", "Core", "Bases");

        private const string _chapterTemplateName = "ChapterTemplate.cs.txt";
        private const string _phaseTemplateName = "PhaseTemplate.cs.txt";
        private const string _milestonePhaseTemplateName = "MilestonePhaseTemplate.cs.txt";
        private const string _milestoneTemplateName = "MilestoneTemplate.cs.txt";
        private const string _componentTemplateName = "ComponentTemplate.cs.txt";

        private const string _framerateManagerName = "FramerateManager.cs";
        private const string _appManagerName = "AppManager.cs";


        private static bool _canAttachScripts;
        private static GameObject _chapterManagerObject;

        private static string AllowSetupKey = "AllowSetupKey";
        private static string ChaptersJSONKey = "ChaptersJSONKey";

        private static string AddComponentToMilestoneKey = "AddComponentToMilestoneKey";
        private static string AddComponentToPhaseKey = "AddComponentToPhaseKey";
        private static string AddChaptersKey = "AddChaptersKey";
        private static string AddPhasekey = "AddPhasekey";
        private static string AddMilestonePhaseKey = "AddMilestonePhaseKey";
        private static string AddMilestoneKey = "AddMilestoneKey";

        private static string ComponentTypeNameKey = "ComponentTypeNameKey";
        private static string ChapterNameKey = "ChapterNameKey";
        private static string PhaseNameKey = "PhaseNameKey";
        private static string MilestonePhaseNameKey = "MilestonePhaseNameKey";
        private static string MilestoneNameKey = "MilestoneNameKey"; 
        #endregion

        private static event Action<Chapters> OnGenerateHierarchy;


        #region Private Static Methods
        [MenuItem("Chapter Framework/Getting Started")]
        private static void OpenWindow()
        {
            EditorWindow.GetWindow(typeof(ChapterFrameworkEditor));

            ChapterFrameworkEditor window = GetWindow<ChapterFrameworkEditor>();
            window.titleContent = new GUIContent("Chapters Framework");
            window.Show();
        }

        [MenuItem("GameObject/Chapters Framework/Add New Default Component", false, 0)]
        private static void AddNewComponent()
        {
            LogSelectedTransformName();
            if (Selection.activeGameObject != null)
            {
                GameObject selectedObject = Selection.activeGameObject;

                if (selectedObject.GetComponent<Phase>() == null && 
                    selectedObject.GetComponent<Milestone>() == null)
                    return;

                GameObject newComponent = new GameObject("New Component");
                newComponent.transform.SetParent(selectedObject.transform);

                Debug.Log("Added new component GameObject under: " + selectedObject.name);

                Component component = newComponent.AddComponent<Component>();

                selectedObject.GetComponent<Phase>().RegisterComponent(component);
            }
            else
            {
                Debug.LogWarning("No GameObject selected. Please select a GameObject to add components.");
            }
        }

        [MenuItem("GameObject/Chapters Framework/Add New Component", false, 0)]
        private static void AddNewCustomComponent()
        {
            ModuleEditor.OpenWindow(Module.Component);   
        }

        [MenuItem("GameObject/Chapters Framework/Add New Chapter", false, 0)]
        private static void AddNewChapter()
        {
            ModuleEditor.OpenWindow(Module.Chapter);
        }

        [MenuItem("GameObject/Chapters Framework/Add New Phase", false, 0)]
        private static void AddNewPhase()
        {
            ModuleEditor.OpenWindow(Module.Phase);
        }

        [MenuItem("GameObject/Chapters Framework/Add New Milestone Phase", false, 0)]
        private static void AddNewMilestonePhase()
        {
            ModuleEditor.OpenWindow(Module.MilestonePhase);
        }

        [MenuItem("GameObject/Chapters Framework/Add New Milestone", false, 0)]
        private static void AddNewMilestone()
        {            
            ModuleEditor.OpenWindow(Module.Milestone);
        }

        static void LogSelectedTransformName()
        {
            if (Selection.activeGameObject != null)
            {
                Debug.Log("Selected object name: " + Selection.activeGameObject.name);
            }
        }
        #endregion

        #region Unity Methods
        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();

            GUILayout.FlexibleSpace();

            if (Menu == ChapterWindowMenu.Configuration)
            {
                if (SirenixEditorGUI.ToolbarButton("Save"))
                {
                    SaveConfiguration();
                }

                if (SirenixEditorGUI.ToolbarButton("Load"))
                {
                    LoadConfiguration();
                }

                if (SirenixEditorGUI.ToolbarButton("Create Setup Now"))
                {
                    Debug.Log("PRESSED");
                    SaveConfiguration();
                    GenerateScripts();
                }
            }
            else if(Menu == ChapterWindowMenu.BaseSceneConfig)
            {
                if (SirenixEditorGUI.ToolbarButton("Setup Base Scene"))
                {
                    SetupBaseScene();
                }
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
        }
        #endregion

        #region Private Methods

        private async void SetupBaseScene()
        {
            if(!CheckIfBaseConfigExists())
            {
                GameObject appManager = new GameObject("App Manager");
                appManager.AddComponent<AppManager>();

                if (ResetTime != 60)
                {
                    EditAppManager();
                }

                if (ApplicationFrameRate != 60 || MsaaQuality != MsaaQuality._2x)
                {
                    EditFramerateManager();
                }

                GameObject framerateManager = new GameObject("Framerate Manager");
                FramerateManager frManager = framerateManager.AddComponent<FramerateManager>();

                GameObject defaultCamera = GameObject.Find("Main Camera");

                if (defaultCamera != null)
                {
                    DestroyImmediate(defaultCamera);
                }

                GameObject ovrCamera = GameObject.Find("OVRCameraRig");
                if (ovrCamera == null)
                {
                    AddOVRCameraRigToScene();
                }
                
                await Task.Delay(1000);

                ovrCamera = GameObject.Find("OVRCameraRig");
                GameObject centerEye = ovrCamera.transform.GetChild(0).GetChild(1).gameObject;

                if (centerEye != null)
                {
                    DestroyImmediate(centerEye.GetComponent<OVRScreenFade>());
                    centerEye.AddComponent<FWS_OVRScreenFade>();
                }
            }
        }

        private void EditAppManager()
        {
            string filePath = Path.Combine(_coreBasePath, _appManagerName);

            string appManagerData = ReadFile(filePath);

            appManagerData = Regex.Replace(appManagerData, @"(\[SerializeField\]\s*private\s+double\s+_resetTime\s*=\s*)\d+(\s*;)", 
                                            m => $"{m.Groups[1].Value}{ResetTime}{m.Groups[2].Value}");

            File.WriteAllText(filePath, appManagerData);

            #if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

        }

        [MenuItem("GameObject/Chapters Framework/VR/Add OVRCameraRig")]
        public static void AddOVRCameraRigToScene()
        {
            // Find prefab by name
            string[] guids = AssetDatabase.FindAssets("OVRCameraRig t:Prefab");

            if (guids == null || guids.Length == 0)
            {
                Debug.LogError("OVRCameraRig prefab not found in project.");
                return;
            }

            // Load the prefab
            string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError("Failed to load OVRCameraRig prefab.");
                return;
            }

            // Instantiate into scene (keeps prefab connection)
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            instance.name = prefab.name;
            Undo.RegisterCreatedObjectUndo(instance, "Add OVRCameraRig");

            Selection.activeGameObject = instance;

            Debug.Log($"OVRCameraRig added to scene from: {prefabPath}");
        }

        private bool CheckIfBaseConfigExists()
        {
            if (FindObjectOfType<AppManager>() != null && FindObjectOfType<FramerateManager>() != null)
                return true;
            else
                return false;
        }

        private void EditFramerateManager()
        {
            string filePath = Path.Combine(_coreBasePath, _framerateManagerName);

            string framerateManagerData = ReadFile(filePath);

            framerateManagerData = Regex.Replace(
         framerateManagerData,
         @"(\[SerializeField\]\s*private\s+int\s+targetFramerate\s*=\s*)\d+(\s*;)",
         m => $"{m.Groups[1].Value}{ApplicationFrameRate}{m.Groups[2].Value}"
     );

            // 2) MsaaQuality initializer
            framerateManagerData = Regex.Replace(
                framerateManagerData,
                @"(\[SerializeField\]\s*private\s+MsaaQuality\s+quality\s*=\s*)MsaaQuality\.\w+(\s*;)",
                m => $"{m.Groups[1].Value}MsaaQuality.{MsaaQuality}{m.Groups[2].Value}"
            );

            File.WriteAllText(filePath, framerateManagerData);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private void LoadLocalConfiguration()
        {
            string localJSON = PlayerPrefs.GetString("ChaptersJSON");

            if (localJSON != string.Empty)
            {
                ChapterConfig = JsonUtility.FromJson<Chapters>(localJSON);
                Debug.Log("Successfully loaded local configuration.");
            }
        }

        private async void GenerateScripts()
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");    

            string chapterTemplate = ReadFile(Path.Combine(templatesPath, _chapterTemplateName));
            string phaseTemplate = ReadFile(Path.Combine(templatesPath, _phaseTemplateName));
            string milestonePhaseTemplate = ReadFile(Path.Combine(templatesPath, _milestonePhaseTemplateName));
            string milestoneTemplate = ReadFile(Path.Combine(templatesPath, _milestoneTemplateName));

            Chapters chapters = ChapterConfig;

           
            await Task.Delay(1000);

            if (chapters != null)
            {
                // Store config for the static reload callback
                string chaptersJson = JsonUtility.ToJson(ChapterConfig);
                SessionState.SetInt(AllowSetupKey, 1);
                SessionState.SetString(ChaptersJSONKey, chaptersJson);
                PlayerPrefs.SetString("ChaptersJSON", chaptersJson);

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

               
                OnGenerateHierarchy += AutoGenerateHierarchy;   
            }
        }

    private static Type GetTypeByName(string name)
    {
        name = name.Replace(" ", "");

        Type type = Type.GetType(name, throwOnError: false);

        if (type != null)
        {          
            return type;
        }
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                type = assembly.GetType(name, throwOnError: false, ignoreCase: false);
                if (type != null)
                {                   
                    return type;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GetTypeByName] Failed querying assembly '{assembly.GetName().Name}': {ex.Message}");
            }
        }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                    Debug.LogWarning($"[GetTypeByName] Partial type load in '{assembly.GetName().Name}'. Continuing with available types.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GetTypeByName] Skipping assembly '{assembly.GetName().Name}");
                    continue;
                }

                foreach (var t in types)
                {
                    if (t.Name == name)
                    {
                        Debug.LogWarning(
                            $"[GetTypeByName] FOUND via short-name match {t.FullName} "
                        );
                        return t;
                    }
                }
            }

            return null;
    }



    [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Debug.Log("Trying to auto generate, Allowed? " + PlayerPrefs.GetInt("AllowSetup"));
          
            if (SessionState.GetInt(AllowSetupKey, 0) == 1)
            {
                SessionState.SetInt(AllowSetupKey, 0);

                string json = SessionState.GetString(ChaptersJSONKey, "");

                Debug.Log("retrieved json: " + json);

                if (json != string.Empty)
                {
                    Chapters config = JsonUtility.FromJson<Chapters>(json);

                    Debug.Log("Trying to auto generate");

                    if(config != null)
                    AutoGenerateHierarchy(config);
                }
            }

            TryAutoAddComponentToPhase();

            _canAttachScripts = false;
        }

        private static GameObject GenerateAndAddComponent(string gameObjectName, Type type)
        {
            GameObject go = new GameObject(gameObjectName);
            go.AddComponent(type); 
            
            return go;
        }

        private static GameObject GenerateAndAddComponent(string gameObjectName, string typeName, Transform parent = null)
        {
            GameObject go = new GameObject(gameObjectName);
            Type type = GetTypeByName(typeName);

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            if (type == null) {
                Debug.Log(type);
                    }
            if (type != null)
                go.AddComponent(type);

            return go;  
        }

        private static async void AutoGenerateHierarchy(Chapters chapters)
        {
            OnGenerateHierarchy -= AutoGenerateHierarchy;

            Debug.Log("Auto generating now!");


            await Task.Delay(5000);

            Transform t_ChapterManager = new GameObject("ChapterManager").transform;

            ChaptersManager chaptersManager = t_ChapterManager.AddComponent<ChaptersManager>();    

            foreach (var chapter in chapters.Data)
            {
                Transform t_chapter = GenerateAndAddComponent(chapter.ChapterName, chapter.ChapterName, t_ChapterManager).transform;

                foreach (var phase in chapter.Phases)
                {
                    Transform t_phase = GenerateAndAddComponent(phase.PhaseName, phase.PhaseName, t_chapter).transform;

                    if (phase.UseMilestones)
                    {
                        foreach (var milestone in phase.Milestones)
                        {
                            GenerateAndAddComponent(milestone, milestone, t_phase);
                        }

                        MilestonePhase ph = t_phase.GetComponent<MilestonePhase>();

                        if (ph != null)
                        {
                            Milestone[] milestones = t_phase.GetComponentsInChildren<Milestone>();

                            if(milestones.Length > 0)
                            {
                                foreach(var milestone in milestones)
                                    ph.RegisterMilestone(milestone); 
                            }
                        }
                    }

                  
                }

                Chapter ch = t_chapter.GetComponent<Chapter>();

                if (ch != null)
                {
                    Phase[] phases = t_chapter.GetComponentsInChildren<Phase>();
                    if (phases.Length > 0)
                    {
                        foreach (Phase phase in phases)
                        {
                            ch.RegisterPhase(phase);
                        }
                    }
                }

                if (chaptersManager != null)
                {
                    chaptersManager.RegisterChapter(t_chapter.GetComponent<Chapter>());
                }
            }
        }

        private static async void TryAutoAddComponentToPhase()
        {
            await Task.Delay(4000);

            GameObject selectedObject = UnityEditor.Selection.activeGameObject;

            if (SessionState.GetInt(AddComponentToPhaseKey, 0) == 1)
            {
                SessionState.SetInt(AddComponentToPhaseKey, 0);
                string componentName = SessionState.GetString(ComponentTypeNameKey, "");
                SessionState.SetString(ComponentTypeNameKey, "");

                if (selectedObject != null)
                {
                    Phase phase = selectedObject.GetComponent<Phase>();

                    if(phase != null && phase is not MilestonePhase && componentName != "")
                    {
                        GameObject componentObject = GenerateAndAddComponent(componentName, componentName, phase.transform);

                        Component component = componentObject.GetComponent<Component>();    
                        if (component != null)
                        {
                            phase.RegisterComponent(component);
                        }
                    }
                }
            }
            else if(SessionState.GetInt(AddComponentToMilestoneKey, 0) == 1)
            {
                SessionState.SetInt(AddComponentToMilestoneKey, 0);
                string componentName = SessionState.GetString(ComponentTypeNameKey, "");

                SessionState.SetString(ComponentTypeNameKey, "");

                if(selectedObject != null)
                {
                    Milestone milestone = selectedObject.GetComponent<Milestone>();

                    if(milestone != null && componentName != "")
                    {
                        GameObject componentObject = GenerateAndAddComponent(componentName, componentName, milestone.transform);
                        Component component = componentObject.GetComponent<Component>();

                        if (component != null)
                        {
                            milestone.GetComponentInParent<Phase>().RegisterComponent(component);
                        }
                    }
                }
            }
            else if (SessionState.GetInt(AddChaptersKey, 0) == 1)
            {
                SessionState.SetInt(AddChaptersKey, 0);
                string chapterName = SessionState.GetString(ChapterNameKey, "");

                SessionState.SetString(ChapterNameKey, "");

                if (selectedObject != null)
                {
                    ChaptersManager chManager = selectedObject.GetComponent<ChaptersManager>();

                    if (chManager != null && chapterName != "")
                    {
                        GameObject chapterObject = GenerateAndAddComponent(chapterName, chapterName, chManager.transform);
                        Chapter chapter = chapterObject.GetComponent<Chapter>();

                        if (chapter != null)
                        {
                            chManager.RegisterChapter(chapter); 
                        }
                    }
                }
            }
            else if (SessionState.GetInt(AddMilestoneKey, 0) == 1)
            {
                SessionState.SetInt(AddMilestoneKey, 0);
                string milestoneName = SessionState.GetString(MilestoneNameKey, "");

                SessionState.SetString(MilestoneNameKey, "");

                if (selectedObject != null)
                {
                    Phase phase = selectedObject.GetComponent<Phase>();

                    if (phase != null && milestoneName != "" && phase is MilestonePhase)
                    {
                        GameObject chapterObject = GenerateAndAddComponent(milestoneName, milestoneName, phase.transform);
                        Milestone milestone = chapterObject.GetComponent<Milestone>();

                        if (milestone != null)
                        {
                            (phase as MilestonePhase).RegisterMilestone(milestone);
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

        private string ReplaceComponentName(string template, string componentName)
        {
            return template.Replace("#COMPONENT_NAME#", componentName.Replace(" ", ""));
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

        #region Public Static Methods

        public void AddComponentToPhase(string component)
        {
            if (component != null)
            {
                SessionState.SetInt(AddComponentToPhaseKey, 1);
                SessionState.SetString(ComponentTypeNameKey, component);

                GenerateComponentScript(component);
            }
            else
            {
                Debug.LogWarning("Phase or Component is null. Cannot add component to phase.");
            }
        }

        public void AddComponentToMilestone(string component)
        {
            if (component != null)
            {
                SessionState.SetInt(AddComponentToMilestoneKey, 1);
                SessionState.SetString(ComponentTypeNameKey, component);

                GenerateComponentScript(component);
            }
            else
            {
                Debug.LogWarning("Milestone or Component is null. Cannot add component to milestone.");
            }
        }

        public void AddChapter(string chapterName)
        {
            if (chapterName != null)
            {
                SessionState.SetInt(AddChaptersKey, 1);
                SessionState.SetString(ChapterNameKey, chapterName);

                GenerateComponentScript(chapterName);
            }
        }



        private void GenerateComponentScript(string componentName)
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
            string componentTemplate = ReadFile(Path.Combine(templatesPath, _componentTemplateName));

            if(componentTemplate != null && componentTemplate != "")
            {
                string componentContent = ReplaceComponentName(componentTemplate, componentName);
                CreateClassFile(scriptsPath, componentName, componentContent);
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