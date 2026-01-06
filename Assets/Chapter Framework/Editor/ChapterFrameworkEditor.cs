using UnityEngine;
using UnityEditor;
using VRG.ChapterFramework;
using Unity.VisualScripting;

namespace VRG.ChapterFramework.Editor
{
    public class ChapterFrameworkEditor
    {
        [MenuItem("Tools/Chapter Framework/Setup Initial Scene Setup")]
        private static void SetupChapterFramework()
        {
            GameObject chapterParent = new GameObject("Chapters Manager");
            Selection.activeGameObject = chapterParent; 

            ChaptersManager manager = chapterParent.AddComponent<ChaptersManager>();


            CreateChapter("Chapter 1", manager);
            CreateChapter("Chapter 2", manager);
        }

        [MenuItem("Tools/Chapter Framework/Add a New Chapter")]
        private static void CreateOrAddNewChapter()
        {
            ChaptersManager manager = GameObject.FindObjectOfType<ChaptersManager>();

            if(manager != null)
            {
                CreateChapter("Chapter " + (manager.transform.childCount + 1), manager);
            }
        }


        private static void CreateChapter(string chapterName, ChaptersManager manager)
        {
            GameObject chapter = new GameObject(chapterName);
            Chapter chapterComponent = chapter.AddComponent<Chapter>();

            chapter.transform.SetParent(manager.transform);

            manager.AddChapter(chapterComponent);   


            CreatePhase("Phase 1", chapterComponent);
            CreatePhase("Phase 2", chapterComponent);
            CreateMilestonePhase("Phase 3 - Milestone", chapterComponent);
            CreateMilestonePhase("Phase 4 - Milestone", chapterComponent);
        }

        private static void CreatePhase(string phaseName, Chapter chapter)
        {
            GameObject phase = new GameObject(phaseName);
            Phase phaseComponent = phase.AddComponent<Phase>();
            chapter.AddPhase(phaseComponent);
            phase.transform.SetParent(chapter.transform);
        }

        private static void CreateMilestonePhase(string phaseName, Chapter chapter)
        {
            GameObject phase = new GameObject(phaseName);
            MilestonePhase phaseComponent = phase.AddComponent<MilestonePhase>();
            chapter.AddPhase(phaseComponent);
            phase.transform.SetParent(chapter.transform);

            CreateMilestones(3, phaseComponent);
        }

        private static void CreateMilestones(int count, MilestonePhase milestonePhase)
        {
            for (int i = 1; i <= count; i++)
            {
                CreateMilestone("Milestone " + i, milestonePhase);
            }
        }

        private static void CreateMilestone(string milestoneName, MilestonePhase milestonePhase)
        {
            GameObject milestone = new GameObject(milestoneName);
            Milestone milestoneComponent = milestone.AddComponent<Milestone>();
            milestonePhase.AddMilestone(milestoneComponent);
            milestone.transform.SetParent(milestonePhase.transform);
        }
    }
}