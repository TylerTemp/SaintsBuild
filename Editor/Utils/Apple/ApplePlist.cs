using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.iOS.Xcode;

namespace SaintsBuild.Editor.Utils.Apple
{
    public class ApplePlist: IDisposable
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable MemberCanBeProtected.Global
        public PlistDocument Plist;
        public string PlistPath;
        public PlistElementDict PlistElementDict;
        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        // ReSharper disable once UnusedParameter.Local
        protected void Init(string pListPath)
        {
            PlistPath = pListPath;
            Plist = new PlistDocument();
            Plist.ReadFromString(File.ReadAllText(PlistPath));
            PlistElementDict = Plist.root;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Save()
        {
            File.WriteAllText(PlistPath, Plist.WriteToString());
        }

        public void Dispose()
        {
            Save();
        }

        public void AddUrlSchemes(IEnumerable<UrlScheme> urlSchemes)
        {
            foreach (UrlScheme urlScheme in urlSchemes)
            {
                IOSPlistUtils.AddUrlScheme(PlistElementDict, urlScheme);
            }
        }

        // ReSharper disable once InconsistentNaming
        public void SetITSAppUsesNonExemptEncryption(bool value=true) => PlistElementDict.SetBoolean("ITSAppUsesNonExemptEncryption", value);
        public void SetBoolean(string name, bool value) => PlistElementDict.SetBoolean(name, value);
        public void SetString(string name, string value) => PlistElementDict.SetString(name, value);
        public void SetInteger(string name, int value) => PlistElementDict.SetInteger(name, value);
        public void SetReal(string name, float value) => PlistElementDict.SetReal(name, value);
        public void SetDate(string name, DateTime value) => PlistElementDict.SetDate(name, value);
    }
}
