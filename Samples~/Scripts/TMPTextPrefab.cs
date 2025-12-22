using TMPro;
using UnityEngine;

namespace SaintsBuild.Samples
{
    public class TMPTextPrefab : MonoBehaviour, IPostProcess
    {
        public TMP_Text targetRenderer;
#if UNITY_EDITOR
        public void EditorOnPostProcessScene(bool isBuilding)
        {
            targetRenderer.text = "Build Text Bake!";
        }
#endif
    }
}
