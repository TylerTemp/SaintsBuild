# SaintsBuild #

`SaintsBuild` is a Unity build (packing) tool for Unity, mainly focused on android and ios.

## Installation ##

*   Using [OpenUPM](https://openupm.com/)

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
```

### IPostProcessScene ###

Get a callback when building a scene or play a scene. Useful when you have some debug tools and want to clean it before playing or building.

**Set Up**

Put this in any of your editor script:

```csharp
using UnityEditor.Callbacks;

[PostProcessScene]
public static void OnPostProcessScene()
{
    Debug.Log("call SaintsBuild OnPostProcessScene");
    SaintsBuild.Editor.Callbacks.OnPostProcessScene();
}
```

**Usage**

Clean Up:

```csharp
public class CleanSlider: MonoBehaviour, IPostProcessScene
{
    public Slider slider;

#if UNITY_EDITOR  // don't forget this
    public void EditorOnPostProcessScene(bool isBuilding)
    {
        slider.value = 0;
    }
#endif
}
```

Advanced Usage:

```csharp
public class TextContainer : MonoBehaviour, IPostProcessScene
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
    public void EditorOnPostProcessScene(bool isBuilding)
    {
        if (isBuilding)  // in building process, Unity will call this function and apply changes to build result
        {
            CleanUpExample();
        }
        else  // in play mode, unity will call Awake first, then call this function. Changes will be revoked after exiting play mode
        {
            // deal conflict with Awake
        }
    }

    private void CleanUpExample()
    {
        foreach (Transform eachExample in container.Cast<Transform>().ToArray())
        {
            Debug.Log(eachExample.gameObject.name);
            DestroyImmediate(eachExample.gameObject);
        }
    }
#endif
}
```
