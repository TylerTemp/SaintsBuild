#if SAINTSBUILD_POST_PROCESS
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.Build.Reporting;
using UnityEngine;
#endif

namespace SaintsBuild.Samples.Scripts.Editor
{
#if !SAINTSBUILD_POST_PROCESS
    public static class SceneCallbacks
    {
        [PostProcessScene]
        public static void OnPostProcessScene()
        {
            Debug.Log("call SaintsBuild OnPostProcessScene");
            SaintsBuild.Editor.Callbacks.OnPostProcessScene();
        }
    }
#endif

#if !SAINTSBUILD_POST_PROCESS
    public class PreprocessBuildWithReport: IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            SaintsBuild.Editor.Callbacks.OnPreprocessBuildCallback();
        }
    }

    public class PostprocessBuildWithReport: IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPostprocessBuild(BuildReport report)
        {
            SaintsBuild.Editor.Callbacks.OnPostprocessBuildCallback();
        }
    }
#endif
}
