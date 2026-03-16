#if UNITY_IOS
using SaintsBuild.Editor.IOS;
using SaintsBuild.Editor.Utils.Apple;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace SaintsBuild.Samples.Scripts.Editor.IOS
{
    public static class BuildIos
    {
        [PostProcessBuild(1)]
        public static void OnPostProcessBuildAttr(BuildTarget target, string path)
        {
            #region IOSPBXProject
            using (IOSPBXProject iosPBXProject = new IOSPBXProject(target, path)) {
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
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.Project.GetUnityMainTargetGuid(), "NO");
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.Project.TargetGuidByName(PBXProject.GetUnityTestTargetName()), "NO");
                iosPBXProject.SetBuildPropertyBitcode(iosPBXProject.Project.GetUnityFrameworkTargetGuid(), "NO");
            }
            #endregion

            #region Plist
            using (IOSPlist iosPlist = new IOSPlist(target, path)) {
                // urlScheme
                iosPlist.AddUrlSchemes(new[]
                {
                    new UrlScheme("yourUrlName", "Viewer", new[] {"yourSchemes"}),
                    new UrlScheme("Another", "Viewer", new[] {"anotherSchemes"}),
                });

                // ITSAppUsesNonExemptEncryption
                iosPlist.SetITSAppUsesNonExemptEncryption(false);

                // if you manually installed the Facebook SDK (not Unity Package)
                iosPlist.SetString("FacebookAppID", "123412341234");
                iosPlist.SetString("FacebookDisplayName", "fbAppName");
                iosPlist.SetString("FacebookClientToken", "token_1234");
                iosPlist.SetBoolean("FacebookAutoLogAppEventsEnabled", true);
                iosPlist.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
            }
            #endregion

            #region IOSAppIcon
            using (IOSAppIcon iosAppIcon = new IOSAppIcon(target, path)) {
                // set market icon as Unity will not process it
                iosAppIcon.SetMarketIcon1024("Assets/Game/icon-1024x1024.png");
            }
            #endregion
        }
    }
}
#endif
