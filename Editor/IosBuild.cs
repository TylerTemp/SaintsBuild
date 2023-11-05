#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class IosBuild: IDisposable
{
    private const string KIconInDir = "Unity-iPhone/Images.xcassets/AppIcon.appiconset";
    private const string KContentsJsonFilePath = "Unity-iPhone/Images.xcassets/AppIcon.appiconset/Contents.json";

    public string projectPath;
    public PBXProject project;
    public ProjectCapabilityManager manager;
    public string targetGuid;
    public PlistDocument plist;
    public string plistPath;
    public PlistElementDict plistElementDict;

    public IosBuild(BuildTarget target, string path)
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

        plistPath = Path.Combine(path, "/Info.plist");
        plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        plistElementDict = plist.root;
    }

    public void Save()
    {
        manager.WriteToFile();
        File.WriteAllText(projectPath, project.WriteToString());
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    public void Dispose()
    {
    }

    public void AddFrameworkToProjectCoreHaptics(bool weak=false) => AddFramework("CoreHaptics.framework", weak);
    public void AddFrameworkToProjectAdServices(bool weak=false) => AddFramework("AdServices.framework", weak);
    public void AddFrameworkToProjectAppTrackingTransparency(bool weak=false) => AddFramework("AppTrackingTransparency.framework", weak);
    public void AddFrameworkToProjectAdSupport(bool weak=false) => AddFramework("AdSupport.framework", weak);
    public void AddFrameworkToProjectCoreTelephony(bool weak=false) => AddFramework("CoreTelephony.framework", weak);
    public void AddFrameworkToProjectSecurity(bool weak=false) => AddFramework("Security.framework", weak);
    public void AddFrameworkToProjectSystemConfiguration(bool weak=false) => AddFramework("SystemConfiguration.framework", weak);
    public void AddFrameworkToProjectLibCPP(bool weak=false) => AddFramework("libc++.tbd", weak);
    public void AddFrameworkToProjectLibZ(bool weak=false) => AddFramework("libz.tbd", weak);
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


    // private static void AddCapabilities(ProjectCapabilityManager manager)
    // {
    //     manager.AddInAppPurchase();
    // }

    public struct UrlScheme
    {
        public string CFBundleTypeRole;
        public string CFBundleURLName;
        public string[] CFBundleURLSchemes;
    }

    public void PListAddUrlSchemes(IEnumerable<UrlScheme> urlSchemes)
    {
        PlistElementArray cfBundleURLTypes = plistElementDict.CreateArray("CFBundleURLTypes");

        foreach (UrlScheme urlScheme in urlSchemes)
        {
            Debug.Log($"add urlScheme {urlScheme.CFBundleURLName}");
            PlistElementDict urlSchemeDict = cfBundleURLTypes.AddDict();
            urlSchemeDict.SetString("CFBundleTypeRole", urlScheme.CFBundleTypeRole);
            urlSchemeDict.SetString("CFBundleURLName", urlScheme.CFBundleURLName);
            PlistElementArray cfBundleURLSchemes = urlSchemeDict.CreateArray("CFBundleURLSchemes");
            foreach (string urlSchemeCfBundleURLScheme in urlScheme.CFBundleURLSchemes)
            {
                cfBundleURLSchemes.AddString(urlSchemeCfBundleURLScheme);
            }
        }
    }

    public void PListSetITSAppUsesNonExemptEncryption(bool value=true) => plistElementDict.SetBoolean("ITSAppUsesNonExemptEncryption", value);
    public void PListSetBoolean(string name, bool value) => plistElementDict.SetBoolean(name, value);
    public void PListSetString(string name, string value) => plistElementDict.SetString(name, value);
    public void PListSetInteger(string name, int value) => plistElementDict.SetInteger(name, value);
    public void PListSetReal(string name, float value) => plistElementDict.SetReal(name, value);
    public void PListSetDate(string name, DateTime value) => plistElementDict.SetDate(name, value);


    public void SetMarketIcon1024(string iconPath)
    {
        Debug.Log($"copying icon {iconPath} -> {projectPath}");
        CopyIconFile(projectPath, iconPath, "icon-1024.png");
        SetupContentsJsonFile(projectPath, "icon-1024.png");
        Debug.Log("copying icon finished");
    }

    public void RunPodInstall(string podPath)
    {
        string podFile = Path.Combine(projectPath, "Podfile");

        if(File.Exists(podPath) && File.Exists(podFile))
        {
            Debug.Log("pod install");

            Process podInstallProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = projectPath,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = podPath,
                    RedirectStandardInput = false,
                    UseShellExecute = true,
                    Arguments = "install",
                },
            };
            podInstallProcess.Start();
            podInstallProcess.WaitForExit();
            // if(!File.Exists(podFile))
            // {
            //     Debug.Log("pod init");
            //
            //     Process podInitProcess = new Process
            //     {
            //         StartInfo = new ProcessStartInfo
            //         {
            //             WorkingDirectory = projDir,
            //             WindowStyle = ProcessWindowStyle.Normal,
            //             FileName = podPath,
            //             RedirectStandardInput = false,
            //             UseShellExecute = true,
            //             Arguments = "init",
            //         },
            //     };
            //     podInitProcess.Start();
            //     podInitProcess.WaitForExit();
            // }
            //
            // Debug.Log("pod check facebook");
            // // string[] curLines = File.ReadAllLines(podFile, new UTF8Encoding());
            // // bool hasFacebook = false;
            // // int lineInsertMarkerIndex = -1;
            // List<string> lines = new List<string>();
            // foreach (string line in File.ReadLines(podFile, new UTF8Encoding()))
            // {
            //     string lineTrim = line.Trim();
            //     string newLine = line;
            //     if (lineTrim == "# Pods for Unity-iPhone" || lineTrim == "# Pods for UnityFramework")
            //     {
            //         newLine = "  pod 'FBSDKCoreKit'\n";
            //     }
            //
            //     lines.Add(newLine);
            // }
            //
            // File.WriteAllLines(podFile, lines, new UTF8Encoding());

            // Debug.Assert(foundLine, string.Join("", lines));

        }
    }

    [PostProcessBuild(int.MaxValue)]
    public static void OnPostProcessBuild(BuildTarget target, string projDir)
    {
        Debug.Log($"ios projDir: {projDir}");

        // GameBuild gameBuildInfo = AssetDatabase.LoadAssetAtPath<GameBuild>("Assets/RawResources/Game/GameBuild.asset");

        #region plist
        // Add url schema to plist file
        string plistPath = projDir + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // if(useFacebook)
        // {
        //     rootDict.SetString("FacebookAppID", facebookAppID);
        //
        //     string facebookDisplayName = gameBuildInfo.facebookDisplayName;
        //     Debug.Assert(!string.IsNullOrWhiteSpace(facebookDisplayName));
        //     rootDict.SetString("FacebookDisplayName", facebookDisplayName);
        //
        //     string facebookClientToken = gameBuildInfo.facebookClientToken;
        //     Debug.Assert(!string.IsNullOrWhiteSpace(facebookClientToken));
        //     rootDict.SetString("FacebookClientToken", facebookClientToken);
        //
        //     rootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", true);
        //     rootDict.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);
        // }

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());

        Debug.Log("plist saved");
        #endregion

        // #region icon
        // // string iconPath = Path.Combine(Application.dataPath, "RawResources/Game/icon1024x1024.png");
        // string iconPath = AssetDatabase.GetAssetPath(gameBuildInfo.icon1024);
        // Debug.Log($"copying icon {iconPath} -> {projDir}");
        // CopyIconFile(projDir, iconPath, "icon-1024.png");
        // SetupContentsJsonFile(projDir, "icon-1024.png");
        // Debug.Log("copying icon finished");
        // #endregion

        // #region cocosPod
        // string podFile = Path.Combine(projDir, "Podfile");
        // // if (useFacebook && gameBuildInfo.podFile)
        // // {
        // //     Debug.Log("copying Podfile");
        // //     File.Copy(AssetDatabase.GetAssetPath(gameBuildInfo.podFile), podFile, true);
        // // }
        //
        // string podPath = gameBuildInfo.podPath;
        // if(File.Exists(podPath) && File.Exists(podFile))
        // {
        //     Debug.Log("pod install");
        //
        //     Process podInstallProcess = new Process
        //     {
        //         StartInfo = new ProcessStartInfo
        //         {
        //             WorkingDirectory = projDir,
        //             WindowStyle = ProcessWindowStyle.Normal,
        //             FileName = podPath,
        //             RedirectStandardInput = false,
        //             UseShellExecute = true,
        //             Arguments = "install",
        //         },
        //     };
        //     podInstallProcess.Start();
        //     podInstallProcess.WaitForExit();
        //     // if(!File.Exists(podFile))
        //     // {
        //     //     Debug.Log("pod init");
        //     //
        //     //     Process podInitProcess = new Process
        //     //     {
        //     //         StartInfo = new ProcessStartInfo
        //     //         {
        //     //             WorkingDirectory = projDir,
        //     //             WindowStyle = ProcessWindowStyle.Normal,
        //     //             FileName = podPath,
        //     //             RedirectStandardInput = false,
        //     //             UseShellExecute = true,
        //     //             Arguments = "init",
        //     //         },
        //     //     };
        //     //     podInitProcess.Start();
        //     //     podInitProcess.WaitForExit();
        //     // }
        //     //
        //     // Debug.Log("pod check facebook");
        //     // // string[] curLines = File.ReadAllLines(podFile, new UTF8Encoding());
        //     // // bool hasFacebook = false;
        //     // // int lineInsertMarkerIndex = -1;
        //     // List<string> lines = new List<string>();
        //     // foreach (string line in File.ReadLines(podFile, new UTF8Encoding()))
        //     // {
        //     //     string lineTrim = line.Trim();
        //     //     string newLine = line;
        //     //     if (lineTrim == "# Pods for Unity-iPhone" || lineTrim == "# Pods for UnityFramework")
        //     //     {
        //     //         newLine = "  pod 'FBSDKCoreKit'\n";
        //     //     }
        //     //
        //     //     lines.Add(newLine);
        //     // }
        //     //
        //     // File.WriteAllLines(podFile, lines, new UTF8Encoding());
        //
        //     // Debug.Assert(foundLine, string.Join("", lines));
        //
        // }
        // #endregion

        // #region placeholder
        //
        // string placeholderFile = Path.Combine(projDir, ".placeholder");
        // if (!File.Exists(placeholderFile))
        // {
        //     FileStream fs = File.Create(placeholderFile);
        //     fs.Close();
        // }
        //
        // #endregion
    }

    private static void CopyIconFile(string projDir, string iconFilePath, string newIconFileName)
    {
        string d = Path.Combine(Path.Combine (projDir, KIconInDir), newIconFileName);
        File.Copy(iconFilePath, d, true);
    }

    private static void SetupContentsJsonFile(string projDir, string newIconFileName)
    {
        string contentsJsonName = Path.Combine(projDir, KContentsJsonFilePath);

        string[] lines = File.ReadAllLines (contentsJsonName);
        IEnumerable<string> destLines = SetupTheIcon (lines, newIconFileName);

        WriteAllLine(destLines, contentsJsonName);
    }

    private static IEnumerable<string> SetupTheIcon(IReadOnlyList<string> lines, string iconFilename)
    {
        int findPos = FindThePos(lines);
        if (findPos == -2)
        {
            return lines;
        }
        return findPos == -1 ? AddTheObject (lines, iconFilename) : SetTheObjetFilename (lines, findPos, iconFilename);
    }

    private static string[] AddTheObject(IReadOnlyList<string> lines, string iconFilename)
    {
        string t =
            $",{{\n\"size\" : \"1024x1024\",\n\"idiom\" : \"ios-marketing\",\n				\"filename\" : \"{iconFilename}\",\n				\"scale\" : \"1x\"\n		}}";
        string[] dest = new string[lines.Count+1];
        int findPos = lines.Count - 9;
        for (int i = 0, dI = 0; i < lines.Count; i++, dI++) {
            if (i == findPos) {
                dest[dI] = t;
                dI++;
            }
            dest[ dI ] = lines[i];
        }
        return dest;
    }

    private static string[] SetTheObjetFilename (IReadOnlyList<string> lines, int findPos, string iconFilename )
    {
        string[] dest = new string[lines.Count+1];
        for (int i = 0, dI = 0; i < lines.Count; i++, dI++) {
            if (i == findPos) {
                dest[dI] = $"\"filename\" : \"{iconFilename}\",";
                dI++;
            }
            dest[ dI ] = lines[i];
        }
        return dest;
    }

    private static int FindThePos(IReadOnlyList<string> lines)
    {
        for (int i = 0; i < lines.Count; i++) {
            string line = lines[i];
            if (line.IndexOf("\"size\" : \"1024x1024\"", StringComparison.Ordinal) < 0)
            {
                continue;
            }

            string nx = lines[i+1];
            if (nx.IndexOf("\"idiom\" : \"ios-marketing\"", StringComparison.Ordinal) < 0)
            {
                // throw new Exception("XcodeAddIcon: ERROR ");
                return -2;
            }
            return i+2;
        }
        return -1;
    }

    private static void WriteAllLine(IEnumerable<string> lines, string filePath)
    {
        StreamWriter streamWriter = new StreamWriter(filePath);
        foreach (string line in lines)
        {
            streamWriter.Write(line);
            streamWriter.Write('\n');
        }
        streamWriter.Close();
    }
}
#endif
