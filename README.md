# SaintsBuild #

[![openupm](https://img.shields.io/npm/v/today.comes.saintsbuild?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/today.comes.saintsbuild/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Ftoday.comes.saintsbuild)](https://openupm.com/packages/today.comes.saintsbuild/)

`SaintsBuild` is a Unity build (packing) tool for Unity, mainly focused on android and ios.

## Installation ##

*   Using [OpenUPM](https://openupm.com/packages/today.comes.saintsbuild/)

    ```bash
    openupm install today.comes.saintsbuild
    ```

*   Using git upm:

    add to `Packages/manifest.json` in your project

    ```javascript
    {
        "dependencies": {
            "today.comes.saintsbuild": "https://github.com/TylerTemp/SaintsBuild.git",
            // your other dependencies...
        }
    }
    ```

*   Using a `unitypackage`:

    Go to the [Release Page](https://github.com/TylerTemp/SaintsBuild/releases) to download a desired version of `unitypackage` and import it to your project

*   Using a git submodule:

    ```bash
    git submodule add https://github.com/TylerTemp/SaintsBuild.git Assets/SaintsBuild
    ```

## Change Log ##

**2.0.0**

1.  When processing an asset, it will now get backup on entering play mode or starting build, and get restored when exit play mode or exit build
2.  Change `IPostProcess.EditorOnPostProcess` to return a `bool` value to show if the asset need to backup/restore. Has no effect for scene objects
3.  Fix: WatchList might get purged when building. WatchList now pauses the watcher if the project is playing or building
4.  Move menu to `Tools/SaintsBuild`

See [the full change log](https://github.com/TylerTemp/SaintsBuild/blob/master/CHANGELOG.md).

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
        public static void OnPostGenerateGradleAndroidProject(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.Android)
            {
                return;
            }

            using AndroidManifest androidAppManifest = new AndroidManifest(path);

            // required for android 12 if you have activity alias etc:
            androidManifest.SetActivityWithLauncherIntentAttribute("exported", "true");

            androidManifest.SetApplicationTheme("dark");

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
```


### iOS ###

```csharp
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using SaintsBuild.Editor.IOS;
using SaintsBuild.Editor.Utils.Apple;

namespace SaintsBuild.Samples.Editor
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
```

### IPostProcess ###

Get a callback when building a scene or play a scene. Useful when you have some debug tools and want to clean it before playing or building.

> [!WARNING]
> For building, the callback is called on build process (so the change happens already before `Awake`). 
> However, in editor runtime (not built runtime), this happens **AFTER** `Awake`


**Set Up**

Add macro `SAINTSBUILD_POST_PROCESS`. You can do it by `Tools/SaintsBuild/Enable Scene Post Process`

Or, put this in any of your editor script:

```csharp
using UnityEditor.Callbacks;

[PostProcessScene]
public static void OnPostProcess()
{
    Debug.Log("call SaintsBuild OnPostProcess");
    SaintsBuild.Editor.Callbacks.OnPostProcess();
}

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
```

**Usage**

Clean Up:

```csharp
public class CleanSlider: MonoBehaviour, IPostProcess
{
    public Slider slider;

#if UNITY_EDITOR  // don't forget this
    public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
    {
        slider.value = 0;
        return false;  // no need to backup
    }
#endif
}
```

Advanced Usage:

```csharp
public class TextContainer : MonoBehaviour, IPostProcess
{
    public GameObject prefab;
    public Transform container;

    private void Awake()
    {
#if UNITY_EDITOR
        // in build, the clean process will happen before build
        // but in play mode, we need to call it manually
        CleanUpExample();
#endif

        // other works
        foreach (int index in Enumerable.Range(0, 5))
        {
            GameObject example = Instantiate(prefab, container);
            example.GetComponent<Text>().text = $"Runtime Awake created {index}";
        }
    }

#if UNITY_EDITOR
    public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
    {
        if (postProcessInfo.IsBuilding)  // in building process, Unity will call this function and apply changes to build result
        {
            CleanUpExample();
        }
        else  // in play mode, unity will call Awake first, then call this function. Changes will be revoked after exiting play mode
        {
            // deal conflict with Awake
        }
        return true;  // need backup/restore as this will modify the asset itself.
    }

    private void CleanUpExample()
    {
        foreach (Transform eachExample in container.Cast<Transform>().ToArray())
        {
            Debug.Log(eachExample.gameObject.name);
            DestroyImmediate(eachExample.gameObject, true);  // allow destroy in prefab
        }
    }
#endif
}
```

Using on a prefab which can also be in scene:

```csharp
public class SubContent : MonoBehaviour, IPostProcess
{
#if UNITY_EDITOR
    public bool EditorOnPostProcess(PostProcessInfo postProcessInfo)
    {
        DestroyImmediate(gameObject, true);  // destory it even it's a prefab
        return true;  // restore it when finish playing/building
    }
#endif
}
```
