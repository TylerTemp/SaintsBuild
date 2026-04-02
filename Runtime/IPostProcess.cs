namespace SaintsBuild
{
    public interface IPostProcess
    {
#if UNITY_EDITOR
        bool EditorOnPostProcess(PostProcessInfo postProcessInfo);
#endif
    }
}
