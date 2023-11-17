#if UNITY_IOS
using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.iOS.Xcode;
using Debug = UnityEngine.Debug;


public class IosPlist: IDisposable
{
    public PlistDocument plist;
    public string plistPath;
    public PlistElementDict plistElementDict;

    public IosPlist(BuildTarget target, string path)
    {
        plistPath = Path.Combine(path, "/Info.plist");
        plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        plistElementDict = plist.root;
    }

    public void Save()
    {
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    public void Dispose()
    {
        Save();
    }

    public struct UrlScheme
    {
        public string CFBundleTypeRole;
        public string CFBundleURLName;
        public string[] CFBundleURLSchemes;
    }

    public void AddUrlSchemes(IEnumerable<UrlScheme> urlSchemes)
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

    public void SetITSAppUsesNonExemptEncryption(bool value=true) => plistElementDict.SetBoolean("ITSAppUsesNonExemptEncryption", value);
    public void PListSetBoolean(string name, bool value) => plistElementDict.SetBoolean(name, value);
    public void SetString(string name, string value) => plistElementDict.SetString(name, value);
    public void SetInteger(string name, int value) => plistElementDict.SetInteger(name, value);
    public void SetReal(string name, float value) => plistElementDict.SetReal(name, value);
    public void SetDate(string name, DateTime value) => plistElementDict.SetDate(name, value);
}
#endif
