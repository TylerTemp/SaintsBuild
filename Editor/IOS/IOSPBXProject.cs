#if UNITY_IOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using Debug = UnityEngine.Debug;


namespace SaintsBuild.Editor.IOS
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public class IOSPBXProject: IDisposable
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable MemberCanBePrivate.Global
        public string ProjectPath;
        public PBXProject Project;
        public ProjectCapabilityManager Manager;
        public string TargetGuid;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        public IOSPBXProject(BuildTarget target, string path)
        {
            // Read.
            Debug.Log($"ios build path: {path}");
            ProjectPath = PBXProject.GetPBXProjectPath(path);
            Debug.Log($"ios projectPath: {ProjectPath}");
            Project = new PBXProject();
            Project.ReadFromString(File.ReadAllText(ProjectPath));
            // string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
            // string targetGUID = project.TargetGuidByName(targetName);
            TargetGuid = Project.GetUnityFrameworkTargetGuid();

            Manager = new ProjectCapabilityManager(
                ProjectPath,
                "Entitlements.entitlements",
                targetGuid: TargetGuid
            );

            Debug.Log($"ios projDir: {path}");
        }

        public void Save()
        {
            Manager.WriteToFile();
            File.WriteAllText(ProjectPath, Project.WriteToString());
        }

        public void Dispose()
        {
            Save();
        }

        public void AddFrameworkCoreHaptics(bool weak=false) => AddFramework("CoreHaptics.framework", weak);
        public void AddFrameworkAdServices(bool weak=false) => AddFramework("AdServices.framework", weak);
        public void AddFrameworkAppTrackingTransparency(bool weak=false) => AddFramework("AppTrackingTransparency.framework", weak);
        public void AddFrameworkAdSupport(bool weak=false) => AddFramework("AdSupport.framework", weak);
        public void AddFrameworkCoreTelephony(bool weak=false) => AddFramework("CoreTelephony.framework", weak);
        public void AddFrameworkSecurity(bool weak=false) => AddFramework("Security.framework", weak);
        public void AddFrameworkSystemConfiguration(bool weak=false) => AddFramework("SystemConfiguration.framework", weak);
        public void AddFrameworkLibCPP(bool weak=false) => AddFramework("libc++.tbd", weak);
        public void AddFrameworkLibZ(bool weak=false) => AddFramework("libz.tbd", weak);
        public void AddFramework(string name, bool weak=false) => Project.AddFrameworkToProject(TargetGuid, name, weak);

        public void AddBuildPropertyOtherLdFlags(string value="-ObjC") => AddBuildProperty("OTHER_LDFLAGS", value);
        public void AddBuildProperty(string name, string value) => Project.AddBuildProperty(TargetGuid, name, value);

        public void SetAllBuildPropertyBitcode(string value = "NO")
        {
            foreach (string target in new[]
                     {
                         Project.GetUnityMainTargetGuid(),
                         Project.TargetGuidByName(PBXProject.GetUnityTestTargetName()),
                         Project.GetUnityFrameworkTargetGuid(),
                     })
            {
                SetBuildProperty(target, "ENABLE_BITCODE", value);
            }
        }
        public void SetBuildPropertyBitcode(string target, string value)
        {
            SetBuildProperty(target, "ENABLE_BITCODE", value);
        }
        public void SetBuildProperty(string guid, string name, string value) => Project.SetBuildProperty(guid, name, value);

        public void ManagerAddInAppPurchase() => Manager.AddInAppPurchase();
    }
}
#endif
