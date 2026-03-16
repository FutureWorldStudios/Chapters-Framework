using UnityEngine;

namespace VRG.ChapterFramework
{
    public static class CanvasGroupExtensions
    {
        public static void FadeIn(this CanvasGroup group, float duration = 1f)
        {
            CanvasGroupFadeRunner.Instance.StartFade(group, 1f, duration);
        }

        public static void FadeOut(this CanvasGroup group, float duration = 1f)
        {
            CanvasGroupFadeRunner.Instance.StartFade(group, 0f, duration);
        }

        /// <summary>
        /// Instantly hide without fading (alpha = 0, interaction disabled)
        /// </summary>
        public static void Hide(this CanvasGroup group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        public static void EnableInteraction(this CanvasGroup group)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        public static void DisableInteraction(this CanvasGroup group)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}