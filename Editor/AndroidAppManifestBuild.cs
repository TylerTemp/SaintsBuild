#if UNITY_ANDROID
using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace SaintsBuild.Editor
{
    public class AndroidAppManifestBuild: IDisposable
    {
        private const string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string path;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public XmlNamespaceManager nsMgr;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public XmlDocument androidManifestXmlDocument;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public XmlElement applicationElement;

        public AndroidAppManifestBuild(string basePath)
        {
            path = Path.Combine(new[] { basePath, "src", "main", "AndroidManifest.xml" });
            XmlDocument document = new XmlDocument();
            using (XmlTextReader reader = new XmlTextReader(path))
            {
                reader.Read();
                document.Load(reader);
            }
            nsMgr = new XmlNamespaceManager(document.NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);

            applicationElement = document.SelectSingleNode("/manifest/application") as XmlElement;

            androidManifestXmlDocument = document;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void Save()
        {
            using XmlTextWriter writer = new XmlTextWriter(path, new UTF8Encoding(false));
            writer.Formatting = Formatting.Indented;
            androidManifestXmlDocument.Save(writer);
        }

        public void Dispose()
        {
            Save();
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value) {
            XmlAttribute attr = androidManifestXmlDocument.CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }

        private XmlNode GetActivityWithLaunchIntent() {
            return androidManifestXmlDocument.SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                                                              "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nsMgr);
        }

        public void SetApplicationTheme(string appTheme) {
            // applicationElement.Attributes.Append(CreateAndroidAttribute("theme", appTheme));
            SetApplicationAttribute("theme", appTheme);
        }

        public void SetApplicationAttribute(string key, string value) {
            applicationElement.Attributes.Append(CreateAndroidAttribute(key, value));
        }

        public void SetStartingActivityName(string activityName) {
            GetActivityWithLaunchIntent().Attributes!.Append(CreateAndroidAttribute("name", activityName));
        }


        public void SetHardwareAcceleration() {
            GetActivityWithLaunchIntent().Attributes!.Append(CreateAndroidAttribute("hardwareAccelerated", "true"));
        }

        public void SetBillingPermission(int maxSdkVersion=-1)  => SetPermissionAttribute("BILLING", maxSdkVersion);

        public void SetVibratePermission(int maxSdkVersion=-1) => SetPermissionAttribute("VIBRATE", maxSdkVersion);

        public void SetPermissionAttribute(string value, int maxSdkVersion=-1)
        {
            XmlNode manifest = androidManifestXmlDocument.SelectSingleNode("/manifest");
            XmlElement child = androidManifestXmlDocument.CreateElement("uses-permission");
            manifest!.AppendChild(child);

            XmlAttribute newAttribute = CreateAndroidAttribute("name", $"android.permission.{value}");
            child.Attributes.Append(newAttribute);

            if (maxSdkVersion != -1)
            {
                XmlAttribute sdkVersionAttribute = CreateAndroidAttribute("maxSdkVersion", $"{maxSdkVersion}");
                child.Attributes.Append(sdkVersionAttribute);
            }

            Debug.Log($"{value} {maxSdkVersion}");
        }
    }
}
#endif
