using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FWS
{

    public enum SceneId
    {

    }


    public class AppManager : MonoBehaviour
    {
        [SerializeField] private double _resetTime = 10;

        private string _currentActiveScene;

        private double _pausedSeconds;
        private DateTime _pausedTime;

        private AudioListener _audioListener;

        public string CurrentActiveScene => _currentActiveScene;
       
        public static Action<string> OnSceneLoaded;
        public static Action OnSceneLoadBegun;

        public static AppManager Instance;


        #region Unity Methods

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            OnSceneLoaded += HandleSceneLoaded;
            _audioListener = FindObjectOfType<AudioListener>();
#if !UNITY_EDITOR
            OVRManager.display.RecenterPose();
#endif
        }

        private void OnDestroy()
        {
            OnSceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(string SceneName)
        {
          
            _audioListener = FindObjectOfType<AudioListener>();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _pausedTime = DateTime.UtcNow;
                Debug.Log($"[{this.name}] App Paused: {_pausedTime.ToString("T")}");
            }
            else
            {
                if (_pausedTime != default)
                {
                    _pausedSeconds = (DateTime.UtcNow - _pausedTime).TotalSeconds;
                    if (_pausedSeconds > _resetTime)
                    {
                        Debug.Log($"[{this.name}] Time Exceeded, Resetting.");

                        ResetApplication();
                    }
                    else
                    {
                        Debug.Log($"[{this.name}] Time Remaining.");
                    }
                }
            }
        }

        [Button]
        private void ResetApplication()
        {
            _audioListener.enabled = false;
            ResetAllVars();
        }

        #endregion

        #region Public Methods

        #region Scene Management

        public void LoadSceneAsync(SceneId sceneId, float fadeDuration = 2f, bool fadeOnLoad = true)
        {
            if (ValidateScene(sceneId.ToString()))
                LoadSceneAsync(sceneId.ToString(), fadeDuration, fadeOnLoad);
        }

      

        [Button]
        public async void LoadSceneAsync(string sceneName, float fadeDuration = 2f, bool fadeOnLoad = true)
        {
            if (!ValidateScene(sceneName))
            {
                Debug.LogError($"[{this.name}] Scene not found in build settings: {sceneName}");
                return;
            }

            if (fadeOnLoad)
                FWS_OVRScreenFade.Instance.FadeOut(fadeDuration);

            await Task.Delay((int)(fadeDuration * 1000));

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
            {
                Debug.Log($"[{this.name}] Loading progress: " + (asyncOperation.progress * 100f) + "%");
                await Task.Yield();
            }

            if (fadeOnLoad)
                FWS_OVRScreenFade.Instance.FadeIn(fadeDuration);

            _currentActiveScene = sceneName;
            OnSceneLoaded?.Invoke(sceneName);
        }

        [Button]
        public bool ValidateScene(string sceneName)
        {
            string sceneNameFromBuild = string.Empty;
            int buildIndex = -1;

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);

                string[] pathFolder = path.Split('/');
                sceneNameFromBuild = pathFolder[pathFolder.Length - 1].Replace(".unity", "");

                if (sceneNameFromBuild == sceneName)
                {
                    buildIndex = i;
                    break;
                }
            }

            if (sceneNameFromBuild == sceneName)
            {
                Debug.Log($"[{this.name}] Found scene: {sceneNameFromBuild} at index: {buildIndex}");
            }
            else
            {
                Debug.LogError($"[{this.name}] Scene not found {sceneNameFromBuild}");
            }

            return buildIndex != -1;
        }

        #endregion

        public void ResetAllVars()
        {
          
        }

        #endregion
    }
}
