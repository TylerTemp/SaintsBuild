using TMPro;
using UnityEngine;

namespace SaintsBuild.Samples
{
    public class TMPTextPrefab : MonoBehaviour, IPostProcess
    {
        public TMP_Text targetRenderer;
#if UNITY_EDITOR
        public void EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            targetRenderer.text = "Build Text Bake!";
        }
#endif
    }
}
