# SaintsBuild #

`SaintsBuild` is a Unity build (packing) tool for Unity, mainly focused on android and ios.

## Usage ##

### Android ###

```csharp
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
```


### iOS ###

```csharp
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace SaintsBuild.Samples.Editor
{
    public static class BuildIos
    {
        [PostProcessBuild(1)]
        public static void OnPostProcessBuildAttr(BuildTarget target, string path)
        {
            #region IosPBXProject
            using (IosPBXProject iosPBXProject = new IosPBXProject(target, path)) {
                // AddFramework
                iosPBXProject.AddFrameworkToProjectCoreHaptics();
                iosPBXProject.AddFrameworkToProjectAdServices();
                iosPBXProject.AddFrameworkToProjectAppTrackingTransparency();
                iosPBXProject.AddFrameworkToProjectAdSupport();
                iosPBXProject.AddFrameworkToProjectCoreTelephony();
                iosPBXProject.AddFrameworkToProjectSecurity();
                iosPBXProject.AddFrameworkToProjectSystemConfiguration();
                iosPBXProject.AddFrameworkToProjectLibCPP();
                iosPBXProject.AddFrameworkToProjectLibZ();
                iosPBXProject.AddFramework("YourFrameWorkName");

                // AddBuildProperty
                iosPBXProject.AddBuildPropertyOtherLdFlags();
                iosPBXProject.AddBuildProperty("yourBuildName", "params");

                // AddInAppPurchase
                iosPBXProject.ManagerAddInAppPurchase();
                // use iosBuild.manager to add more you need

                // BITCODE
                // all target
                iosPBXProject.SetAllBuildPropertyBitcode("NO");
                // or if you want to set for each target
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.project.GetUnityMainTargetGuid(), "NO");
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.project.TargetGuidByName(PBXProject.GetUnityTestTargetName()), "NO");
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.project.GetUnityFrameworkTargetGuid(), "NO");
            }
            #endregion

            #region Plist
            using (IosPlist iosPlist = new IosPlist(target, path)) {
                // urlScheme
                iosPlist.PListAddUrlSchemes(new[]
                {
                    new IosPlist.UrlScheme
                    {
                        CFBundleURLName = "yourUrlName",
                        CFBundleTypeRole = "Viewer",
                        CFBundleURLSchemes = new[] {"yourSchemes"},
                    },
                    new IosPlist.UrlScheme
                    {
                        CFBundleURLName = "Another",
                        CFBundleTypeRole = "Viewer",
                        CFBundleURLSchemes = new[] {"anotherSchemes"},
                    },
                });

                // ITSAppUsesNonExemptEncryption
                iosPlist.PListSetITSAppUsesNonExemptEncryption(false);

                // if you manually installed the Facebook SDK (not Unity Package)
                iosPlist.PListSetString("FacebookAppID", "123412341234");
                iosPlist.PListSetString("FacebookDisplayName", "fbAppName");
                iosPlist.PListSetString("FacebookClientToken", "token_1234");
                iosPlist.PListSetBoolean("FacebookAutoLogAppEventsEnabled", true);
                iosPlist.PListSetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
            }
            #endregion

            #region IosAppIcon
            using (IosAppIcon iosAppIcon = new IosAppIcon(target, path)) {
                // set market icon as Unity will not process it
                iosAppIcon.SetMarketIcon1024("Assets/Game/icon-1024x1024.png");
            }
            #endregion
        }
    }
}
#endif
```
