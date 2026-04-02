using System;

namespace SaintsBuild.Editor.Utils
{
    [Serializable]
    public struct BackupInfo
    {
        public string assetPath;
        public string backupPath;

        public BackupInfo(string source, string backupTarget)
        {
            assetPath = source;
            backupPath = backupTarget;
        }
    }
}
