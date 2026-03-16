using UnityEngine;
using System.Collections;

namespace VRG.ChapterFramework
{
    internal class CanvasGroupFadeRunner : MonoBehaviour
    {
        private static CanvasGroupFadeRunner _instance;
        public static CanvasGroupFadeRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CanvasGroupFadeRunner");
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    _instance = obj.AddComponent<CanvasGroupFadeRunner>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public void StartFade(CanvasGroup group, float target, float duration)
        {
            StartCoroutine(FadeRoutine(group, target, duration));
        }

        private IEnumerator FadeRoutine(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }

            group.alpha = target;

            bool visible = target >= 0.99f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
