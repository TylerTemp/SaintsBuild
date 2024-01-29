namespace SaintsBuild
{
    public interface IPostProcessScene
    {
#if UNITY_EDITOR
        void EditorOnPostProcessScene(bool isBuilding);
#endif
    }
}
