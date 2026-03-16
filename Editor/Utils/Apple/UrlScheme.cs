using System.Collections.Generic;

namespace SaintsBuild.Editor.Utils.Apple
{
    public readonly struct UrlScheme
    {
        public readonly string CfBundleTypeRole;
        public readonly string CfBundleURLName;
        public readonly IReadOnlyList<string> CfBundleURLSchemes;

        public UrlScheme(string cfBundleTypeRole, string cfBundleURLName, IReadOnlyList<string> cfBundleURLSchemes)
        {
            CfBundleTypeRole = cfBundleTypeRole;
            CfBundleURLName = cfBundleURLName;
            CfBundleURLSchemes = cfBundleURLSchemes;
        }
    }
}
