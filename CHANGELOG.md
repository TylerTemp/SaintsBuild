## 2.1.3 ##

Fix: Android `AndroidRes` now can get the correct path for directly building apk. README updated.

## 2.1.2 ##

Fix: Assign package as Unity 6k requires

## 2.1.1 ##

Fix: built-in `rcedit-x64.exe` could not be found if this package is installed via `openupm`

## 2.1.0 ##

Add `SaintsBuild.Editor.Windows.WindowsDetails`, you can now change
*   ProductName
*   ProductVersion
*   FileDescription
*   LegalCopyright
*   FileVersion
for your windows build. No longer says the windows exe version is Unity Editor's version.

## 2.0.0

1.  When processing an asset, it will now get backup on entering play mode or starting build, and get restored when exit play mode or exit build
2.  Change `IPostProcess.EditorOnPostProcess` to return a `bool` value to show if the asset need to backup/restore. Has no effect for scene objects
3.  Fix: WatchList might get purged when building. WatchList now pauses the watcher if the project is playing or building
4.  Move menu to `Tools/SaintsBuild`

## 1.2.1

**Breaking Changes**

1.  Change types to corresponding namespaces, change type names
2.  Support `OSX` plist modify
3.  Add `FindActivityNode`, `RemoveIntentMainLauncher` for `AndroidManifest`

## 1.1.0

1.  Fix: If a settings is already in android, override it instead of appending a new one
2.  Add: Ability to change values under res folder for android

## 1.0.8

Fix cover config file in some case

## 1.0.7

1.  Delete unused `IPostProcessScene`
2.  Add `postProcessInfo.PrefabDangerousDestroy()` function

## 1.0.6

1.  Delete `IPostProcessScene`, change callback name of `IPostProcess`
2.  Support prefab processer

## 1.0.5

1.  Renamed to `IPostProcess`
2.  `IPostProcess` now works for prefab & ScriptableObject too

## 1.0.4

1.  iOS: Rename `IosPlist.PListSetBoolean` to `IosPlist.SetBoolean`
2.  Android: Rename `AndroidAppManifestBuild` to `AndroidManifest`
3.  Android: Add `AndroidManifest.SetApplicationAttribute`
4.  Android: Add `AndroidManifest.SetActivityWithLauncherIntentAttribute`
