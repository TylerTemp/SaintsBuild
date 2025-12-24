namespace SaintsBuild
{
    public interface IPostProcess
    {
#if UNITY_EDITOR
        void EditorOnPostProcess(PostProcessInfo postProcessInfo);
#endif
    }
}
