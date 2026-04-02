using UnityEngine;

namespace SaintsBuild.Samples.Scripts
{
    public class SubContent : MonoBehaviour, IPostProcess
    {

#if UNITY_EDITOR
        public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            DestroyImmediate(gameObject, true);
            return true;
        }
#endif
    }
}
