#if UNITY_ANDROID
using SaintsBuild.Editor;
using SaintsBuild.Editor.Android;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SaintsBuild.Samples.Scripts.Editor.Android
{
    public static class BuildAndroid : IPostGenerateGradleAndroidProject
    {

        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject (BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.Android)
            {
                return;
            }

            using AndroidManifest androidManifest = new AndroidManifest(pathToBuiltProject);

            // required for android 12 if you have activity alias etc:
            Debug.Log($"Add android:exported=true");
            androidManifest.SetActivityWithLauncherIntentAttribute("exported", "true");

            // androidManifest.SetApplicationTheme("dark");

            androidManifest.SetStartingActivityName("CustomActivity");

            androidManifest.SetHardwareAcceleration();

            androidManifest.SetBillingPermission();
            androidManifest.SetVibratePermission();

            // other you need
            androidManifest.SetPermissionAttribute("WRITE_EXTERNAL_STORAGE", 18);

            // change values under `launcher/src/main/res`
            AndroidRes androidRes = new AndroidRes(pathToBuiltProject);
            // add new
            using (AndroidValue valueXml = androidRes.CreateOrGetValue("values-b+zh+Hans/string.xml"))
            {
                valueXml.SetString("app_name", "简体项目");
            }
            using (AndroidValue valueXml = androidRes.CreateOrGetValue("values-b+zh+Hant/string.xml"))
            {
                valueXml.SetString("app_name", "繁體項目");
            }
        }
    }
}
#endif
