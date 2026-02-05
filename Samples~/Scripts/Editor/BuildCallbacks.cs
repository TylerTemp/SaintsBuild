using UnityEditor.Callbacks;
using UnityEngine;

namespace SaintsBuild.Samples.Editor
{
    public static class BuildCallbacks
    {
        [PostProcessScene]
        public static void OnPostProcessScene()
        {
            Debug.Log("call SaintsBuild OnPostProcessScene");
            SaintsBuild.Editor.Callbacks.OnPostProcessScene();
        }
    }
}
