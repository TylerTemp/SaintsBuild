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
            using AndroidAppManifestBuild androidAppManifest = new AndroidAppManifestBuild(path);

            // required for android 12 if you have activity alias etc:
            Debug.Log($"Add android:exported=true");
            androidAppManifest.SetApplicationAttribute("exported", "true");

            androidAppManifest.SetApplicationTheme("dark");

            androidAppManifest.SetStartingActivityName("CustomActivity");

            androidAppManifest.SetHardwareAcceleration();

            androidAppManifest.SetBillingPermission();
            androidAppManifest.SetVibratePermission();
            // other you need
            androidAppManifest.SetPermissionAttribute("WRITE_EXTERNAL_STORAGE", 18);
        }
    }
}
#endif
