namespace SaintsBuild
{
    public interface IPostProcess
    {
#if UNITY_EDITOR
        void EditorOnPostProcessScene(bool isBuilding);
#endif
    }
}
