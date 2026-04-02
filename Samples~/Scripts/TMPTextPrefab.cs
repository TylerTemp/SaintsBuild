using TMPro;
using UnityEngine;

namespace SaintsBuild.Samples.Scripts
{
    public class TMPTextPrefab : MonoBehaviour, IPostProcess
    {
        public TMP_Text targetRenderer;
        public GameObject c;
#if UNITY_EDITOR
        public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            Debug.Log("PoseProcess TMPTextPrefab");
            targetRenderer.text = "Build Text Bake!";
            DestroyImmediate(c, true);
            DestroyImmediate(this, true);
            return true;
        }
#endif
    }
}
