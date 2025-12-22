using UnityEngine;

namespace SaintsBuild.Editor.Utils
{
    public class AssetPostprocessorWatcherList: ScriptableObject
    {
        public Component[] components = {};
        public ScriptableObject[] scriptableObjs = {};
    }
}
