#if UNITY_ANDROID
using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace SaintsBuild.Editor
{
    public class AndroidManifest: IDisposable
    {
        private const string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public readonly string path;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public readonly XmlNamespaceManager nsMgr;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public readonly XmlDocument androidManifestXmlDocument;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public readonly XmlElement applicationElement;

        public AndroidManifest(string basePath)
        {
            path = Path.Combine(new[] { basePath, "src", "main", "AndroidManifest.xml" });
            if (!File.Exists(path))
            {
                path = Path.Combine(new[] { basePath, "unityLibrary", "src", "main", "AndroidManifest.xml" });
            }
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

        public void SetApplicationAttribute(string key, string value)
        {
            SetOrReplaceAttribute(applicationElement.Attributes, key, value);
        }

        public void SetOrReplaceAttribute(XmlAttributeCollection attributes, string key, string value)
        {
            XmlAttribute existing = attributes[key, AndroidXmlNamespace];
            if (existing != null)
            {
                Debug.Log($"set {existing.Name} with {key}={value}");
                existing.Value = value; // replace value
            }
            else
            {
                Debug.Log($"add {attributes} with {key}={value}");
                attributes.Append(CreateAndroidAttribute(key, value));
            }
        }

        public void SetStartingActivityName(string activityName) {
            SetOrReplaceAttribute(GetActivityWithLaunchIntent().Attributes!,"name", activityName);
            // GetActivityWithLaunchIntent().Attributes!.Append(CreateAndroidAttribute("name", activityName));
        }


        public void SetHardwareAcceleration() => SetActivityWithLauncherIntentAttribute("hardwareAccelerated", "true");

        public void SetActivityWithLauncherIntentAttribute(string key, string value) {
            SetOrReplaceAttribute(GetActivityWithLaunchIntent().Attributes!, key, value);
            // GetActivityWithLaunchIntent().Attributes!.Append(CreateAndroidAttribute(key, value));
        }

        public void SetBillingPermission(int maxSdkVersion=-1)  => SetPermissionAttribute("BILLING", maxSdkVersion);

        public void SetVibratePermission(int maxSdkVersion=-1) => SetPermissionAttribute("VIBRATE", maxSdkVersion);

        public void SetPermissionAttribute(string value, int maxSdkVersion=-1)
        {
            // XmlNode manifest = androidManifestXmlDocument.SelectSingleNode("/manifest");
            // XmlElement child = androidManifestXmlDocument.CreateElement("uses-permission");
            // manifest!.AppendChild(child);
            //
            // XmlAttribute newAttribute = CreateAndroidAttribute("name", $"android.permission.{value}");
            // child.Attributes.Append(newAttribute);
            //
            // if (maxSdkVersion != -1)
            // {
            //     XmlAttribute sdkVersionAttribute = CreateAndroidAttribute("maxSdkVersion", $"{maxSdkVersion}");
            //     child.Attributes.Append(sdkVersionAttribute);
            // }
            //
            // Debug.Log($"{value} {maxSdkVersion}");

            XmlElement manifest = (XmlElement)androidManifestXmlDocument.SelectSingleNode("/manifest")!;

            string permissionName = $"android.permission.{value}";

            XmlElement permissionElement = null;

            foreach (XmlNode node in manifest.ChildNodes)
            {
                // ReSharper disable once MergeIntoPattern
                // ReSharper disable once InvertIf
                if (node is XmlElement el &&
                    el.Name == "uses-permission" &&
                    el.GetAttribute("name", AndroidXmlNamespace) == permissionName)
                {
                    permissionElement = el;
                    break;
                }
            }

            if (permissionElement == null)
            {
                permissionElement = androidManifestXmlDocument.CreateElement("uses-permission");
                manifest.AppendChild(permissionElement);
            }

            XmlAttribute nameAttr =
                permissionElement.Attributes["name", AndroidXmlNamespace]
                ?? CreateAndroidAttribute("name", permissionName);

            nameAttr.Value = permissionName;

            if (nameAttr.OwnerElement == null)
            {
                permissionElement.Attributes.Append(nameAttr);
            }

            if (maxSdkVersion != -1)
            {
                XmlAttribute sdkAttr =
                    permissionElement.Attributes["maxSdkVersion", AndroidXmlNamespace]
                    ?? CreateAndroidAttribute("maxSdkVersion", maxSdkVersion.ToString());

                sdkAttr.Value = maxSdkVersion.ToString();

                if (sdkAttr.OwnerElement == null)
                    permissionElement.Attributes.Append(sdkAttr);
            }
            else
            {
                XmlAttribute oldSdkAttr =
                    permissionElement.Attributes["maxSdkVersion", AndroidXmlNamespace];

                if (oldSdkAttr != null)
                {
                    permissionElement.Attributes.Remove(oldSdkAttr);
                }
            }

            Debug.Log($"Permission set: {permissionName}, maxSdkVersion={maxSdkVersion}");
        }
    }
}
#endif
