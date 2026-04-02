using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    [FilePath("Assets/Editor Default Resources/SaintsBuild/AssetPostprocessorWatcherList.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AssetPostprocessorWatcherList: ScriptableSingleton<AssetPostprocessorWatcherList>
    {
        private const string BackupTargetFolder = "Library/SaintsBuildBackup";

        public PrefabInfo[] prefabInfos = {};
        public ScriptableObject[] scriptableObjs = {};

        public List<BackupInfo> backupInfos = new List<BackupInfo>();

        // This is very broken...
        public void SaveToDisk()
        {
            using(new DisableUnityLogScoop())
            {
                try
                {
                    Save(true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static string EnsureBackupFolder()
        {
            if (!Directory.Exists(BackupTargetFolder))
            {
                Directory.CreateDirectory(BackupTargetFolder);
            }
            return BackupTargetFolder;
        }

        public void RestoreFromBackupAndClear()
        {
            List<string> reImports = new List<string>();
            foreach (BackupInfo backupInfo in backupInfos)
            {
#if SAINTSBUILD_DEBUG && SAINTSBUILD_DEBUG_CALLBACKS
                Debug.Log($"#PostProcess# restore {backupInfo.assetPath} from {backupInfo.backupPath}");
#endif
                File.Copy(backupInfo.backupPath, backupInfo.assetPath, true);
                reImports.Add(backupInfo.assetPath);
            }

            if (reImports.Count == 0)
            {
                return;
            }

            foreach (string reImport in reImports)
            {
                AssetDatabase.ImportAsset(reImport);
            }

            EditorApplication.delayCall += () =>
            {
                backupInfos.Clear();

                using SerializedObject serializedObject = new SerializedObject(this);
                serializedObject.FindProperty(nameof(backupInfos)).arraySize = 0;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            };
        }
    }
}
