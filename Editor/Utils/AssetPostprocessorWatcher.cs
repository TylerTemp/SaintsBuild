using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    public class AssetPostprocessorWatcher: AssetPostprocessor
    {
        private static AssetPostprocessorWatcherList _assetPostprocessorWatcherList;

        private static AssetPostprocessorWatcherList EnsureAssetPostprocessorWatcherList()
        {
            if (_assetPostprocessorWatcherList == null)
            {
                _assetPostprocessorWatcherList =
                    AssetDatabase.LoadAssetAtPath<AssetPostprocessorWatcherList>(
                        "Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset");
            }
            // ReSharper disable once InvertIf
            if (_assetPostprocessorWatcherList == null)
            {
                if (!Directory.Exists("Assets/Editor Default Resources"))
                {
                    Debug.Log("Create folder: Assets/Editor Default Resources");
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                if (!Directory.Exists("Assets/Editor Default Resources/SaintsBuild"))
                {
                    Debug.Log("Create folder: Assets/Editor Default Resources/SaintsBuild");
                    AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsBuild");
                }
                _assetPostprocessorWatcherList = ScriptableObject.CreateInstance<AssetPostprocessorWatcherList>();
                Debug.Log("Create AssetPostprocessorWatcherList");
                AssetDatabase.CreateAsset(_assetPostprocessorWatcherList,
                    "Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset");
                AssetDatabase.SaveAssets();
            }

            return _assetPostprocessorWatcherList;
        }

        // importedAssets to check scriptableObject
        // delete trigger we wants
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            List<ScriptableObject> toAddSo = new List<ScriptableObject>();
            List<Component> toAddComponents = new List<Component>();

            List<int> toDeleteSoIndex = new List<int>();
            List<int> toDeleteComponentIndex = new List<int>();

            AssetPostprocessorWatcherList watchedList = EnsureAssetPostprocessorWatcherList();

            foreach (string importedAsset in importedAssets)
            {
                if (importedAsset.EndsWith(".asset"))
                {
                    ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(importedAsset);
                    if (so != null)
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (so is IPostProcess && !watchedList.scriptableObjs.Contains(so))
                        {
                            toAddSo.Add(so);
                        }
                    }
                }
                else if (importedAsset.EndsWith(".prefab"))
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(importedAsset);
                    if (go != null)
                    {
                        foreach (Component component in go.GetComponentsInChildren<Component>(true))
                        {
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            if (component != null && component is IPostProcess && !toAddComponents.Contains(component))
                            {
                                toAddComponents.Add(component);
                            }
                        }
                    }
                }
            }

            // check delete
            for (int index = watchedList.components.Length - 1; index >= 0; index--)
            {
                Component comp = watchedList.components[index];
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (comp == null || comp is not IPostProcess)
                {
                    Debug.Log($"Component {comp} is not target, will get delete at {index}");
                    toDeleteComponentIndex.Add(index);
                }
            }
            for (int index = watchedList.scriptableObjs.Length - 1; index >= 0; index--)
            {
                ScriptableObject so = watchedList.scriptableObjs[index];
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (so == null || so is not IPostProcess)
                {
                    toDeleteSoIndex.Add(index);
                }
            }

            if (toAddSo.Count == 0 && toDeleteSoIndex.Count == 0 && toDeleteComponentIndex.Count == 0 && toAddComponents.Count == 0)
            {
                return;
            }

            using (SerializedObject so = new SerializedObject(watchedList))
            {
                // int toAddSoCount = toAddSo.Count;
                SerializedProperty soPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.scriptableObjs));

                foreach (ScriptableObject target in toAddSo)
                {
                    int toAddIndex = soPropArray.arraySize;
                    soPropArray.arraySize = toAddIndex + 1;
                    SerializedProperty soPropItem = soPropArray.GetArrayElementAtIndex(toAddIndex);
                    Debug.Log($"Add {target}@[{toAddIndex}] to watched scriptableObjects");
                    soPropItem.objectReferenceValue = target;
                }

                SerializedProperty compPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.components));

                foreach (Component target in toAddComponents)
                {
                    int toAddIndex = compPropArray.arraySize;
                    compPropArray.arraySize = toAddIndex + 1;
                    SerializedProperty compPropItem = compPropArray.GetArrayElementAtIndex(toAddIndex);
                    Debug.Log($"Add {target}@[{toAddIndex}] to watched comp");
                    compPropItem.objectReferenceValue = target;
                }

                foreach (int toDeleteComp in toDeleteComponentIndex)
                {
                    Debug.Log($"Delete [{toDeleteComp}] from watched components");
                    compPropArray.DeleteArrayElementAtIndex(toDeleteComp);
                }

                foreach (int toDeleteSo in toDeleteSoIndex)
                {
                    Debug.Log($"Delete [{toDeleteSo}] from watched scriptableObject");
                    soPropArray.DeleteArrayElementAtIndex(toDeleteSo);
                }

                so.ApplyModifiedProperties();
            }
        }

        // private static readonly List<Component> ToAddComponents = new List<Component>();
        // private void OnPostprocessPrefab(GameObject go)
        // {
        //     // List<Component> toAddComponents = new List<Component>();
        //     foreach (Component component in go.GetComponentsInChildren<Component>(true))
        //     {
        //         // ReSharper disable once SuspiciousTypeConversion.Global
        //         if (component != null && component is IPostProcess && !ToAddComponents.Contains(component))
        //         {
        //             ToAddComponents.Add(component);
        //         }
        //     }
        //
        //     // if (toAddComponents.Count == 0)
        //     // {
        //     //     return;
        //     // }
        //     //
        //     // using SerializedObject so = new SerializedObject(EnsureAssetPostprocessorWatcherList());
        //     // SerializedProperty compPropArray = so.FindProperty(nameof(AssetPostprocessorWatcherList.components));
        //     //
        //     // foreach (Component addComponent in toAddComponents)
        //     // {
        //     //     int index = compPropArray.arraySize;
        //     //     compPropArray.arraySize += 1;
        //     //     SerializedProperty itemProp = compPropArray.GetArrayElementAtIndex(index);
        //     //     itemProp.objectReferenceValue = addComponent;
        //     //     Debug.Log($"Add component {addComponent}@[{index}] to watch list: {itemProp.objectReferenceValue}");
        //     // }
        //     // so.ApplyModifiedProperties();
        // }
    }
}
