using UnityEngine;

namespace SaintsBuild.Samples
{
    public class SubContent : MonoBehaviour, IPostProcess
    {

#if UNITY_EDITOR
        public void EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            if (postProcessInfo.PrefabDangerousDestroy())  // hide it so we don't destroy the prefab and save it
            {
                gameObject.SetActive(false);
            }
            else  // can safely destroy
            {
                DestroyImmediate(gameObject);
            }
        }
#endif
    }
}
