using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;


namespace VRG.ChapterFramework
{
    public class ChaptersManager : MonoBehaviour
    {
        [SerializeField] private List<Chapter> chapters = new List<Chapter>();

        public static Action<int> OnChapterBegun;
        public void AddChapter(Chapter chapter)
        {
            if (!chapters.Contains(chapter))
            {
                chapters.Add(chapter);
            }
        }
    }
}