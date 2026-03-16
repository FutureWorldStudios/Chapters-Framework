namespace VRG.ChapterFramework.Core
{
    public interface IExecutable
    {
        void Begin();
        void Complete();
        void ForceReset();
        void ForceCompletion();
    }
}