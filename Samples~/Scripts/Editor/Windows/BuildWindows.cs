using SaintsBuild.Editor.Windows;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SaintsBuild.Samples.Scripts.Editor.Windows
{
    public class BuildWindows : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;
        public void OnPostprocessBuild(BuildReport report)
        {
            WindowsDetails windowsDetails = new WindowsDetails(report);

            windowsDetails
                .SetProductName()
                .SetProductVersion()
                .SetFileVersion()
                .SetLegalCopyright()
                .SetFileDescription($"{Application.productName} is a nice game created by {Application.companyName}. Enjoy!")
                .Apply();
        }
    }
}
