#if UNITY_ANDROID
using SaintsBuild.Editor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SaintsBuild.Samples.Editor
{
    public static class BuildAndroid
    {
        [PostProcessBuild(1)]
        public static void OnPostGenerateGradleAndroidProject(string path)
        {
            using AndroidManifest androidManifest = new AndroidManifest(path);

            // required for android 12 if you have activity alias etc:
            Debug.Log($"Add android:exported=true");
            androidManifest.SetActivityWithLauncherIntentAttribute("exported", "true");

            androidManifest.SetApplicationTheme("dark");

            androidManifest.SetStartingActivityName("CustomActivity");

            androidManifest.SetHardwareAcceleration();

            androidManifest.SetBillingPermission();
            androidManifest.SetVibratePermission();
            // other you need
            androidManifest.SetPermissionAttribute("WRITE_EXTERNAL_STORAGE", 18);
        }
    }
}
#endif
