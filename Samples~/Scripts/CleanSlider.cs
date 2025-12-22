using UnityEngine;
using UnityEngine.UI;

namespace SaintsBuild.Samples
{
    public class CleanSlider: MonoBehaviour, IPostProcessScene
    {
        public Slider slider;

#if UNITY_EDITOR
        public void EditorOnPostProcessScene(bool isBuilding)
        {
            slider.value = 0;
        }
#endif
    }
}
