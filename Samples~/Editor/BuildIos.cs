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
                iosPBXProject.AddFrameworkCoreHaptics();
                iosPBXProject.AddFrameworkAdServices();
                iosPBXProject.AddFrameworkAppTrackingTransparency();
                iosPBXProject.AddFrameworkAdSupport();
                iosPBXProject.AddFrameworkCoreTelephony();
                iosPBXProject.AddFrameworkSecurity();
                iosPBXProject.AddFrameworkSystemConfiguration();
                iosPBXProject.AddFrameworkLibCPP();
                iosPBXProject.AddFrameworkLibZ();
                iosPBXProject.AddFramework("YourFrameWorkName");

                // AddBuildProperty
                iosPBXProject.AddBuildPropertyOtherLdFlags("-ObjC");
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
                iosPlist.AddUrlSchemes(new[]
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
                iosPlist.SetITSAppUsesNonExemptEncryption(false);

                // if you manually installed the Facebook SDK (not Unity Package)
                iosPlist.SetString("FacebookAppID", "123412341234");
                iosPlist.SetString("FacebookDisplayName", "fbAppName");
                iosPlist.SetString("FacebookClientToken", "token_1234");
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
