#if UNITY_ANDROID
using SaintsBuild.Editor;
using UnityEditor.Callbacks;

namespace SaintsBuild.Samples.Editor
{
    public static class BuildAndroid
    {
        [PostProcessBuild(1)]
        public static void OnPostGenerateGradleAndroidProject(string path)
        {
            using AndroidAppManifestBuild androidAppManifest = new AndroidAppManifestBuild(path);

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
