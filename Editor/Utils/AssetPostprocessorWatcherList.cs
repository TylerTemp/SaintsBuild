using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    public class AssetPostprocessorWatcherList: ScriptableObject
    {
        public PrefabInfo[] prefabInfos = {};
        public ScriptableObject[] scriptableObjs = {};
    }
}
