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
