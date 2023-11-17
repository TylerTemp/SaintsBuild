#if UNITY_IOS
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.iOS.Xcode;
using Debug = UnityEngine.Debug;


public class IosAppIcon: IDisposable
{
    private const string KIconInDir = "Unity-iPhone/Images.xcassets/AppIcon.appiconset";
    private const string KContentsJsonFilePath = "Unity-iPhone/Images.xcassets/AppIcon.appiconset/Contents.json";

    public string projectPath;
    public readonly List<string> contentsLines = new List<string>();

    public IosAppIcon(BuildTarget target, string path)
    {
        projectPath = PBXProject.GetPBXProjectPath(path);

        string contentsJsonName = Path.Combine(projectPath, KContentsJsonFilePath);

        contentsLines.AddRange(File.ReadAllLines (contentsJsonName));
    }

    public void Save()
    {
        WriteAllLine(contentsLines, Path.Combine(projectPath, KContentsJsonFilePath));
    }

    public void Dispose()
    {
        Save();
    }

    public void SetMarketIcon1024(string iconPath)
    {
        Debug.Log($"copying icon {iconPath} -> {projectPath}");
        CopyIconFile(projectPath, iconPath, "icon-1024.png");
        // SetupContentsJsonFile(projectPath, "icon-1024.png");
        SetupTheIcon(contentsLines, "icon-1024.png");
        Debug.Log("copying icon finished");
    }

    private static void CopyIconFile(string projDir, string iconFilePath, string newIconFileName)
    {
        string d = Path.Combine(Path.Combine (projDir, KIconInDir), newIconFileName);
        File.Copy(iconFilePath, d, true);
    }

    // private static void SetupContentsJsonFile(string projDir, string newIconFileName)
    // {
    //     SetupTheIcon (content, newIconFileName);
    //     // IEnumerable<string> destLines = SetupTheIcon (lines, newIconFileName);
    //     //
    //     // WriteAllLine(destLines, contentsJsonName);
    // }

    private static void SetupTheIcon(IList<string> lines, string iconFilename)
    {
        int findPos = FindThePos(lines as IReadOnlyList<string>);
        if (findPos == -2)
        {
            return;
        }
        if(findPos == -1)
        {
            AddTheObject(lines, iconFilename);
        } else
        {
            SetTheObjetFilename(lines, findPos, iconFilename);
        }
    }

    private static void AddTheObject(IList<string> lines, string iconFilename)
    {
        string t =
            $",{{\n\"size\" : \"1024x1024\",\n\"idiom\" : \"ios-marketing\",\n				\"filename\" : \"{iconFilename}\",\n				\"scale\" : \"1x\"\n		}}";
        // string[] dest = new string[lines.Count+1];
        int findPos = lines.Count - 9;
        lines.Insert(findPos, t);
        // for (int i = 0, dI = 0; i < lines.Count; i++, dI++) {
        //     if (i == findPos) {
        //         dest[dI] = t;
        //         dI++;
        //     }
        //     dest[ dI ] = lines[i];
        // }
        // return dest;
    }

    private static void SetTheObjetFilename (IList<string> lines, int findPos, string iconFilename )
    {
        lines.Insert(findPos, $"\"filename\" : \"{iconFilename}\",");
        // string[] dest = new string[lines.Count+1];
        // for (int i = 0, dI = 0; i < lines.Count; i++, dI++) {
        //     if (i == findPos) {
        //         dest[dI] = $"\"filename\" : \"{iconFilename}\",";
        //         dI++;
        //     }
        //     dest[ dI ] = lines[i];
        // }
        // return dest;
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
