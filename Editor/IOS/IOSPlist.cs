#if UNITY_IOS
using System.IO;
using SaintsBuild.Editor.Utils.Apple;
using UnityEditor;


namespace SaintsBuild.Editor.IOS
{
    public class IOSPlist: ApplePlist
    {
        // ReSharper disable once UnusedParameter.Local
        public IOSPlist(BuildTarget target, string path)
        {
            PlistPath = Path.Combine(path, "Info.plist");
            Init(PlistPath);
        }
    }
}
#endif
