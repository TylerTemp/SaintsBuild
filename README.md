# SaintsBuild #

`SaintsBuild` is a Unity build (packing) tool for Unity, mainly focused on android and ios.

## Usage ##

### Android ###

```csharp
#if UNITY_ANDROID
using SaintsBuild.Editor;
using UnityEditor.Android;
using UnityEditor.Callbacks;

namespace Editor
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
```


### iOS ###

```csharp
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Editor
{
    public static class BuildIos
    {
        [PostProcessBuild(1)]
        public static void OnPostProcessBuildAttr(BuildTarget target, string path)
        {
            using IosBuild iosBuild = new IosBuild(target, path);
            // AddFramework
            iosBuild.AddFrameworkToProjectCoreHaptics();
            iosBuild.AddFrameworkToProjectAdServices();
            iosBuild.AddFrameworkToProjectAppTrackingTransparency();
            iosBuild.AddFrameworkToProjectAdSupport();
            iosBuild.AddFrameworkToProjectCoreTelephony();
            iosBuild.AddFrameworkToProjectSecurity();
            iosBuild.AddFrameworkToProjectSystemConfiguration();
            iosBuild.AddFrameworkToProjectLibCPP();
            iosBuild.AddFrameworkToProjectLibZ();
            iosBuild.AddFramework("YourFrameWorkName");

            // AddBuildProperty
            iosBuild.AddBuildPropertyOtherLdFlags();
            iosBuild.AddBuildProperty("yourBuildName", "params");

            // AddInAppPurchase
            iosBuild.ManagerAddInAppPurchase();
            // use iosBuild.manager to add more you need

            // BITCODE
            // all target
            iosBuild.SetAllBuildPropertyBitcode("NO");
            // or if you want to set for each target
            iosBuild.SetBuildPropertyBitcode(iosBuild.project.GetUnityMainTargetGuid(), "NO");
            iosBuild.SetBuildPropertyBitcode(iosBuild.project.TargetGuidByName(PBXProject.GetUnityTestTargetName()), "NO");
            iosBuild.SetBuildPropertyBitcode(iosBuild.project.GetUnityFrameworkTargetGuid(), "NO");

            // urlScheme
            iosBuild.PListAddUrlSchemes(new[]
            {
                new IosBuild.UrlScheme
                {
                    CFBundleURLName = "yourUrlName",
                    CFBundleTypeRole = "Viewer",
                    CFBundleURLSchemes = new[] {"yourSchemes"},
                },
                new IosBuild.UrlScheme
                {
                    CFBundleURLName = "Another",
                    CFBundleTypeRole = "Viewer",
                    CFBundleURLSchemes = new[] {"anotherSchemes"},
                }
            });

            // ITSAppUsesNonExemptEncryption
            iosBuild.PListSetITSAppUsesNonExemptEncryption(false);

            // if you manually installed the Facebook SDK (not Unity Package)
            iosBuild.PListSetString("FacebookAppID", "123412341234");
            iosBuild.PListSetString("FacebookDisplayName", "fbAppName");
            iosBuild.PListSetString("FacebookClientToken", "token_1234");
            iosBuild.PListSetBoolean("FacebookAutoLogAppEventsEnabled", true);
            iosBuild.PListSetBoolean("FacebookAdvertiserIDCollectionEnabled", true);

            // set market icon as Unity will not process it
            iosBuild.SetMarketIcon1024("Assets/Game/icon-1024x1024.png");

            // at last if you want to run pod, you need to save it first
            // otherwise it'll be saved when exiting using block
            iosBuild.Save();
            iosBuild.RunPodInstall("/usr/local/bin/pod");
        }
    }
}
#endif
```
