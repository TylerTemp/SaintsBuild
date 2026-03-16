using System.Linq;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace SaintsBuild.Editor.Utils.Apple
{
    public static class IOSPlistUtils
    {
        public static void AddUrlScheme(PlistElementDict root, UrlScheme urlScheme)
        {
            const string cfBundleURLTypesKey = "CFBundleURLTypes";

            PlistElementArray urlTypesKeyElement;
            if (root.values.ContainsKey(cfBundleURLTypesKey))
            {
                urlTypesKeyElement = root[cfBundleURLTypesKey].AsArray();
            }
            else
            {
                Debug.Log($"create plist array {cfBundleURLTypesKey}");
                urlTypesKeyElement = root.CreateArray(cfBundleURLTypesKey);
            }

            const string cfBundleURLNameKey = "CFBundleURLName";

            PlistElementDict urlTypesFirstDict = urlTypesKeyElement.values
                .OfType<PlistElementDict>()
                .FirstOrDefault(each => each.values.TryGetValue(cfBundleURLNameKey, out PlistElement cfBundleURLNameElement) && cfBundleURLNameElement.AsString() == urlScheme.CfBundleURLName);

            if (urlTypesFirstDict == null)
            {
                Debug.Log($"create plist dict under {cfBundleURLTypesKey}: {cfBundleURLNameKey}={urlScheme.CfBundleURLName}");
                urlTypesFirstDict = urlTypesKeyElement.AddDict();
                urlTypesFirstDict.SetString(cfBundleURLNameKey, urlScheme.CfBundleURLName);
            }

            if(urlScheme.CfBundleTypeRole != null)
            {
                const string cfBundleTypeRoleKey = "CFBundleTypeRole";
                urlTypesFirstDict.SetString(cfBundleTypeRoleKey, urlScheme.CfBundleTypeRole);
            }

            const string cfBundleURLSchemesKey = "CFBundleURLSchemes";
            PlistElementArray cfBundleURLSchemesElement;
            if (urlTypesFirstDict.values.TryGetValue(cfBundleURLSchemesKey,
                    out PlistElement cfBundleURLSchemesRawElement))
            {
                cfBundleURLSchemesElement = cfBundleURLSchemesRawElement.AsArray();
            }
            else
            {
                Debug.Log($"create plist array under {cfBundleURLTypesKey}: {cfBundleURLSchemesKey}");
                cfBundleURLSchemesElement = urlTypesFirstDict.CreateArray(cfBundleURLSchemesKey);
            }

            cfBundleURLSchemesElement.values.Clear();
            foreach (string bundleURLScheme in urlScheme.CfBundleURLSchemes)
            {
                Debug.Log($"set plist array under {cfBundleURLTypesKey}.{cfBundleURLSchemesKey} set {bundleURLScheme}");
                cfBundleURLSchemesElement.AddString(bundleURLScheme);
            }
        }
    }
}
