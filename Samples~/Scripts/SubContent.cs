using UnityEngine;

namespace SaintsBuild.Samples
{
    public class SubContent : MonoBehaviour, IPostProcess
    {

#if UNITY_EDITOR
        public void EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            switch (postProcessInfo.Type)
            {
                case PostProcessType.SceneGameObject:  // We can safely destroy unnecessary
                    DestroyImmediate(gameObject);
                    break;
                case PostProcessType.Prefab:
                    if (postProcessInfo.IsBuilding)  // only destroy on build
                    {
                        DestroyImmediate(gameObject);
                    }
                    else  // otherwise, hide it so we don't destroy the prefab and save it
                    {
                        gameObject.SetActive(false);
                    }
                    break;
            }
        }
#endif
    }
}
