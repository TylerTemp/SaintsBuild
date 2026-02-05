#if UNITY_ANDROID
using System;
using System.IO;
using System.Xml;

namespace SaintsBuild.Editor
{
    public class AndroidValues
    {
        private readonly string _resFolder;

        public AndroidValues(string pathToBuiltProject)
        {
            _resFolder = Path.Combine(
                pathToBuiltProject,
                "unityLibrary/src/main/res");
        }

        public AndroidValue CreateOrGetValue(string name)
        {
            return new AndroidValue(GetValuePath(name));
        }

        public string GetValuePath(string name) => Path.Combine(_resFolder, name);
    }

    public class AndroidValue: IDisposable
    {
        private readonly string _filePath;
        private readonly XmlDocument _doc;
        private readonly XmlElement _resourcesNode;
        private bool _dirty;

        public AndroidValue(string filePath)
        {
            _filePath = filePath;
            DirectoryInfo folder = Directory.GetParent(_filePath)!;
            if (!folder.Exists)
            {
                Directory.CreateDirectory(folder.FullName);
            }
            _doc = new XmlDocument();

            if (File.Exists(_filePath))
            {
                _doc.Load(_filePath);

                _resourcesNode = _doc.DocumentElement;
                if (_resourcesNode == null || _resourcesNode.Name != "resources")
                    throw new InvalidOperationException("Invalid Android strings.xml format");
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

                XmlDeclaration declaration = _doc.CreateXmlDeclaration("1.0", "utf-8", null);
                _doc.AppendChild(declaration);

                _resourcesNode = _doc.CreateElement("resources");
                _doc.AppendChild(_resourcesNode);

                _dirty = true;
            }
        }

        public void SetString(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            XmlElement stringNode = null;

            foreach (XmlNode node in _resourcesNode.ChildNodes)
            {
                // ReSharper disable once InvertIf
                // ReSharper disable once MergeIntoPattern
                if (node is XmlElement el &&
                    el.Name == "string" &&
                    el.GetAttribute("name") == name)
                {
                    stringNode = el;
                    break;
                }
            }

            if (stringNode == null)
            {
                stringNode = _doc.CreateElement("string");
                stringNode.SetAttribute("name", name);
                stringNode.InnerText = value;
                _resourcesNode.AppendChild(stringNode);
            }
            else
            {
                stringNode.InnerText = value;
            }

            _dirty = true;
        }

        private void Save()
        {
            if (!_dirty)
                return;

            using XmlTextWriter writer = new XmlTextWriter(_filePath, System.Text.Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            _doc.Save(writer);
            _dirty = false;
        }

        public void Dispose()
        {
            Save();
        }
    }
}
#endif
