## 1.2.0

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
