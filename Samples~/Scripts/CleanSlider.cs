using UnityEngine;
using UnityEngine.UI;

namespace SaintsBuild.Samples
{
    public class CleanSlider: MonoBehaviour, IPostProcess
    {
        public Slider slider;

#if UNITY_EDITOR
        public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
        {
            slider.value = 0;
            return false;
        }
#endif
    }
}
