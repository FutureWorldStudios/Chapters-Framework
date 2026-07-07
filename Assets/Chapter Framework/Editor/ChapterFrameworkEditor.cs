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
using VRG.ChapterFramework.Core;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;

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
        private const string _uiPhaseTemplateName = "UIPhaseTemplate.cs.txt";
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
        private static string MilestoneNamesJsonKey = "MilestoneNamesJsonKey";

        private static string ComponentTypeNameKey = "ComponentTypeNameKey";
        private static string ChapterNameKey = "ChapterNameKey";
        private static string PhaseNameKey = "PhaseNameKey";
        private static string MilestonePhaseNameKey = "MilestonePhaseNameKey";
        private static string MilestoneNameKey = "MilestoneNameKey";

        private static string TargetObjectPathKey = "TargetObjectPathKey";
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

        [MenuItem("GameObject/Chapters Framework/Add and Register/Component", false, 0)]
        private static void AddNewComponent()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogWarning("No GameObject selected. Please select a GameObject.");
                return;
            }

            GameObject selectedObject = Selection.activeGameObject;

            Phase phase = selectedObject.GetComponent<Phase>();
            Milestone milestone = selectedObject.GetComponent<Milestone>();

            if (phase == null && milestone == null)
            {
                Debug.LogWarning("Selected object must have either a Phase or Milestone component.");
                return;
            }

            GameObject newComponent = new GameObject("New Component");
            Undo.RegisterCreatedObjectUndo(newComponent, "Create New Component");

            newComponent.transform.SetParent(selectedObject.transform, false);

            DefaultComponentEntity component = newComponent.AddComponent<DefaultComponentEntity>();

            if (phase != null)
            {
                phase.RegisterComponent(component);
            }
            else
            {
                Debug.LogWarning("Selected object has Milestone but no registration logic is implemented for it yet.");
                // milestone.RegisterComponent(component); // use this if Milestone supports it
            }

            Selection.activeGameObject = newComponent;

            Debug.Log("Added new component under: " + selectedObject.name);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Default Component", false, 0)]
        private static void AddComponent()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                Debug.LogWarning("No GameObject selected. Please select a GameObject.");
                return;
            }

            GameObject newComponent = new GameObject("New Component");
            Undo.RegisterCreatedObjectUndo(newComponent, "Create New Component");
            newComponent.transform.SetParent(selectedObject.transform, false);
            newComponent.AddComponent<DefaultComponentEntity>();

            Selection.activeGameObject = newComponent;

            Debug.Log("Added new component under: " + selectedObject.name);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Custom Component", false, 0)]
        private static void AddNewCustomComponent()
        {
            ModuleEditor.OpenWindow(Module.Component);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Chapter", false, 0)]
        private static void AddNewChapter()
        {
            ModuleEditor.OpenWindow(Module.Chapter);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Phase", false, 0)]
        private static void AddNewPhase()
        {
            ModuleEditor.OpenWindow(Module.Phase);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Milestone Phase", false, 0)]
        private static void AddNewMilestonePhase()
        {
            ModuleEditor.OpenWindow(Module.MilestonePhase);
        }

        [MenuItem("GameObject/Chapters Framework/Add New/Milestone", false, 0)]
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

            appManagerData = Regex.Replace(appManagerData, @"(\[SerializeField\]\s*private\s+bool\s+_boothMode\s*=\s*)\d+(\s*;)",
                                            m => $"{m.Groups[1].Value}{_boothMode}{m.Groups[2].Value}");

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

        private void GenerateScripts()
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");    

            string chapterTemplate = ReadFile(Path.Combine(templatesPath, _chapterTemplateName));
            string phaseTemplate = ReadFile(Path.Combine(templatesPath, _phaseTemplateName));
            string milestonePhaseTemplate = ReadFile(Path.Combine(templatesPath, _milestonePhaseTemplateName));
            string uiPhaseTemplate = ReadFile(Path.Combine(templatesPath, _uiPhaseTemplateName));
            string milestoneTemplate = ReadFile(Path.Combine(templatesPath, _milestoneTemplateName));

            Chapters chapters = ChapterConfig;

           
            //await Task.Delay(1000);

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
                        CreateClassFile(scriptsPath, chapterConfig.ChapterName, chapterContent, false);

                        if (chapterConfig.Phases.Count > 0)
                        {
                            foreach (PhaseConfig phaseConfig in chapterConfig.Phases)
                            {
                                if (phaseConfig.Type == PhaseType.MilestonePhase)
                                {
                                    string milestonePhaseContent = ReplaceMilestonePhaseName(milestonePhaseTemplate, phaseConfig.PhaseName);
                                    CreateClassFile(scriptsPath, phaseConfig.PhaseName, milestonePhaseContent);
                                    if (phaseConfig.Milestones.Count > 0)
                                    {
                                        foreach (string milestoneName in phaseConfig.Milestones)
                                        {
                                            string milestoneContent = ReplaceMilestoneName(milestoneTemplate, milestoneName);
                                            CreateClassFile(scriptsPath, milestoneName, milestoneContent, false);
                                        }
                                    }
                                }
                                else if(phaseConfig.Type == PhaseType.Phase)
                                {
                                    string phaseContent = ReplacePhaseName(phaseTemplate, phaseConfig.PhaseName);
                                    CreateClassFile(scriptsPath, phaseConfig.PhaseName, phaseContent, false);
                                }
                                else if(phaseConfig.Type == PhaseType.UIPhase)
                                {
                                    string phaseContent = ReplaceUIPhaseName(uiPhaseTemplate, phaseConfig.PhaseName);
                                    CreateClassFile(scriptsPath, phaseConfig.PhaseName, phaseContent, false);
                                }
                            }
                        }
                    }


                }

               
                //OnGenerateHierarchy += AutoGenerateHierarchy;   
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


            //await Task.Delay(5000);

            Transform t_ChapterManager = new GameObject("ChapterManager").transform;

            ChaptersManager chaptersManager = t_ChapterManager.AddComponent<ChaptersManager>();    

            foreach (var chapter in chapters.Data)
            {
                Transform t_chapter = GenerateAndAddComponent(chapter.ChapterName, chapter.ChapterName, t_ChapterManager).transform;

                foreach (var phase in chapter.Phases)
                {
                    Transform t_phase = GenerateAndAddComponent(phase.PhaseName, phase.PhaseName, t_chapter).transform;

                    if (phase.Type == PhaseType.MilestonePhase)
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

            UnityEditor.EditorUtility.SetDirty(chaptersManager);
            EditorSceneManager.MarkSceneDirty(t_ChapterManager.gameObject.scene);
            Selection.activeGameObject = t_ChapterManager.gameObject;

            Debug.Log(
                $"Created and registered {chaptersManager.GetChapterCount()} chapters.");
        }

        private static async void TryAutoAddComponentToPhase()
        {
            await Task.Delay(4000);

            string targetPath = SessionState.GetString(TargetObjectPathKey, "");
            GameObject selectedObject = FindGameObjectByPath(targetPath);

            if (selectedObject == null)
                selectedObject = UnityEditor.Selection.activeGameObject;

            if (SessionState.GetInt(AddComponentToPhaseKey, 0) == 1)
            {
                SessionState.SetInt(AddComponentToPhaseKey, 0);
                string componentName = SessionState.GetString(ComponentTypeNameKey, "");
                SessionState.SetString(ComponentTypeNameKey, "");

                if (selectedObject != null)
                {
                    Phase phase = selectedObject.GetComponent<Phase>();

                    if (phase != null && phase is not MilestonePhase && componentName != "")
                    {
                        GameObject componentObject = GenerateAndAddComponent(componentName, componentName, phase.transform);

                        ComponentEntity component = componentObject.GetComponent<ComponentEntity>();
                        if (component != null)
                        {
                            phase.RegisterComponent(component);
                            Debug.Log($"Registered component '{component.name}' to '{phase.name}'.");
                        }
                    }
                }

                SessionState.SetString(TargetObjectPathKey, "");
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
                        ComponentEntity component = componentObject.GetComponent<ComponentEntity>();

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
            else if (SessionState.GetInt(AddPhasekey, 0) == 1)
            {
                SessionState.SetInt(AddPhasekey, 0);
                string phaseName = SessionState.GetString(PhaseNameKey, "");
                SessionState.SetString(PhaseNameKey, "");

                if (selectedObject != null)
                {
                    Chapter chapter = selectedObject.GetComponent<Chapter>();

                    if (chapter != null && phaseName != "")
                    {
                        GameObject phaseObject = GenerateAndAddComponent(phaseName, phaseName, chapter.transform);
                        Phase phase = phaseObject.GetComponent<Phase>();

                        if (phase != null)
                        {
                            chapter.RegisterPhase(phase);
                        }
                    }
                }
            }
            else if (SessionState.GetInt(AddMilestonePhaseKey, 0) == 1)
            {
                SessionState.SetInt(AddMilestonePhaseKey, 0);

                string milestonePhaseName = SessionState.GetString(MilestonePhaseNameKey, "");
                SessionState.SetString(MilestonePhaseNameKey, "");

                string milestonesJson = SessionState.GetString(MilestoneNamesJsonKey, "");
                SessionState.SetString(MilestoneNamesJsonKey, "");

                List<string> milestoneNames = new List<string>();
                if (!string.IsNullOrEmpty(milestonesJson))
                {
                    StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(milestonesJson);
                    if (wrapper != null && wrapper.Data != null)
                        milestoneNames = wrapper.Data;
                }

                if (selectedObject != null)
                {
                    Chapter chapter = selectedObject.GetComponent<Chapter>();

                    if (chapter != null && milestonePhaseName != "")
                    {
                        GameObject phaseObject = GenerateAndAddComponent(milestonePhaseName, milestonePhaseName, chapter.transform);
                        MilestonePhase phase = phaseObject.GetComponent<MilestonePhase>();

                        if (phase != null)
                        {
                            chapter.RegisterPhase(phase);

                            foreach (string milestoneName in milestoneNames)
                            {
                                if (string.IsNullOrWhiteSpace(milestoneName))
                                    continue;

                                GameObject milestoneObject = GenerateAndAddComponent(milestoneName, milestoneName, phase.transform);
                                Milestone milestone = milestoneObject.GetComponent<Milestone>();

                                if (milestone != null)
                                {
                                    phase.RegisterMilestone(milestone);
                                }
                            }
                        }
                    }
                }
            }
            else if (SessionState.GetInt(AddComponentToMilestoneKey, 0) == 1)
            {
                SessionState.SetInt(AddComponentToMilestoneKey, 0);

                string componentName = SessionState.GetString(ComponentTypeNameKey, "");
                SessionState.SetString(ComponentTypeNameKey, "");

                if (selectedObject != null)
                {
                    Milestone milestone = selectedObject.GetComponent<Milestone>();

                    if (milestone != null && componentName != "")
                    {
                        GameObject componentObject = GenerateAndAddComponent(componentName, componentName, milestone.transform);
                        ComponentEntity component = componentObject.GetComponent<ComponentEntity>();

                        if (component != null)
                        {
                            MilestonePhase parentPhase = milestone.GetComponentInParent<MilestonePhase>();

                            if (parentPhase != null)
                            {
                                parentPhase.RegisterComponent(component);
                                Debug.Log($"Registered component '{component.name}' to '{parentPhase.name}'.");
                            }
                            else
                            {
                                Debug.LogWarning("No parent MilestonePhase found.");
                            }
                        }
                    }
                }

                SessionState.SetString(TargetObjectPathKey, "");
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
        private string ReplaceUIPhaseName(string template, string phaseName)
        {
            return template.Replace("#UIPHASE_NAME#", phaseName.Replace(" ", ""));
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

        private void CreateClassFile(
     string folderAssetPath,
     string className,
     string content,
     bool importImmediately = true)
        {
            string folderFullPath = Path.GetFullPath(folderAssetPath);

            if (!Directory.Exists(folderFullPath))
                Directory.CreateDirectory(folderFullPath);

            string classNameText = className.Replace(" ", "");
            string fileFullPath = Path.Combine(folderFullPath, classNameText + ".cs");

            try
            {
                File.WriteAllText(fileFullPath, content);

                string assetPath =
                    "Assets" +
                    fileFullPath.Substring(Application.dataPath.Length).Replace('\\', '/');

                if (importImmediately)
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                Debug.Log("Successfully created class file: " + assetPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create class file: {e.Message}\nPath: {fileFullPath}");
            }
        }

        #endregion

        #region Public Static Methods

        public void AddComponentToPhase(string component)
        {
            if (!string.IsNullOrWhiteSpace(component) && Selection.activeGameObject != null)
            {
                SessionState.SetInt(AddComponentToPhaseKey, 1);
                SessionState.SetString(ComponentTypeNameKey, component);
                SessionState.SetString(TargetObjectPathKey, GetTransformPath(Selection.activeGameObject.transform));

                GenerateComponentScript(component);
            }
            else
            {
                Debug.LogWarning("Phase or Component is null. Cannot add component to phase.");
            }
        }

        public void AddComponentToMilestone(string component)
        {
            if (!string.IsNullOrWhiteSpace(component) && Selection.activeGameObject != null)
            {
                SessionState.SetInt(AddComponentToMilestoneKey, 1);
                SessionState.SetString(ComponentTypeNameKey, component);
                SessionState.SetString(TargetObjectPathKey, GetTransformPath(Selection.activeGameObject.transform));

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

                GenerateChapterScript(chapterName);
            }
        }



        private void GenerateChapterScript(string chapterName)
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
            string chapterTemplate = ReadFile(Path.Combine(templatesPath, _chapterTemplateName));

            if (!string.IsNullOrEmpty(chapterTemplate))
            {
                string chapterContent = ReplaceClassName(chapterTemplate, chapterName);
                CreateClassFile(scriptsPath, chapterName, chapterContent);
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

        public void AddPhase(string phaseName)
        {
            if (!string.IsNullOrWhiteSpace(phaseName))
            {
                SessionState.SetInt(AddPhasekey, 1);
                SessionState.SetString(PhaseNameKey, phaseName);

                GeneratePhaseScript(phaseName);
            }
            else
            {
                Debug.LogWarning("Phase name is null or empty.");
            }
        }

        public void AddMilestonePhase(string milestonePhaseName, List<string> milestoneNames)
        {
            if (!string.IsNullOrWhiteSpace(milestonePhaseName))
            {
                SessionState.SetInt(AddMilestonePhaseKey, 1);
                SessionState.SetString(MilestonePhaseNameKey, milestonePhaseName);

                string milestonesJson = JsonUtility.ToJson(new StringListWrapper { Data = milestoneNames ?? new List<string>() });
                SessionState.SetString(MilestoneNamesJsonKey, milestonesJson);

                GenerateMilestonePhaseScript(milestonePhaseName);

                if (milestoneNames != null)
                {
                    foreach (string milestoneName in milestoneNames)
                    {
                        if (!string.IsNullOrWhiteSpace(milestoneName))
                            GenerateMilestoneScript(milestoneName);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Milestone Phase name is null or empty.");
            }
        }

        public void AddMilestone(string milestoneName)
        {
            if (!string.IsNullOrWhiteSpace(milestoneName))
            {
                SessionState.SetInt(AddMilestoneKey, 1);
                SessionState.SetString(MilestoneNameKey, milestoneName);

                GenerateMilestoneScript(milestoneName);
            }
            else
            {
                Debug.LogWarning("Milestone name is null or empty.");
            }
        }
        private void GeneratePhaseScript(string phaseName)
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
            string phaseTemplate = ReadFile(Path.Combine(templatesPath, _phaseTemplateName));

            if (!string.IsNullOrEmpty(phaseTemplate))
            {
                string phaseContent = ReplacePhaseName(phaseTemplate, phaseName);
                CreateClassFile(scriptsPath, phaseName, phaseContent);
            }
        }

        private void GenerateMilestonePhaseScript(string milestonePhaseName)
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
            string milestonePhaseTemplate = ReadFile(Path.Combine(templatesPath, _milestonePhaseTemplateName));

            if (!string.IsNullOrEmpty(milestonePhaseTemplate))
            {
                string content = ReplaceMilestonePhaseName(milestonePhaseTemplate, milestonePhaseName);
                CreateClassFile(scriptsPath, milestonePhaseName, content);
            }
        }

        private void GenerateMilestoneScript(string milestoneName)
        {
            string templatesPath = Path.Combine(Application.dataPath, "Chapter Framework", "Editor", "Templates");
            string scriptsPath = Path.Combine(Application.dataPath, "Chapter Framework", "Scripts");
            string milestoneTemplate = ReadFile(Path.Combine(templatesPath, _milestoneTemplateName));

            if (!string.IsNullOrEmpty(milestoneTemplate))
            {
                string content = ReplaceMilestoneName(milestoneTemplate, milestoneName);
                CreateClassFile(scriptsPath, milestoneName, content);
            }
        }

        private static string GetTransformPath(Transform target)
        {
            if (target == null) return string.Empty;

            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
        }

        private static GameObject FindGameObjectByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Transform found = GameObject.Find(path)?.transform;
            return found != null ? found.gameObject : null;
        }


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

    public enum PhaseType
    {
        Phase,
        MilestonePhase,
        UIPhase
    }

    [Serializable]
    public class PhaseConfig
    {
        public string PhaseName;

        [OnValueChanged("RefreshMilestoneCount")]
        public PhaseType Type;

        [ShowIf("@Type==PhaseType.MilestonePhase")]
        [MinValue(0)]
        [OnValueChanged(nameof(SyncMilestones))]
        public int MilestoneCount;

        [ShowIf("@MilestoneCount > 0")]
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

        private void RefreshMilestoneCount()
        {
            MilestoneCount = 0;
            Milestones = new List<string>();
        }

    }
}

[Serializable]
public class StringListWrapper
{
    public List<string> Data = new();
}
