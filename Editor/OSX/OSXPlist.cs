using System.IO;
using SaintsBuild.Editor.Utils.Apple;
using UnityEditor;
using UnityEngine;


namespace SaintsBuild.Editor.IOS
{
    public class OSXPlist: ApplePlist
    {
        // ReSharper disable once UnusedParameter.Local
        public OSXPlist(BuildTarget target, string path)
        {
            string plistPath = Path.Combine(path, Application.productName, "Info.plist");
            if (!File.Exists(plistPath))
            {
                plistPath = Path.Combine(path, "Contents", "Info.plist");
            }

            if (!File.Exists(plistPath))
            {
                Debug.LogWarning("Info.plist not found, skip");
                return;
            }

            Init(plistPath);
        }
    }
}
