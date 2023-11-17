#if UNITY_IOS
using System;
using UnityEditor;
using System.IO;
using UnityEditor.iOS.Xcode;
using Debug = UnityEngine.Debug;


public class IosPBXProject: IDisposable
{
    public string projectPath;
    public PBXProject project;
    public ProjectCapabilityManager manager;
    public string targetGuid;

    public IosPBXProject(BuildTarget target, string path)
    {
        // Read.
        Debug.Log($"ios build path: {path}");
        projectPath = PBXProject.GetPBXProjectPath(path);
        Debug.Log($"ios projectPath: {projectPath}");
        project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));
        // string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
        // string targetGUID = project.TargetGuidByName(targetName);
        targetGuid = project.GetUnityFrameworkTargetGuid();

        manager = new ProjectCapabilityManager(
            projectPath,
            "Entitlements.entitlements",
            targetGuid: targetGuid
        );

        Debug.Log($"ios projDir: {path}");
    }

    public void Save()
    {
        manager.WriteToFile();
        File.WriteAllText(projectPath, project.WriteToString());
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
    public void AddFramework(string name, bool weak=false) => project.AddFrameworkToProject(targetGuid, name, weak);

    public void AddBuildPropertyOtherLdFlags(string value="-ObjC") => AddBuildProperty("OTHER_LDFLAGS", value);
    public void AddBuildProperty(string name, string value) => project.AddBuildProperty(targetGuid, name, value);

    public void SetAllBuildPropertyBitcode(string value = "NO")
    {
        foreach (string target in new[]
                 {
                     project.GetUnityMainTargetGuid(),
                     project.TargetGuidByName(PBXProject.GetUnityTestTargetName()),
                     project.GetUnityFrameworkTargetGuid(),
                 })
        {
            SetBuildProperty(target, "ENABLE_BITCODE", value);
        }
    }
    public void SetBuildPropertyBitcode(string target, string value)
    {
        SetBuildProperty(target, "ENABLE_BITCODE", value);
    }
    public void SetBuildProperty(string guid, string name, string value) => project.SetBuildProperty(guid, name, value);

    public void ManagerAddInAppPurchase() => manager.AddInAppPurchase();
}
#endif
